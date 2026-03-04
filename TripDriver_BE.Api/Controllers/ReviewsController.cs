using TripDriver_BE.Api.Auth;
using TripDriver_BE.Repo.Data;
using TripDriver_BE.Api.Dtos;
using TripDriver_BE.Repo.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TripDriver_BE.Api.Controllers;

[ApiController]
public class ReviewsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ReviewsController(AppDbContext db) => _db = db;

    // POST /reviews (only when booking COMPLETED)
    [Authorize(Roles = UserRoles.CUSTOMER)]
    [HttpPost("reviews")]
    public async Task<IActionResult> Create([FromBody] CreateReviewRequest req)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var customerId = User.UserId();
        var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == req.BookingId)
            ?? throw new KeyNotFoundException("Booking not found");

        if (booking.CustomerId != customerId)
            return Forbid();

        if (booking.Status != BookingStatus.COMPLETED)
            return BadRequest(new { error = new { message = "Booking must be COMPLETED to review" } });

        var exists = await _db.CarReviews.AnyAsync(r => r.BookingId == booking.Id);
        if (exists)
            return BadRequest(new { error = new { message = "This booking already has a review" } });

        var review = new CarReview
        {
            Id = Guid.NewGuid(),
            BookingId = booking.Id,
            CarId = booking.CarId,
            OwnerId = booking.OwnerId,
            CustomerId = booking.CustomerId,
            Rating = req.Rating,
            Comment = req.Comment,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.CarReviews.Add(review);
        await _db.SaveChangesAsync();

        return Ok(new { review.Id });
    }

    // GET /cars/{id}/reviews
    [AllowAnonymous]
    [HttpGet("cars/{id:guid}/reviews")]
    public async Task<IActionResult> ByCar(Guid id, [FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        page = Math.Max(page, 1);
        limit = Math.Clamp(limit, 1, 100);

        var q = _db.CarReviews.AsNoTracking().Where(r => r.CarId == id);
        var total = await q.CountAsync();

        var data = await q.OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(r => new { r.Rating, r.Comment, r.CreatedAt })
            .ToListAsync();

        return Ok(new { page, limit, total, data });
    }
}

