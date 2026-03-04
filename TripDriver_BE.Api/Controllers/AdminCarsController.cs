using TripDriver_BE.Api.Auth;
using TripDriver_BE.Repo.Data;
using TripDriver_BE.Repo.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TripDriver_BE.Api.Controllers;

[ApiController]
[Route("admin/cars")]
[Authorize(Roles = UserRoles.ADMIN)]
public class AdminCarsController : ControllerBase
{
    private readonly AppDbContext _db;
    public AdminCarsController(AppDbContext db) => _db = db;

    // GET /admin/cars/pending
    [HttpGet("pending")]
    public async Task<IActionResult> Pending()
    {
        var cars = await _db.Cars.AsNoTracking()
            .Where(c => c.Status == CarStatus.PENDING_APPROVAL)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        return Ok(cars);
    }

    // POST /admin/cars/{id}/approve -> ACTIVE
    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var car = await _db.Cars.FirstOrDefaultAsync(c => c.Id == id);
        if (car is null) return NotFound();

        if (car.Status != CarStatus.PENDING_APPROVAL)
            return BadRequest(new { error = new { message = "Car is not PENDING_APPROVAL" } });

        car.Status = CarStatus.ACTIVE;
        car.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { car.Id, car.Status });
    }

    // POST /admin/cars/{id}/reject -> INACTIVE
    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id)
    {
        var car = await _db.Cars.FirstOrDefaultAsync(c => c.Id == id);
        if (car is null) return NotFound();

        if (car.Status != CarStatus.PENDING_APPROVAL)
            return BadRequest(new { error = new { message = "Car is not PENDING_APPROVAL" } });

        car.Status = CarStatus.INACTIVE;
        car.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { car.Id, car.Status });
    }
}

