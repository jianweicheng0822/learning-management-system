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

        Assert.True(result.IsSuccess);
        Assert.Equal("HW1", result.Value!.Title);
        Assert.Equal(course.Id, result.Value.CourseId);
        Assert.Equal(course.Title, result.Value.CourseName);
    }

    [Fact]
    public async Task CreateAsync_ReturnsUnauthorizedForWrongInstructor()
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

        var result = await _sut.CreateAsync(course.Id, "inst-2", false, request);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unauthorized, result.Error);
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

        Assert.True(result.IsSuccess);
        Assert.Equal("Admin HW", result.Value!.Title);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNotFoundForNonExistent()
    {
        var result = await _sut.GetByIdAsync(999);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error);
    }

    [Fact]
    public async Task GetByCourseAsync_ReturnsAssignments()
    {
        var instructor = _db.CreateUser("inst-1", "John", "john@test.com");
        var course = _db.CreateCourse(instructor.Id);
        _db.CreateAssignment(course.Id, "HW1");
        _db.CreateAssignment(course.Id, "HW2");

        var result = await _sut.GetByCourseAsync(course.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.TotalCount);
    }

    [Fact]
    public async Task GetByCourseAsync_ReturnsNotFoundForNonExistentCourse()
    {
        var result = await _sut.GetByCourseAsync(999);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error);
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

        Assert.True(result.IsSuccess);
        Assert.Equal("New Title", result.Value!.Title);
        Assert.Equal("New Desc", result.Value.Description);
    }

    [Fact]
    public async Task DeleteAsync_RemovesAssignment()
    {
        var instructor = _db.CreateUser("inst-1", "John", "john@test.com");
        var course = _db.CreateCourse(instructor.Id);
        var assignment = _db.CreateAssignment(course.Id);

        var result = await _sut.DeleteAsync(assignment.Id, instructor.Id, false);

        Assert.True(result.IsSuccess);
        var getResult = await _sut.GetByIdAsync(assignment.Id);
        Assert.False(getResult.IsSuccess);
        Assert.Equal(ErrorType.NotFound, getResult.Error);
    }

    [Fact]
    public async Task GetByCourseAsync_PaginatesCorrectly()
    {
        var instructor = _db.CreateUser("inst-1", "John", "john@test.com");
        var course = _db.CreateCourse(instructor.Id);
        for (int i = 1; i <= 5; i++)
            _db.CreateAssignment(course.Id, $"HW{i}");

        var page1 = await _sut.GetByCourseAsync(course.Id, page: 1, pageSize: 2);
        var page2 = await _sut.GetByCourseAsync(course.Id, page: 2, pageSize: 2);

        Assert.Equal(5, page1.Value!.TotalCount);
        Assert.Equal(2, page1.Value.Items.Count);
        Assert.True(page1.Value.HasNextPage);

        Assert.Equal(2, page2.Value!.Items.Count);
        Assert.True(page2.Value.HasPreviousPage);
    }

    public void Dispose() => _db.Dispose();
}
