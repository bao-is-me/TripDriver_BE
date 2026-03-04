using System.ComponentModel.DataAnnotations;

namespace TripDriver_BE.Api.Dtos;

public class RegisterRequest
{
    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; } = "";

    [Required, MinLength(6)]
    public string Password { get; set; } = "";

    /// <summary>
    /// Customer | Owner
    /// </summary>
    [Required]
    public string Role { get; set; } = "Customer";

    [Required, MaxLength(200)]
    public string FullName { get; set; } = "";

    [Required, MaxLength(30)]
    public string Phone { get; set; } = "";

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(255)]
    public string? Address { get; set; }
}

public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";
}

public class AuthResponse
{
    public string AccessToken { get; set; } = "";
    public string Role { get; set; } = "";
    public Guid UserId { get; set; }
    public string Email { get; set; } = "";
}

