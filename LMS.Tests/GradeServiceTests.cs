using LMS.DTOs;
using LMS.Services;

namespace LMS.Tests;

public class GradeServiceTests : IDisposable
{
    private readonly TestDbHelper _db = new();
    private readonly GradeService _sut;

    public GradeServiceTests()
    {
        _sut = new GradeService(_db.Context);
    }

    [Fact]
    public async Task GradeSubmissionAsync_CreatesGrade()
    {
        var instructor = _db.CreateUser("inst-1", "John", "john@test.com");
        var student = _db.CreateUser("stu-1", "Jane", "jane@test.com");
        var course = _db.CreateCourse(instructor.Id);
        var assignment = _db.CreateAssignment(course.Id);
        var submission = _db.CreateSubmission(student.Id, assignment.Id);

        var request = new GradeSubmissionRequest { Score = 95, Feedback = "Great work!" };
        var result = await _sut.GradeSubmissionAsync(submission.Id, instructor.Id, false, request);

        Assert.True(result.IsSuccess);
        Assert.Equal(95, result.Value!.Score);
        Assert.Equal("Great work!", result.Value.Feedback);
        Assert.Equal("John", result.Value.GradedByName);
    }

    [Fact]
    public async Task GradeSubmissionAsync_ReturnsUnauthorizedForWrongInstructor()
    {
        var instructor = _db.CreateUser("inst-1", "John", "john@test.com");
        _db.CreateUser("inst-2", "Other", "other@test.com");
        var student = _db.CreateUser("stu-1", "Jane", "jane@test.com");
        var course = _db.CreateCourse(instructor.Id);
        var assignment = _db.CreateAssignment(course.Id);
        var submission = _db.CreateSubmission(student.Id, assignment.Id);

        var request = new GradeSubmissionRequest { Score = 50 };
        var result = await _sut.GradeSubmissionAsync(submission.Id, "inst-2", false, request);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unauthorized, result.Error);
    }

    [Fact]
    public async Task GradeSubmissionAsync_AdminCanGradeAnyCourse()
    {
        var instructor = _db.CreateUser("inst-1", "John", "john@test.com");
        var admin = _db.CreateUser("admin-1", "Admin", "admin@test.com");
        var student = _db.CreateUser("stu-1", "Jane", "jane@test.com");
        var course = _db.CreateCourse(instructor.Id);
        var assignment = _db.CreateAssignment(course.Id);
        var submission = _db.CreateSubmission(student.Id, assignment.Id);

        var request = new GradeSubmissionRequest { Score = 88, Feedback = "Good" };
        var result = await _sut.GradeSubmissionAsync(submission.Id, admin.Id, true, request);

        Assert.True(result.IsSuccess);
        Assert.Equal(88, result.Value!.Score);
    }

    [Fact]
    public async Task GradeSubmissionAsync_ReturnsConflictIfAlreadyGraded()
    {
        var instructor = _db.CreateUser("inst-1", "John", "john@test.com");
        var student = _db.CreateUser("stu-1", "Jane", "jane@test.com");
        var course = _db.CreateCourse(instructor.Id);
        var assignment = _db.CreateAssignment(course.Id);
        var submission = _db.CreateSubmission(student.Id, assignment.Id);

        var request = new GradeSubmissionRequest { Score = 90 };
        await _sut.GradeSubmissionAsync(submission.Id, instructor.Id, false, request);

        var result = await _sut.GradeSubmissionAsync(submission.Id, instructor.Id, false, request);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error);
    }

    [Fact]
    public async Task UpdateGradeAsync_UpdatesExistingGrade()
    {
        var instructor = _db.CreateUser("inst-1", "John", "john@test.com");
        var student = _db.CreateUser("stu-1", "Jane", "jane@test.com");
        var course = _db.CreateCourse(instructor.Id);
        var assignment = _db.CreateAssignment(course.Id);
        var submission = _db.CreateSubmission(student.Id, assignment.Id);

        await _sut.GradeSubmissionAsync(submission.Id, instructor.Id, false,
            new GradeSubmissionRequest { Score = 70, Feedback = "Needs work" });

        var result = await _sut.UpdateGradeAsync(submission.Id, instructor.Id, false,
            new GradeSubmissionRequest { Score = 85, Feedback = "Regraded" });

        Assert.True(result.IsSuccess);
        Assert.Equal(85, result.Value!.Score);
        Assert.Equal("Regraded", result.Value.Feedback);
    }

    [Fact]
    public async Task GetGradesForStudentCourseAsync_ReturnsGradedSubmissions()
    {
        var instructor = _db.CreateUser("inst-1", "John", "john@test.com");
        var student = _db.CreateUser("stu-1", "Jane", "jane@test.com");
        var course = _db.CreateCourse(instructor.Id);
        var assignment = _db.CreateAssignment(course.Id);
        var submission = _db.CreateSubmission(student.Id, assignment.Id);

        await _sut.GradeSubmissionAsync(submission.Id, instructor.Id, false,
            new GradeSubmissionRequest { Score = 92, Feedback = "Excellent" });

        var result = await _sut.GetGradesForStudentCourseAsync(student.Id, course.Id);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Items);
        Assert.NotNull(result.Value.Items[0].Grade);
        Assert.Equal(92, result.Value.Items[0].Grade!.Score);
    }

    [Fact]
    public async Task GetAverageScoreAsync_ReturnsAverage()
    {
        var instructor = _db.CreateUser("inst-1", "John", "john@test.com");
        var student = _db.CreateUser("stu-1", "Jane", "jane@test.com");
        var course = _db.CreateCourse(instructor.Id);

        var a1 = _db.CreateAssignment(course.Id, "HW1");
        var a2 = _db.CreateAssignment(course.Id, "HW2");
        var s1 = _db.CreateSubmission(student.Id, a1.Id);
        var s2 = _db.CreateSubmission(student.Id, a2.Id);

        await _sut.GradeSubmissionAsync(s1.Id, instructor.Id, false,
            new GradeSubmissionRequest { Score = 80 });
        await _sut.GradeSubmissionAsync(s2.Id, instructor.Id, false,
            new GradeSubmissionRequest { Score = 90 });

        var average = await _sut.GetAverageScoreAsync(student.Id, course.Id);

        Assert.NotNull(average);
        Assert.Equal(85m, average.Value);
    }

    [Fact]
    public async Task GetAverageScoreAsync_ReturnsNullWhenNoGrades()
    {
        var instructor = _db.CreateUser("inst-1", "John", "john@test.com");
        var student = _db.CreateUser("stu-1", "Jane", "jane@test.com");
        var course = _db.CreateCourse(instructor.Id);

        var average = await _sut.GetAverageScoreAsync(student.Id, course.Id);

        Assert.Null(average);
    }

    [Fact]
    public async Task GetGradesForStudentCourseAsync_PaginatesCorrectly()
    {
        var instructor = _db.CreateUser("inst-1", "John", "john@test.com");
        var student = _db.CreateUser("stu-1", "Jane", "jane@test.com");
        var course = _db.CreateCourse(instructor.Id);

        for (int i = 1; i <= 4; i++)
        {
            var assignment = _db.CreateAssignment(course.Id, $"HW{i}");
            var submission = _db.CreateSubmission(student.Id, assignment.Id);
            await _sut.GradeSubmissionAsync(submission.Id, instructor.Id, false,
                new GradeSubmissionRequest { Score = 80 + i });
        }

        var result = await _sut.GetGradesForStudentCourseAsync(student.Id, course.Id, page: 1, pageSize: 2);

        Assert.Equal(4, result.Value!.TotalCount);
        Assert.Equal(2, result.Value.Items.Count);
        Assert.True(result.Value.HasNextPage);
    }

    public void Dispose() => _db.Dispose();
}
