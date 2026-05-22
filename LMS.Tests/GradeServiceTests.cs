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

        Assert.Equal(95, result.Score);
        Assert.Equal("Great work!", result.Feedback);
        Assert.Equal("John", result.GradedByName);
    }

    [Fact]
    public async Task GradeSubmissionAsync_ThrowsForWrongInstructor()
    {
        var instructor = _db.CreateUser("inst-1", "John", "john@test.com");
        _db.CreateUser("inst-2", "Other", "other@test.com");
        var student = _db.CreateUser("stu-1", "Jane", "jane@test.com");
        var course = _db.CreateCourse(instructor.Id);
        var assignment = _db.CreateAssignment(course.Id);
        var submission = _db.CreateSubmission(student.Id, assignment.Id);

        var request = new GradeSubmissionRequest { Score = 50 };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _sut.GradeSubmissionAsync(submission.Id, "inst-2", false, request));
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

        Assert.Equal(88, result.Score);
    }

    [Fact]
    public async Task GradeSubmissionAsync_ThrowsIfAlreadyGraded()
    {
        var instructor = _db.CreateUser("inst-1", "John", "john@test.com");
        var student = _db.CreateUser("stu-1", "Jane", "jane@test.com");
        var course = _db.CreateCourse(instructor.Id);
        var assignment = _db.CreateAssignment(course.Id);
        var submission = _db.CreateSubmission(student.Id, assignment.Id);

        var request = new GradeSubmissionRequest { Score = 90 };
        await _sut.GradeSubmissionAsync(submission.Id, instructor.Id, false, request);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.GradeSubmissionAsync(submission.Id, instructor.Id, false, request));
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

        var updated = await _sut.UpdateGradeAsync(submission.Id, instructor.Id, false,
            new GradeSubmissionRequest { Score = 85, Feedback = "Regraded" });

        Assert.Equal(85, updated.Score);
        Assert.Equal("Regraded", updated.Feedback);
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

        Assert.Single(result);
        Assert.NotNull(result[0].Grade);
        Assert.Equal(92, result[0].Grade!.Score);
    }

    public void Dispose() => _db.Dispose();
}
