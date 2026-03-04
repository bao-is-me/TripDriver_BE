using System.ComponentModel.DataAnnotations;

namespace TripDriver_BE.Api.Dtos;

public class CreatePaymentIntentRequest
{
    /// <summary>
    /// DEPOSIT | REMAINING
    /// </summary>
    [Required]
    public string Type { get; set; } = "";

    /// <summary>
    /// e.g. VNPAY/MOMO/STRIPE/MOCK
    /// </summary>
    [Required]
    public string Method { get; set; } = "MOCK";
}

public class PaymentWebhookRequest
{
    [Required]
    public Guid BookingId { get; set; }

    [Required]
    public string Type { get; set; } = ""; // DEPOSIT | REMAINING

    [Required]
    public string Status { get; set; } = ""; // SUCCESS | FAILED

    public string? ExternalTxnId { get; set; }
}

