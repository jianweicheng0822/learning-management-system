using System.ComponentModel.DataAnnotations;

namespace LMS.Models;

public class Course
{
    public int Id { get; set; }

    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign key
    public string InstructorId { get; set; } = string.Empty;
    public ApplicationUser Instructor { get; set; } = null!;

    // Navigation properties
    public ICollection<Enrollment> Enrollments { get; set; } = [];
    public ICollection<Assignment> Assignments { get; set; } = [];
}
