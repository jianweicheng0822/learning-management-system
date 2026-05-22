using System.ComponentModel.DataAnnotations;

namespace LMS.DTOs;

// Request/response DTOs for assignment operations

// Input for creating a new assignment under a course
public record CreateAssignmentRequest
{
    [Required, MaxLength(200)]
    public string Title { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; init; } = string.Empty;

    [Required]
    public DateTime DueDate { get; init; }
}

// Input for updating an existing assignment
public record UpdateAssignmentRequest
{
    [Required, MaxLength(200)]
    public string Title { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; init; } = string.Empty;

    [Required]
    public DateTime DueDate { get; init; }
}

// Read-only view of an assignment with its parent course name and submission count
public record AssignmentDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTime DueDate { get; init; }
    public int CourseId { get; init; }
    public string CourseName { get; init; } = string.Empty;
    public int SubmissionCount { get; init; }
    public DateTime CreatedAt { get; init; }
}
