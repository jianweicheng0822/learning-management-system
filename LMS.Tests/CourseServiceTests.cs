using LMS.DTOs;
using LMS.Services;

namespace LMS.Tests;

public class CourseServiceTests : IDisposable
{
    private readonly TestDbHelper _db = new();
    private readonly CourseService _sut;

    public CourseServiceTests()
    {
        _sut = new CourseService(_db.Context);
    }

    [Fact]
    public async Task CreateAsync_ReturnsCourseDto()
    {
        var instructor = _db.CreateUser("inst-1", "John Doe", "john@test.com");
        var request = new CreateCourseRequest { Title = "C# 101", Description = "Intro to C#" };

        var result = await _sut.CreateAsync(instructor.Id, request);

        Assert.True(result.IsSuccess);
        Assert.Equal("C# 101", result.Value!.Title);
        Assert.Equal("John Doe", result.Value.InstructorName);
        Assert.Equal(0, result.Value.EnrolledCount);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllCourses()
    {
        var instructor = _db.CreateUser("inst-1", "John Doe", "john@test.com");
        _db.CreateCourse(instructor.Id, "Course A");
        _db.CreateCourse(instructor.Id, "Course B");

        var result = await _sut.GetAllAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.TotalCount);
        Assert.Equal(2, result.Value.Items.Count);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDetailDto()
    {
        var instructor = _db.CreateUser("inst-1", "John Doe", "john@test.com");
        var student = _db.CreateUser("stu-1", "Jane Smith", "jane@test.com");
        var course = _db.CreateCourse(instructor.Id);
        _db.CreateEnrollment(student.Id, course.Id);
        _db.CreateAssignment(course.Id, "HW1");

        var result = await _sut.GetByIdAsync(course.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(course.Id, result.Value!.Id);
        Assert.Single(result.Value.EnrolledStudents);
        Assert.Single(result.Value.Assignments);
        Assert.Equal("Jane Smith", result.Value.EnrolledStudents[0].FullName);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNotFoundForNonExistent()
    {
        var result = await _sut.GetByIdAsync(999);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesCourse()
    {
        var instructor = _db.CreateUser("inst-1", "John Doe", "john@test.com");
        var course = _db.CreateCourse(instructor.Id, "Old Title");
        var request = new UpdateCourseRequest { Title = "New Title", Description = "New Desc" };

        var result = await _sut.UpdateAsync(course.Id, instructor.Id, false, request);

        Assert.True(result.IsSuccess);
        Assert.Equal("New Title", result.Value!.Title);
        Assert.Equal("New Desc", result.Value.Description);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsUnauthorizedForWrongInstructor()
    {
        var instructor = _db.CreateUser("inst-1", "John Doe", "john@test.com");
        _db.CreateUser("inst-2", "Other", "other@test.com");
        var course = _db.CreateCourse(instructor.Id);
        var request = new UpdateCourseRequest { Title = "Hacked", Description = "Hacked" };

        var result = await _sut.UpdateAsync(course.Id, "inst-2", false, request);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unauthorized, result.Error);
    }

    [Fact]
    public async Task UpdateAsync_AdminCanUpdateAnyCourse()
    {
        var instructor = _db.CreateUser("inst-1", "John Doe", "john@test.com");
        var admin = _db.CreateUser("admin-1", "Admin", "admin@test.com");
        var course = _db.CreateCourse(instructor.Id, "Original");
        var request = new UpdateCourseRequest { Title = "Admin Edit", Description = "Updated" };

        var result = await _sut.UpdateAsync(course.Id, admin.Id, true, request);

        Assert.True(result.IsSuccess);
        Assert.Equal("Admin Edit", result.Value!.Title);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsUnauthorizedForWrongInstructor()
    {
        var instructor = _db.CreateUser("inst-1", "John Doe", "john@test.com");
        _db.CreateUser("inst-2", "Other", "other@test.com");
        var course = _db.CreateCourse(instructor.Id);

        var result = await _sut.DeleteAsync(course.Id, "inst-2", false);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unauthorized, result.Error);
    }

    [Fact]
    public async Task DeleteAsync_AdminCanDeleteAnyCourse()
    {
        var instructor = _db.CreateUser("inst-1", "John Doe", "john@test.com");
        var course = _db.CreateCourse(instructor.Id);

        var result = await _sut.DeleteAsync(course.Id, "admin-1", true);

        Assert.True(result.IsSuccess);
        var all = await _sut.GetAllAsync();
        Assert.Equal(0, all.Value!.TotalCount);
    }

    [Fact]
    public async Task EnrollStudentAsync_EnrollsSuccessfully()
    {
        var instructor = _db.CreateUser("inst-1", "John Doe", "john@test.com");
        var student = _db.CreateUser("stu-1", "Jane Smith", "jane@test.com");
        var course = _db.CreateCourse(instructor.Id);

        var result = await _sut.EnrollStudentAsync(course.Id, student.Id);

        Assert.True(result.IsSuccess);
        var detail = await _sut.GetByIdAsync(course.Id);
        Assert.Single(detail.Value!.EnrolledStudents);
    }

    [Fact]
    public async Task EnrollStudentAsync_ReturnsConflictIfAlreadyEnrolled()
    {
        var instructor = _db.CreateUser("inst-1", "John Doe", "john@test.com");
        var student = _db.CreateUser("stu-1", "Jane Smith", "jane@test.com");
        var course = _db.CreateCourse(instructor.Id);
        _db.CreateEnrollment(student.Id, course.Id);

        var result = await _sut.EnrollStudentAsync(course.Id, student.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error);
    }

    [Fact]
    public async Task UnenrollStudentAsync_RemovesEnrollment()
    {
        var instructor = _db.CreateUser("inst-1", "John Doe", "john@test.com");
        var student = _db.CreateUser("stu-1", "Jane Smith", "jane@test.com");
        var course = _db.CreateCourse(instructor.Id);
        _db.CreateEnrollment(student.Id, course.Id);

        var result = await _sut.UnenrollStudentAsync(course.Id, student.Id);

        Assert.True(result.IsSuccess);
        var detail = await _sut.GetByIdAsync(course.Id);
        Assert.Empty(detail.Value!.EnrolledStudents);
    }

    [Fact]
    public async Task GetByInstructorAsync_ReturnsOnlyInstructorsCourses()
    {
        var inst1 = _db.CreateUser("inst-1", "John", "john@test.com");
        var inst2 = _db.CreateUser("inst-2", "Jane", "jane@test.com");
        _db.CreateCourse(inst1.Id, "Course A");
        _db.CreateCourse(inst1.Id, "Course B");
        _db.CreateCourse(inst2.Id, "Course C");

        var result = await _sut.GetByInstructorAsync(inst1.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.TotalCount);
        Assert.All(result.Value.Items, c => Assert.Equal(inst1.Id, c.InstructorId));
    }

    [Fact]
    public async Task GetEnrolledCoursesAsync_ReturnsOnlyEnrolledCourses()
    {
        var instructor = _db.CreateUser("inst-1", "John", "john@test.com");
        var student = _db.CreateUser("stu-1", "Jane", "jane@test.com");
        var courseA = _db.CreateCourse(instructor.Id, "Course A");
        _db.CreateCourse(instructor.Id, "Course B");
        _db.CreateEnrollment(student.Id, courseA.Id);

        var result = await _sut.GetEnrolledCoursesAsync(student.Id);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Items);
        Assert.Equal("Course A", result.Value.Items[0].Title);
    }

    [Fact]
    public async Task GetAllAsync_PaginatesCorrectly()
    {
        var instructor = _db.CreateUser("inst-1", "John", "john@test.com");
        for (int i = 1; i <= 5; i++)
            _db.CreateCourse(instructor.Id, $"Course {i}");

        var page1 = await _sut.GetAllAsync(page: 1, pageSize: 2);
        var page2 = await _sut.GetAllAsync(page: 2, pageSize: 2);
        var page3 = await _sut.GetAllAsync(page: 3, pageSize: 2);

        Assert.Equal(5, page1.Value!.TotalCount);
        Assert.Equal(3, page1.Value.TotalPages);
        Assert.Equal(2, page1.Value.Items.Count);
        Assert.True(page1.Value.HasNextPage);
        Assert.False(page1.Value.HasPreviousPage);

        Assert.Equal(2, page2.Value!.Items.Count);
        Assert.True(page2.Value.HasPreviousPage);
        Assert.True(page2.Value.HasNextPage);

        Assert.Single(page3.Value!.Items);
        Assert.True(page3.Value.HasPreviousPage);
        Assert.False(page3.Value.HasNextPage);
    }

    [Fact]
    public async Task GetByInstructorAsync_PaginatesCorrectly()
    {
        var instructor = _db.CreateUser("inst-1", "John", "john@test.com");
        for (int i = 1; i <= 4; i++)
            _db.CreateCourse(instructor.Id, $"Course {i}");

        var result = await _sut.GetByInstructorAsync(instructor.Id, page: 1, pageSize: 2);

        Assert.Equal(4, result.Value!.TotalCount);
        Assert.Equal(2, result.Value.Items.Count);
        Assert.True(result.Value.HasNextPage);
    }

    [Fact]
    public async Task GetEnrolledCoursesAsync_PaginatesCorrectly()
    {
        var instructor = _db.CreateUser("inst-1", "John", "john@test.com");
        var student = _db.CreateUser("stu-1", "Jane", "jane@test.com");
        for (int i = 1; i <= 3; i++)
        {
            var course = _db.CreateCourse(instructor.Id, $"Course {i}");
            _db.CreateEnrollment(student.Id, course.Id);
        }

        var result = await _sut.GetEnrolledCoursesAsync(student.Id, page: 1, pageSize: 2);

        Assert.Equal(3, result.Value!.TotalCount);
        Assert.Equal(2, result.Value.Items.Count);
        Assert.True(result.Value.HasNextPage);
    }

    public void Dispose() => _db.Dispose();
}
