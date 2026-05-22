using System.ComponentModel.DataAnnotations;

namespace LMS.ViewModels;

// Form data for the login page, with optional return URL for post-login redirect
public class LoginViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
