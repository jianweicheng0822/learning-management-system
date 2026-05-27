using LMS.DTOs;
using LMS.Services;

namespace LMS.Tests;

public class FakeFileStorageService : IFileStorageService
{
    public Task<string> UploadAsync(Stream fileStream, string key, string contentType) => Task.FromResult(key);
    public string GetDownloadUrl(string key, string originalFileName, TimeSpan? expiry = null) => $"https://s3.example.com/{key}?name={originalFileName}";
    public Task DeleteAsync(string key) => Task.CompletedTask;
}

public class SubmissionServiceTests : IDisposable
{
    private readonly TestDbHelper _db = new();
    private readonly SubmissionService _sut;

    public SubmissionServiceTests()
    {
        _sut = new SubmissionService(_db.Context, new FakeFileStorageService());
    }

    [Fact]
    public async Task SubmitAsync_CreatesSubmission()
    {
        var instructor = _db.CreateUser("inst-1", "John", "john@test.com");
        var student = _db.CreateUser("stu-1", "Jane", "jane@test.com");
        var course = _db.CreateCourse(instructor.Id);
        _db.CreateEnrollment(student.Id, course.Id);
        var assignment = _db.CreateAssignment(course.Id);

        var request = new CreateSubmissionRequest { TextContent = "My answer" };
        var result = await _sut.SubmitAsync(assignment.Id, student.Id, request);

        Assert.True(result.IsSuccess);
        Assert.Equal("My answer", result.Value!.TextContent);
        Assert.Equal(student.Id, result.Value.StudentId);
        Assert.Equal(assignment.Id, result.Value.AssignmentId);
    }

