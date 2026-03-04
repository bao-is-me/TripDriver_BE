namespace TripDriver_BE.Repo.Entities;

public class Payment
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }

    /// <summary>
    /// DEPOSIT | REMAINING
    /// </summary>
    public string Type { get; set; } = "";

    /// <summary>
    /// PENDING | SUCCESS | FAILED | CANCELLED
    /// </summary>
    public string Status { get; set; } = "PENDING";

    /// <summary>
    /// your method name: e.g. VNPAY, MOMO, STRIPE, MOCK
    /// </summary>
    public string Method { get; set; } = "MOCK";

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";

    public string? ProviderTxnId { get; set; }
    public string? PaymentUrl { get; set; }
    public DateTime? PaidAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public static class PaymentTypes
{
    public const string DEPOSIT = "DEPOSIT";
    public const string REMAINING = "REMAINING";
}

public static class PaymentStatus
{
    public const string PENDING = "PENDING";
    public const string SUCCESS = "SUCCESS";
    public const string FAILED = "FAILED";
    public const string CANCELLED = "CANCELLED";
}

