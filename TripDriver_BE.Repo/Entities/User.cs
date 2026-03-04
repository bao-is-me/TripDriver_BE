namespace TripDriver_BE.Repo.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";

    /// <summary>
    /// DB constraint: CUSTOMER | OWNER | ADMIN
    /// </summary>
    public string Role { get; set; } = UserRoles.CUSTOMER;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public static class UserRoles
{
    public const string CUSTOMER = "CUSTOMER";
    public const string OWNER = "OWNER";
    public const string ADMIN = "ADMIN";
}

