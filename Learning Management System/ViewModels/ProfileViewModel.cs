using System.ComponentModel.DataAnnotations;

namespace LMS.ViewModels;

// Used for both displaying and editing a user's profile (name, email, roles)
public class ProfileViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public IList<string> Roles { get; set; } = [];
}
