using Microsoft.AspNetCore.Identity;

namespace LMS.Models;

// Extends the default Identity user with LMS-specific profile data and relationships
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Course> InstructedCourses { get; set; } = [];
    public ICollection<Enrollment> Enrollments { get; set; } = [];
    public ICollection<Submission> Submissions { get; set; } = [];
}
