using System.ComponentModel.DataAnnotations;

namespace LMS.DTOs;

// Request/response DTOs for grading operations

// Input for grading or updating a grade on a submission (score 0-100)
public record GradeSubmissionRequest
{
    [Required, Range(0, 100)]
    public decimal Score { get; init; }

    [MaxLength(1000)]
    public string? Feedback { get; init; }
}

// Read-only view of a grade with the grader's name
public record GradeDto
{
    public int Id { get; init; }
    public decimal Score { get; init; }
    public string? Feedback { get; init; }
    public DateTime GradedAt { get; init; }
    public string GradedByName { get; init; } = string.Empty;
}