    [Fact]
    public async Task SubmitAsync_ReturnsUnauthorizedIfNotEnrolled()
    {
        var instructor = _db.CreateUser("inst-1", "John", "john@test.com");
        var student = _db.CreateUser("stu-1", "Jane", "jane@test.com");
        var course = _db.CreateCourse(instructor.Id);
        var assignment = _db.CreateAssignment(course.Id);

        var request = new CreateSubmissionRequest { TextContent = "My answer" };
        var result = await _sut.SubmitAsync(assignment.Id, student.Id, request);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unauthorized, result.Error);
    }

    [Fact]
    public async Task SubmitAsync_ReturnsConflictIfAlreadySubmitted()
    {
        var instructor = _db.CreateUser("inst-1", "John", "john@test.com");
        var student = _db.CreateUser("stu-1", "Jane", "jane@test.com");
        var course = _db.CreateCourse(instructor.Id);
        _db.CreateEnrollment(student.Id, course.Id);
        var assignment = _db.CreateAssignment(course.Id);
        _db.CreateSubmission(student.Id, assignment.Id);

        var request = new CreateSubmissionRequest { TextContent = "Second attempt" };
        var result = await _sut.SubmitAsync(assignment.Id, student.Id, request);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error);
    }

    [Fact]
    public async Task SubmitAsync_ReturnsValidationErrorIfNoContent()
    {
        var instructor = _db.CreateUser("inst-1", "John", "john@test.com");
        var student = _db.CreateUser("stu-1", "Jane", "jane@test.com");
        var course = _db.CreateCourse(instructor.Id);
        _db.CreateEnrollment(student.Id, course.Id);
        var assignment = _db.CreateAssignment(course.Id);

        var request = new CreateSubmissionRequest();
        var result = await _sut.SubmitAsync(assignment.Id, student.Id, request);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.ValidationError, result.Error);
    }

    [Fact]
    public async Task SubmitAsync_WithFile_StoresS3KeyAndOriginalFileName()
    {
        var instructor = _db.CreateUser("inst-1", "John", "john@test.com");
        var student = _db.CreateUser("stu-1", "Jane", "jane@test.com");
        var course = _db.CreateCourse(instructor.Id);
        _db.CreateEnrollment(student.Id, course.Id);
        var assignment = _db.CreateAssignment(course.Id);

        var request = new CreateSubmissionRequest { TextContent = "See attached" };
        var result = await _sut.SubmitAsync(assignment.Id, student.Id, request,
            s3Key: "courses/1/assignments/1/students/stu-1/abc.pdf",
            originalFileName: "homework.pdf");

        Assert.True(result.IsSuccess);
        Assert.Equal("courses/1/assignments/1/students/stu-1/abc.pdf", result.Value!.FilePath);
        Assert.Equal("homework.pdf", result.Value.OriginalFileName);
    }

    [Fact]
    public async Task GetDownloadUrlAsync_ReturnsUrl_ForOwnSubmission()
    {
        var instructor = _db.CreateUser("inst-1", "John", "john@test.com");
        var student = _db.CreateUser("stu-1", "Jane", "jane@test.com");
        var course = _db.CreateCourse(instructor.Id);
        _db.CreateEnrollment(student.Id, course.Id);
        var assignment = _db.CreateAssignment(course.Id);
        _db.CreateSubmission(student.Id, assignment.Id, filePath: "s3/key.pdf", originalFileName: "hw.pdf");

        var submission = _db.Context.Submissions.First();
        var result = await _sut.GetDownloadUrlAsync(submission.Id, student.Id, isInstructorOrAdmin: false);

        Assert.True(result.IsSuccess);
        Assert.Contains("s3/key.pdf", result.Value!);
    }

    [Fact]
    public async Task GetDownloadUrlAsync_ReturnsUnauthorized_ForOtherStudent()
    {
        var instructor = _db.CreateUser("inst-1", "John", "john@test.com");
        var student1 = _db.CreateUser("stu-1", "Jane", "jane@test.com");
        var student2 = _db.CreateUser("stu-2", "Bob", "bob@test.com");
        var course = _db.CreateCourse(instructor.Id);
        _db.CreateEnrollment(student1.Id, course.Id);
        var assignment = _db.CreateAssignment(course.Id);
        _db.CreateSubmission(student1.Id, assignment.Id, filePath: "s3/key.pdf", originalFileName: "hw.pdf");

        var submission = _db.Context.Submissions.First();
        var result = await _sut.GetDownloadUrlAsync(submission.Id, student2.Id, isInstructorOrAdmin: false);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unauthorized, result.Error);
    }

    [Fact]
    public async Task GetDownloadUrlAsync_ReturnsNotFound_WhenNoFile()
    {
        var instructor = _db.CreateUser("inst-1", "John", "john@test.com");
        var student = _db.CreateUser("stu-1", "Jane", "jane@test.com");
        var course = _db.CreateCourse(instructor.Id);
        _db.CreateEnrollment(student.Id, course.Id);
        var assignment = _db.CreateAssignment(course.Id);
        _db.CreateSubmission(student.Id, assignment.Id);

        var submission = _db.Context.Submissions.First();
        var result = await _sut.GetDownloadUrlAsync(submission.Id, student.Id, isInstructorOrAdmin: false);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error);
    }

    [Fact]
    public async Task GetByAssignmentAsync_ReturnsSubmissions()
    {
        var instructor = _db.CreateUser("inst-1", "John", "john@test.com");
        var student1 = _db.CreateUser("stu-1", "Jane", "jane@test.com");
        var student2 = _db.CreateUser("stu-2", "Bob", "bob@test.com");
        var course = _db.CreateCourse(instructor.Id);
        _db.CreateEnrollment(student1.Id, course.Id);
        _db.CreateEnrollment(student2.Id, course.Id);
        var assignment = _db.CreateAssignment(course.Id);
        _db.CreateSubmission(student1.Id, assignment.Id);
        _db.CreateSubmission(student2.Id, assignment.Id);

        var result = await _sut.GetByAssignmentAsync(assignment.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.TotalCount);
    }

    [Fact]
    public async Task GetByStudentAsync_ReturnsOnlyStudentSubmissions()
    {
        var instructor = _db.CreateUser("inst-1", "John", "john@test.com");
        var student1 = _db.CreateUser("stu-1", "Jane", "jane@test.com");
        var student2 = _db.CreateUser("stu-2", "Bob", "bob@test.com");
        var course = _db.CreateCourse(instructor.Id);
        _db.CreateEnrollment(student1.Id, course.Id);
        _db.CreateEnrollment(student2.Id, course.Id);
        var assignment = _db.CreateAssignment(course.Id);
        _db.CreateSubmission(student1.Id, assignment.Id);
        _db.CreateSubmission(student2.Id, assignment.Id);

        var result = await _sut.GetByStudentAsync(student1.Id);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Items);
        Assert.Equal(student1.Id, result.Value.Items[0].StudentId);
    }

    [Fact]
    public async Task GetByAssignmentAsync_PaginatesCorrectly()
    {
        var instructor = _db.CreateUser("inst-1", "John", "john@test.com");
        var course = _db.CreateCourse(instructor.Id);
        var assignment = _db.CreateAssignment(course.Id);

        for (int i = 1; i <= 5; i++)
        {
            var student = _db.CreateUser($"stu-{i}", $"Student {i}", $"stu{i}@test.com");
            _db.CreateEnrollment(student.Id, course.Id);
            _db.CreateSubmission(student.Id, assignment.Id);
        }

        var page1 = await _sut.GetByAssignmentAsync(assignment.Id, page: 1, pageSize: 2);
        var page2 = await _sut.GetByAssignmentAsync(assignment.Id, page: 2, pageSize: 2);

        Assert.Equal(5, page1.Value!.TotalCount);
        Assert.Equal(2, page1.Value.Items.Count);
        Assert.True(page1.Value.HasNextPage);

        Assert.Equal(2, page2.Value!.Items.Count);
        Assert.True(page2.Value.HasPreviousPage);
    }

    [Fact]
    public async Task GetByStudentAsync_PaginatesCorrectly()
    {
        var instructor = _db.CreateUser("inst-1", "John", "john@test.com");
        var student = _db.CreateUser("stu-1", "Jane", "jane@test.com");
        var course = _db.CreateCourse(instructor.Id);
        _db.CreateEnrollment(student.Id, course.Id);

        for (int i = 1; i <= 4; i++)
        {
            var assignment = _db.CreateAssignment(course.Id, $"HW{i}");
            _db.CreateSubmission(student.Id, assignment.Id);
        }

        var result = await _sut.GetByStudentAsync(student.Id, page: 1, pageSize: 2);

        Assert.Equal(4, result.Value!.TotalCount);
        Assert.Equal(2, result.Value.Items.Count);
        Assert.True(result.Value.HasNextPage);
    }

    public void Dispose() => _db.Dispose();
}
