using System.ComponentModel.DataAnnotations;

namespace LMS.Models;

public class Submission
{
    public int Id { get; set; }

    [MaxLength(5000)]
    public string? TextContent { get; set; }

    [MaxLength(500)]
    public string? FilePath { get; set; }

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    // Foreign keys
    public string StudentId { get; set; } = string.Empty;
    public ApplicationUser Student { get; set; } = null!;

    public int AssignmentId { get; set; }
    public Assignment Assignment { get; set; } = null!;

    // Navigation
    public Grade? Grade { get; set; }
}
