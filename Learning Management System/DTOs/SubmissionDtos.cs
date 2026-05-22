using System.ComponentModel.DataAnnotations;

namespace LMS.DTOs;

// Request/response DTOs for submission operations

// Input for submitting work — at least one of TextContent or FilePath must be provided
public record CreateSubmissionRequest
{
    [MaxLength(5000)]
    public string? TextContent { get; init; }

    [MaxLength(500)]
    public string? FilePath { get; init; }
}

// Read-only view of a submission, including the optional attached grade
public record SubmissionDto
{
    public int Id { get; init; }
    public string? TextContent { get; init; }
    public string? FilePath { get; init; }
    public DateTime SubmittedAt { get; init; }
    public string StudentId { get; init; } = string.Empty;
    public string StudentName { get; init; } = string.Empty;
    public int AssignmentId { get; init; }
    public string AssignmentTitle { get; init; } = string.Empty;
    public GradeDto? Grade { get; init; }
}
