using System.ComponentModel.DataAnnotations;

namespace LMS.DTOs;

public record RegisterRequest
{
    [Required, MaxLength(100)]
    public string FullName { get; init; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; init; } = string.Empty;

    [Required]
    public string Role { get; init; } = "Student"; // "Student" or "Instructor"
}

public record LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}

public record AuthResponse
{
    public string Token { get; init; } = string.Empty;
    public DateTime Expiration { get; init; }
    public UserDto User { get; init; } = null!;
}

public record UserDto
{
    public string Id { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public IList<string> Roles { get; init; } = [];
}

public record UpdateProfileRequest
{
    [Required, MaxLength(100)]
    public string FullName { get; init; } = string.Empty;
}
