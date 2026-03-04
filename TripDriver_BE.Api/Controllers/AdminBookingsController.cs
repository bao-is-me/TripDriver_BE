using TripDriver_BE.Api.Auth;
using TripDriver_BE.Api.Dtos;
using TripDriver_BE.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TripDriver_BE.Api.Controllers;

[ApiController]
[Route("admin/bookings")]
[Authorize(Roles = TripDriver_BE.Repo.Entities.UserRoles.ADMIN)]
public class AdminBookingsController : ControllerBase
{
    private readonly BookingWorkflowService _workflow;

    public AdminBookingsController(BookingWorkflowService workflow)
    {
        _workflow = workflow;
    }

    // POST /admin/bookings/{id}/cancel
    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelBookingRequest req)
    {
        await _workflow.AdminCancelAsync(id, User.UserId(), req.Reason);
        return Ok(new { bookingId = id, status = TripDriver_BE.Repo.Entities.BookingStatus.CANCELLED });
    }
}

