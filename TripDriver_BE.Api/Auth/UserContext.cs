using System.Security.Claims;

namespace TripDriver_BE.Api.Auth;

public static class UserContext
{
    public static Guid UserId(this ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(id, out var guid) ? guid : Guid.Empty;
    }

    public static string Role(this ClaimsPrincipal user)
        => user.FindFirstValue(ClaimTypes.Role) ?? "";
}

