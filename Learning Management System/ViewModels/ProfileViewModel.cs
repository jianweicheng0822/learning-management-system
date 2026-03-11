using System.ComponentModel.DataAnnotations;

namespace LMS.ViewModels;

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
