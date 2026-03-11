using System.ComponentModel.DataAnnotations;

namespace LMS.DTOs;

public record CreateAssignmentRequest
{
    [Required, MaxLength(200)]
    public string Title { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; init; } = string.Empty;

    [Required]
    public DateTime DueDate { get; init; }
}

public record UpdateAssignmentRequest
{
    [Required, MaxLength(200)]
    public string Title { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; init; } = string.Empty;

    [Required]
    public DateTime DueDate { get; init; }
}

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
