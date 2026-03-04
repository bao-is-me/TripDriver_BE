using TripDriver_BE.Repo.Data;
using TripDriver_BE.Repo.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TripDriver_BE.Api.Controllers;

[ApiController]
[Route("cars")]
public class CarsController : ControllerBase
{
    private readonly AppDbContext _db;

    public CarsController(AppDbContext db) => _db = db;

    // GET /cars (default: all ACTIVE)
    // optional: ?startDate=2026-03-01&endDate=2026-03-05 to filter availability
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] DateOnly? startDate = null,
        [FromQuery] DateOnly? endDate = null)
    {
        page = Math.Max(page, 1);
        limit = Math.Clamp(limit, 1, 100);

        var q = _db.Cars.AsNoTracking().Where(c => c.Status == CarStatus.ACTIVE);

        if (startDate.HasValue && endDate.HasValue)
        {
            var s = startDate.Value;
            var e = endDate.Value;
            q = q.Where(c => !_db.Bookings.Any(b =>
                b.CarId == c.Id &&
                BookingStatus.Blocking.Contains(b.Status) &&
                b.StartDate < e &&
                b.EndDate > s
            ));
        }

        var total = await q.CountAsync();

        var data = await q
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(c => new
            {
                c.Id,
                c.Brand,
                c.Model,
                c.Year,
                c.Transmission,
                c.Fuel,
                c.Seats,
                c.Location,
                c.PricePerDay,
                c.DepositPercent,
                ThumbnailUrl = _db.CarImages
                    .Where(i => i.CarId == c.Id)
                    .OrderBy(i => i.SortOrder)
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return Ok(new { page, limit, total, data });
    }

    // GET /cars/{id} (only ACTIVE)
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Detail(Guid id)
    {
        var car = await _db.Cars.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id && c.Status == CarStatus.ACTIVE);
        if (car is null) return NotFound();

        var images = await _db.CarImages.AsNoTracking()
            .Where(i => i.CarId == id)
            .OrderBy(i => i.SortOrder)
            .Select(i => i.ImageUrl)
            .ToListAsync();

        var reviews = await _db.CarReviews.AsNoTracking()
            .Where(r => r.CarId == id)
            .OrderByDescending(r => r.CreatedAt)
            .Take(10)
            .Select(r => new { r.Rating, r.Comment, r.CreatedAt })
            .ToListAsync();

        return Ok(new
        {
            car.Id,
            car.Brand,
            car.Model,
            car.Year,
            car.Transmission,
            car.Fuel,
            car.Seats,
            car.Location,
            car.PricePerDay,
            car.DepositPercent,
            car.Description,
            Images = images,
            RecentReviews = reviews
        });
    }
}

