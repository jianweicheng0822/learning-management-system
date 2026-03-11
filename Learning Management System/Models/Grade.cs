using System.ComponentModel.DataAnnotations;

namespace LMS.Models;

public class Grade
{
    public int Id { get; set; }

    public decimal Score { get; set; }

    [MaxLength(1000)]
    public string? Feedback { get; set; }

    public DateTime GradedAt { get; set; } = DateTime.UtcNow;

    // Foreign keys
    public int SubmissionId { get; set; }
    public Submission Submission { get; set; } = null!;

    public string GradedById { get; set; } = string.Empty;
    public ApplicationUser GradedBy { get; set; } = null!;
}
