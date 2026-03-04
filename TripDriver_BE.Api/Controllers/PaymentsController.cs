using Microsoft.AspNetCore.Mvc;
using TripDriver_BE.Api.Dtos;
using TripDriver_BE.Service;

namespace TripDriver_BE.Api.Controllers;

[ApiController]
[Route("payments")]
public class PaymentsController : ControllerBase
{
    private readonly PaymentService _payments;

    public PaymentsController(PaymentService payments)
    {
        _payments = payments;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook(
        [FromBody] TripDriver_BE.Api.Dtos.PaymentWebhookRequest req)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var svcReq = new TripDriver_BE.Service.PaymentWebhookRequest
        {
            BookingId = req.BookingId,
            Type = req.Type,
            Status = req.Status,
            ExternalTxnId = req.ExternalTxnId
        };

        var result = await _payments.HandleWebhookAsync(svcReq);
        return Ok(result);
    }
}