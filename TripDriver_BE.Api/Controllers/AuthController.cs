using BCrypt.Net;
using TripDriver_BE.Api.Auth;
using TripDriver_BE.Repo.Data;
using TripDriver_BE.Api.Dtos;
using TripDriver_BE.Repo.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TripDriver_BE.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtTokenService _jwt;

    public AuthController(AppDbContext db, JwtTokenService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest req)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var roleInput = req.Role?.Trim().ToUpperInvariant() ?? "";

        var role = roleInput switch
        {
            "CUSTOMER" or "CUS" or "USER" or "CLIENT" or "CUSTOMERROLE" => UserRoles.CUSTOMER,
            "OWNER" => UserRoles.OWNER,
            _ => ""
        };

        if (role is not (UserRoles.CUSTOMER or UserRoles.OWNER))
            return BadRequest(new { error = new { message = "Role must be Customer or Owner" } });

        var emailExists = await _db.Users.AnyAsync(u => u.Email == req.Email);
        if (emailExists)
            return BadRequest(new { error = new { message = "Email already exists" } });

        var phoneExists = await _db.UserProfiles.AnyAsync(p => p.Phone == req.Phone);
        if (phoneExists)
            return BadRequest(new { error = new { message = "Phone already exists" } });

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = req.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var profile = new UserProfile
        {
            UserId = user.Id,
            FullName = req.FullName,
            Phone = req.Phone,
            City = req.City,
            Address = req.Address,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        _db.UserProfiles.Add(profile);
        await _db.SaveChangesAsync();

        var token = _jwt.CreateToken(user);

        return Ok(new AuthResponse
        {
            AccessToken = token,
            Role = user.Role,
            UserId = user.Id,
            Email = user.Email
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest req)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);

        if (user is null)
            return Unauthorized(new { error = new { message = "Invalid credentials" } });

        if (!user.IsActive)
            return Unauthorized(new { error = new { message = "User is inactive" } });

        var ok = BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash);

        if (!ok)
            return Unauthorized(new { error = new { message = "Invalid credentials" } });

        var token = _jwt.CreateToken(user);

        return Ok(new AuthResponse
        {
            AccessToken = token,
            Role = user.Role,
            UserId = user.Id,
            Email = user.Email
        });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = User.UserId();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
            return NotFound();

        var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

        return Ok(new
        {
            user.Id,
            user.Email,
            user.Role,
            user.IsActive,
            Profile = profile is null
                ? null
                : new
                {
                    profile.FullName,
                    profile.Phone,
                    profile.City,
                    profile.Address
                }
        });
    }
}