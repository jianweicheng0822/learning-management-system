using System.ComponentModel.DataAnnotations;

namespace LMS.DTOs;

public record CreateCourseRequest
{
    [Required, MaxLength(200)]
    public string Title { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; init; } = string.Empty;
}

public record UpdateCourseRequest
{
    [Required, MaxLength(200)]
    public string Title { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; init; } = string.Empty;
}

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

public record CourseDetailDto : CourseDto
{
    public IList<EnrolledStudentDto> EnrolledStudents { get; init; } = [];
    public IList<AssignmentDto> Assignments { get; init; } = [];
}

public record EnrolledStudentDto
{
    public string StudentId { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime EnrolledAt { get; init; }
}
