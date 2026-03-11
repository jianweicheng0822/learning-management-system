using System.ComponentModel.DataAnnotations;

namespace LMS.DTOs;

public record GradeSubmissionRequest
{
    [Required, Range(0, 100)]
    public decimal Score { get; init; }

    [MaxLength(1000)]
    public string? Feedback { get; init; }
}

public record GradeDto
{
    public int Id { get; init; }
    public decimal Score { get; init; }
    public string? Feedback { get; init; }
    public DateTime GradedAt { get; init; }
    public string GradedByName { get; init; } = string.Empty;
}
