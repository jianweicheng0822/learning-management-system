using System.ComponentModel.DataAnnotations;

namespace LMS.DTOs;

// Request/response DTOs for course operations

// Input for creating a new course
public record CreateCourseRequest
{
    [Required, MaxLength(200)]
    public string Title { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; init; } = string.Empty;
}

// Input for updating an existing course
public record UpdateCourseRequest
{
    [Required, MaxLength(200)]
    public string Title { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; init; } = string.Empty;
}

// Summary view of a course used in list pages
public record CourseDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string InstructorId { get; init; } = string.Empty;
    public string InstructorName { get; init; } = string.Empty;
    public int EnrolledCount { get; init; }
    public DateTime CreatedAt { get; init; }
}

// Extended course view with enrolled students and assignments (used on the detail page)
public record CourseDetailDto : CourseDto
{
    public IList<EnrolledStudentDto> EnrolledStudents { get; init; } = [];
    public IList<AssignmentDto> Assignments { get; init; } = [];
}

// Student info shown in the course detail enrollment list
public record EnrolledStudentDto
{
    public string StudentId { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime EnrolledAt { get; init; }
}
