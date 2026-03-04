using TripDriver_BE.Api.Auth;
using TripDriver_BE.Repo.Data;
using TripDriver_BE.Api.Dtos;
using TripDriver_BE.Repo.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TripDriver_BE.Api.Controllers;

[ApiController]
[Route("owner/cars")]
[Authorize(Roles = UserRoles.OWNER)]
public class OwnerCarsController : ControllerBase
{
    private readonly AppDbContext _db;
    public OwnerCarsController(AppDbContext db) => _db = db;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] OwnerCreateCarRequest req)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var ownerId = User.UserId();

        var car = new Car
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            Brand = req.Brand,
            Model = req.Model,
            Year = req.Year,
            Transmission = req.Transmission,
            Fuel = req.Fuel,
            Seats = req.Seats,
            Location = req.Location,
            PricePerDay = req.PricePerDay,
            DepositPercent = req.DepositPercent,
            Description = req.Description,
            Status = CarStatus.DRAFT,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Cars.Add(car);
        await _db.SaveChangesAsync();
        return Ok(car);
    }

    [HttpGet]
    public async Task<IActionResult> MyCars()
    {
        var ownerId = User.UserId();
        var cars = await _db.Cars.AsNoTracking()
            .Where(c => c.OwnerId == ownerId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return Ok(cars);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] OwnerUpdateCarRequest req)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var ownerId = User.UserId();
        var car = await _db.Cars.FirstOrDefaultAsync(c => c.Id == id && c.OwnerId == ownerId);
        if (car is null) return NotFound();

        // Limit updates when submitted/active
        if (car.Status == CarStatus.PENDING_APPROVAL || car.Status == CarStatus.ACTIVE)
            return BadRequest(new { error = new { message = "Cannot edit car when PENDING_APPROVAL or ACTIVE" } });

        car.Brand = req.Brand;
        car.Model = req.Model;
        car.Year = req.Year;
        car.Transmission = req.Transmission;
        car.Fuel = req.Fuel;
        car.Seats = req.Seats;
        car.Location = req.Location;
        car.PricePerDay = req.PricePerDay;
        car.DepositPercent = req.DepositPercent;
        car.Description = req.Description;
        car.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(car);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ownerId = User.UserId();
        var car = await _db.Cars.FirstOrDefaultAsync(c => c.Id == id && c.OwnerId == ownerId);
        if (car is null) return NotFound();

        if (car.Status == CarStatus.ACTIVE)
            return BadRequest(new { error = new { message = "Cannot delete ACTIVE car" } });

        _db.Cars.Remove(car);
        await _db.SaveChangesAsync();
        return Ok(new { ok = true });
    }

    // POST /owner/cars/{id}/submit -> PENDING_APPROVAL
    [HttpPost("{id:guid}/submit")]
    public async Task<IActionResult> Submit(Guid id)
    {
        var ownerId = User.UserId();
        var car = await _db.Cars.FirstOrDefaultAsync(c => c.Id == id && c.OwnerId == ownerId);
        if (car is null) return NotFound();

        if (car.Status != CarStatus.DRAFT && car.Status != CarStatus.INACTIVE)
            return BadRequest(new { error = new { message = "Can only submit when DRAFT or INACTIVE" } });

        car.Status = CarStatus.PENDING_APPROVAL;
        car.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { car.Id, car.Status });
    }
}

