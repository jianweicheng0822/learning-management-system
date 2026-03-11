using System.ComponentModel.DataAnnotations;

namespace LMS.Models;

public class Assignment
{
    public int Id { get; set; }

    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    public DateTime DueDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign key
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    // Navigation
    public ICollection<Submission> Submissions { get; set; } = [];
}
