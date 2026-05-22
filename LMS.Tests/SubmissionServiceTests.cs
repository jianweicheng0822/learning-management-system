using LMS.DTOs;
using LMS.Services;

namespace LMS.Tests;

public class SubmissionServiceTests : IDisposable
{
    private readonly TestDbHelper _db = new();
    private readonly SubmissionService _sut;

    public SubmissionServiceTests()
    {
        _sut = new SubmissionService(_db.Context);
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

        Assert.Equal("My answer", result.TextContent);
        Assert.Equal(student.Id, result.StudentId);
        Assert.Equal(assignment.Id, result.AssignmentId);
    }

    [Fact]
    public async Task SubmitAsync_ThrowsIfNotEnrolled()
    {
        var instructor = _db.CreateUser("inst-1", "John", "john@test.com");
        var student = _db.CreateUser("stu-1", "Jane", "jane@test.com");
        var course = _db.CreateCourse(instructor.Id);
        var assignment = _db.CreateAssignment(course.Id);

        var request = new CreateSubmissionRequest { TextContent = "My answer" };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _sut.SubmitAsync(assignment.Id, student.Id, request));
    }

    [Fact]
    public async Task SubmitAsync_ThrowsIfAlreadySubmitted()
    {
        var instructor = _db.CreateUser("inst-1", "John", "john@test.com");
        var student = _db.CreateUser("stu-1", "Jane", "jane@test.com");
        var course = _db.CreateCourse(instructor.Id);
        _db.CreateEnrollment(student.Id, course.Id);
        var assignment = _db.CreateAssignment(course.Id);
        _db.CreateSubmission(student.Id, assignment.Id);

        var request = new CreateSubmissionRequest { TextContent = "Second attempt" };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.SubmitAsync(assignment.Id, student.Id, request));
    }

    [Fact]
    public async Task SubmitAsync_ThrowsIfNoContent()
    {
        var instructor = _db.CreateUser("inst-1", "John", "john@test.com");
        var student = _db.CreateUser("stu-1", "Jane", "jane@test.com");
        var course = _db.CreateCourse(instructor.Id);
        _db.CreateEnrollment(student.Id, course.Id);
        var assignment = _db.CreateAssignment(course.Id);

        var request = new CreateSubmissionRequest();

        await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.SubmitAsync(assignment.Id, student.Id, request));
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

        Assert.Equal(2, result.Count);
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

        Assert.Single(result);
        Assert.Equal(student1.Id, result[0].StudentId);
    }

    public void Dispose() => _db.Dispose();
}
