namespace TripDriver_BE.Service;

/// <summary>Webhook payload forwarded from the Api layer.</summary>
public class PaymentWebhookRequest
{
    public Guid BookingId { get; set; }
    public string Type { get; set; } = "";   // DEPOSIT | REMAINING
    public string Status { get; set; } = ""; // SUCCESS | FAILED
    public string? ExternalTxnId { get; set; }
}
