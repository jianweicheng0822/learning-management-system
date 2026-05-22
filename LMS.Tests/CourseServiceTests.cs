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

        Assert.Equal("C# 101", result.Title);
        Assert.Equal("John Doe", result.InstructorName);
        Assert.Equal(0, result.EnrolledCount);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllCourses()
    {
        var instructor = _db.CreateUser("inst-1", "John Doe", "john@test.com");
        _db.CreateCourse(instructor.Id, "Course A");
        _db.CreateCourse(instructor.Id, "Course B");

        var result = await _sut.GetAllAsync();

        Assert.Equal(2, result.Count);
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

        Assert.Equal(course.Id, result.Id);
        Assert.Single(result.EnrolledStudents);
        Assert.Single(result.Assignments);
        Assert.Equal("Jane Smith", result.EnrolledStudents[0].FullName);
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsForNonExistent()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.GetByIdAsync(999));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesCourse()
    {
        var instructor = _db.CreateUser("inst-1", "John Doe", "john@test.com");
        var course = _db.CreateCourse(instructor.Id, "Old Title");
        var request = new UpdateCourseRequest { Title = "New Title", Description = "New Desc" };

        var result = await _sut.UpdateAsync(course.Id, instructor.Id, false, request);

        Assert.Equal("New Title", result.Title);
        Assert.Equal("New Desc", result.Description);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsForWrongInstructor()
    {
        var instructor = _db.CreateUser("inst-1", "John Doe", "john@test.com");
        _db.CreateUser("inst-2", "Other", "other@test.com");
        var course = _db.CreateCourse(instructor.Id);
        var request = new UpdateCourseRequest { Title = "Hacked", Description = "Hacked" };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _sut.UpdateAsync(course.Id, "inst-2", false, request));
    }

    [Fact]
    public async Task UpdateAsync_AdminCanUpdateAnyCourse()
    {
        var instructor = _db.CreateUser("inst-1", "John Doe", "john@test.com");
        var admin = _db.CreateUser("admin-1", "Admin", "admin@test.com");
        var course = _db.CreateCourse(instructor.Id, "Original");
        var request = new UpdateCourseRequest { Title = "Admin Edit", Description = "Updated" };

        var result = await _sut.UpdateAsync(course.Id, admin.Id, true, request);

        Assert.Equal("Admin Edit", result.Title);
    }

    [Fact]
    public async Task DeleteAsync_ThrowsForWrongInstructor()
    {
        var instructor = _db.CreateUser("inst-1", "John Doe", "john@test.com");
        _db.CreateUser("inst-2", "Other", "other@test.com");
        var course = _db.CreateCourse(instructor.Id);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _sut.DeleteAsync(course.Id, "inst-2", false));
    }

    [Fact]
    public async Task DeleteAsync_AdminCanDeleteAnyCourse()
    {
        var instructor = _db.CreateUser("inst-1", "John Doe", "john@test.com");
        var course = _db.CreateCourse(instructor.Id);

        await _sut.DeleteAsync(course.Id, "admin-1", true);

        var all = await _sut.GetAllAsync();
        Assert.Empty(all);
    }

    [Fact]
    public async Task EnrollStudentAsync_EnrollsSuccessfully()
    {
        var instructor = _db.CreateUser("inst-1", "John Doe", "john@test.com");
        var student = _db.CreateUser("stu-1", "Jane Smith", "jane@test.com");
        var course = _db.CreateCourse(instructor.Id);

        await _sut.EnrollStudentAsync(course.Id, student.Id);

        var detail = await _sut.GetByIdAsync(course.Id);
        Assert.Single(detail.EnrolledStudents);
    }

    [Fact]
    public async Task EnrollStudentAsync_ThrowsIfAlreadyEnrolled()
    {
        var instructor = _db.CreateUser("inst-1", "John Doe", "john@test.com");
        var student = _db.CreateUser("stu-1", "Jane Smith", "jane@test.com");
        var course = _db.CreateCourse(instructor.Id);
        _db.CreateEnrollment(student.Id, course.Id);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.EnrollStudentAsync(course.Id, student.Id));
    }

    [Fact]
    public async Task UnenrollStudentAsync_RemovesEnrollment()
    {
        var instructor = _db.CreateUser("inst-1", "John Doe", "john@test.com");
        var student = _db.CreateUser("stu-1", "Jane Smith", "jane@test.com");
        var course = _db.CreateCourse(instructor.Id);
        _db.CreateEnrollment(student.Id, course.Id);

        await _sut.UnenrollStudentAsync(course.Id, student.Id);

        var detail = await _sut.GetByIdAsync(course.Id);
        Assert.Empty(detail.EnrolledStudents);
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

        Assert.Equal(2, result.Count);
        Assert.All(result, c => Assert.Equal(inst1.Id, c.InstructorId));
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

        Assert.Single(result);
        Assert.Equal("Course A", result[0].Title);
    }

    public void Dispose() => _db.Dispose();
}
