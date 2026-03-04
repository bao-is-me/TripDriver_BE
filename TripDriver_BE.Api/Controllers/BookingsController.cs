using TripDriver_BE.Api.Auth;
using TripDriver_BE.Repo.Data;
using TripDriver_BE.Api.Dtos;
using TripDriver_BE.Repo.Entities;
using TripDriver_BE.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TripDriver_BE.Api.Controllers;

[ApiController]
[Route("bookings")]
[Authorize(Roles = UserRoles.CUSTOMER)]
public class BookingsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly BookingWorkflowService _workflow;
    private readonly PaymentService _payments;

    public BookingsController(AppDbContext db, BookingWorkflowService workflow, PaymentService payments)
    {
        _db = db;
        _workflow = workflow;
        _payments = payments;
    }

    // POST /bookings -> create PENDING_DEPOSIT
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest req)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var customerId = User.UserId();
        var booking = await _workflow.CreateBookingAsync(customerId, req.CarId, req.StartDate, req.EndDate, req.PickupNote, req.CustomerNote);

        return Ok(new
        {
            booking.Id,
            booking.BookingCode,
            booking.Status,
            booking.TotalAmount,
            booking.DepositAmount,
            booking.RemainingAmount,
            booking.StartDate,
            booking.EndDate
        });
    }

    // GET /bookings/my
    [HttpGet("my")]
    public async Task<IActionResult> My()
    {
        var customerId = User.UserId();
        var data = await _db.Bookings.AsNoTracking()
            .Where(b => b.CustomerId == customerId)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new
            {
                b.Id,
                b.BookingCode,
                b.Status,
                b.CarId,
                b.StartDate,
                b.EndDate,
                b.TotalAmount,
                b.DepositAmount,
                b.RemainingAmount,
                b.CreatedAt
            })
            .ToListAsync();

        return Ok(data);
    }

    // POST /bookings/{id}/payments/deposit
    [HttpPost("{id:guid}/payments/deposit")]
    public async Task<IActionResult> CreateDepositIntent(Guid id, [FromBody] CreatePaymentIntentRequest req)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        if (!req.Type.Equals(PaymentTypes.DEPOSIT, StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = new { message = "Type must be DEPOSIT for this endpoint" } });

        var customerId = User.UserId();
        var payment = await _payments.CreatePaymentIntentAsync(id, customerId, PaymentTypes.DEPOSIT, req.Method);

        return Ok(new { payment.Id, payment.BookingId, payment.Type, payment.Status, payment.Method, payment.Amount, payment.PaymentUrl });
    }

    // POST /bookings/{id}/payments/remaining
    [HttpPost("{id:guid}/payments/remaining")]
    public async Task<IActionResult> CreateRemainingIntent(Guid id, [FromBody] CreatePaymentIntentRequest req)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        if (!req.Type.Equals(PaymentTypes.REMAINING, StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = new { message = "Type must be REMAINING for this endpoint" } });

        var customerId = User.UserId();
        var payment = await _payments.CreatePaymentIntentAsync(id, customerId, PaymentTypes.REMAINING, req.Method);

        return Ok(new { payment.Id, payment.BookingId, payment.Type, payment.Status, payment.Method, payment.Amount, payment.PaymentUrl });
    }
}

