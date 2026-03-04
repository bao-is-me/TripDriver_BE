using TripDriver_BE.Api.Auth;
using TripDriver_BE.Repo.Data;
using TripDriver_BE.Api.Dtos;
using TripDriver_BE.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TripDriver_BE.Api.Controllers;

[ApiController]
[Route("owner/bookings")]
[Authorize(Roles = TripDriver_BE.Repo.Entities.UserRoles.OWNER)]
public class OwnerBookingsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly BookingWorkflowService _workflow;

    public OwnerBookingsController(AppDbContext db, BookingWorkflowService workflow)
    {
        _db = db;
        _workflow = workflow;
    }

    // GET /owner/bookings
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var ownerId = User.UserId();
        var data = await _db.Bookings.AsNoTracking()
            .Where(b => b.OwnerId == ownerId)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new { b.Id, b.BookingCode, b.Status, b.CarId, b.CustomerId, b.StartDate, b.EndDate, b.TotalAmount, b.DepositAmount, b.RemainingAmount, b.CreatedAt })
            .ToListAsync();

        return Ok(data);
    }

    [HttpPost("{id:guid}/confirm")]
    public async Task<IActionResult> Confirm(Guid id)
    {
        await _workflow.OwnerConfirmAsync(id, User.UserId());
        return Ok(new { bookingId = id, status = TripDriver_BE.Repo.Entities.BookingStatus.OWNER_CONFIRMED });
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectBookingRequest req)
    {
        await _workflow.OwnerRejectAsync(id, User.UserId(), req.Reason);
        return Ok(new { bookingId = id, status = TripDriver_BE.Repo.Entities.BookingStatus.OWNER_REJECTED });
    }

    [HttpPost("{id:guid}/hand-over")]
    public async Task<IActionResult> HandOver(Guid id)
    {
        await _workflow.OwnerHandOverAsync(id, User.UserId());
        return Ok(new { bookingId = id, status = TripDriver_BE.Repo.Entities.BookingStatus.IN_PROGRESS });
    }

    [HttpPost("{id:guid}/return-ok")]
    public async Task<IActionResult> ReturnOk(Guid id)
    {
        await _workflow.OwnerReturnOkAsync(id, User.UserId());
        return Ok(new { bookingId = id, status = TripDriver_BE.Repo.Entities.BookingStatus.RETURN_OK_PENDING_PAYMENT });
    }

    [HttpPost("{id:guid}/report-damage")]
    public async Task<IActionResult> ReportDamage(Guid id, [FromBody] string? note)
    {
        await _workflow.OwnerReportDamageAsync(id, User.UserId(), note);
        return Ok(new { bookingId = id, status = TripDriver_BE.Repo.Entities.BookingStatus.DISPUTE });
    }
}

