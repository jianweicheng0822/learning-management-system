using LMS.DTOs;
using LMS.Services;

namespace LMS.Tests;

public class AssignmentServiceTests : IDisposable
{
    private readonly TestDbHelper _db = new();
    private readonly AssignmentService _sut;

    public AssignmentServiceTests()
    {
        _sut = new AssignmentService(_db.Context);
    }

    [Fact]
    public async Task CreateAsync_ReturnsAssignmentDto()
    {
        var instructor = _db.CreateUser("inst-1", "John", "john@test.com");
        var course = _db.CreateCourse(instructor.Id);
        var request = new CreateAssignmentRequest
        {
            Title = "HW1",
            Description = "Do the homework",
            DueDate = DateTime.UtcNow.AddDays(7)
        };

        var result = await _sut.CreateAsync(course.Id, instructor.Id, false, request);

        Assert.Equal("HW1", result.Title);
        Assert.Equal(course.Id, result.CourseId);
        Assert.Equal(course.Title, result.CourseName);
    }

    [Fact]
    public async Task CreateAsync_ThrowsForWrongInstructor()
    {
        var instructor = _db.CreateUser("inst-1", "John", "john@test.com");
        _db.CreateUser("inst-2", "Other", "other@test.com");
        var course = _db.CreateCourse(instructor.Id);
        var request = new CreateAssignmentRequest
        {
            Title = "HW1",
            Description = "Desc",
            DueDate = DateTime.UtcNow.AddDays(7)
        };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _sut.CreateAsync(course.Id, "inst-2", false, request));
    }

    [Fact]
    public async Task CreateAsync_AdminCanCreateForAnyCourse()
    {
        var instructor = _db.CreateUser("inst-1", "John", "john@test.com");
        var admin = _db.CreateUser("admin-1", "Admin", "admin@test.com");
        var course = _db.CreateCourse(instructor.Id);
        var request = new CreateAssignmentRequest
        {
            Title = "Admin HW",
            Description = "Desc",
            DueDate = DateTime.UtcNow.AddDays(7)
        };

        var result = await _sut.CreateAsync(course.Id, admin.Id, true, request);

        Assert.Equal("Admin HW", result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsForNonExistent()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.GetByIdAsync(999));
    }

    [Fact]
    public async Task GetByCourseAsync_ReturnsAssignments()
    {
        var instructor = _db.CreateUser("inst-1", "John", "john@test.com");
        var course = _db.CreateCourse(instructor.Id);
        _db.CreateAssignment(course.Id, "HW1");
        _db.CreateAssignment(course.Id, "HW2");

        var result = await _sut.GetByCourseAsync(course.Id);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByCourseAsync_ThrowsForNonExistentCourse()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.GetByCourseAsync(999));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesAssignment()
    {
        var instructor = _db.CreateUser("inst-1", "John", "john@test.com");
        var course = _db.CreateCourse(instructor.Id);
        var assignment = _db.CreateAssignment(course.Id, "Old Title");
        var newDue = DateTime.UtcNow.AddDays(14);
        var request = new UpdateAssignmentRequest
        {
            Title = "New Title",
            Description = "New Desc",
            DueDate = newDue
        };

        var result = await _sut.UpdateAsync(assignment.Id, instructor.Id, false, request);

        Assert.Equal("New Title", result.Title);
        Assert.Equal("New Desc", result.Description);
    }

    [Fact]
    public async Task DeleteAsync_RemovesAssignment()
    {
        var instructor = _db.CreateUser("inst-1", "John", "john@test.com");
        var course = _db.CreateCourse(instructor.Id);
        var assignment = _db.CreateAssignment(course.Id);

        await _sut.DeleteAsync(assignment.Id, instructor.Id, false);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.GetByIdAsync(assignment.Id));
    }

    public void Dispose() => _db.Dispose();
}
