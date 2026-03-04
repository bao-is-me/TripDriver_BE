namespace TripDriver_BE.Repo.Entities;

public class Booking
{
    public Guid Id { get; set; }
    public string BookingCode { get; set; } = "";

    public Guid CarId { get; set; }
    public Guid OwnerId { get; set; }
    public Guid CustomerId { get; set; }

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public string? PickupNote { get; set; }
    public string? CustomerNote { get; set; }

    public string Status { get; set; } = BookingStatus.PENDING_DEPOSIT;

    public decimal TotalAmount { get; set; }
    public decimal DepositAmount { get; set; }
    public decimal RemainingAmount { get; set; }

    public string Currency { get; set; } = "VND";

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public static class BookingStatus
{
    public const string PENDING_DEPOSIT = "PENDING_DEPOSIT";
    public const string DEPOSIT_PAID = "DEPOSIT_PAID";
    public const string OWNER_CONFIRMED = "OWNER_CONFIRMED";
    public const string OWNER_REJECTED = "OWNER_REJECTED";
    public const string IN_PROGRESS = "IN_PROGRESS";
    public const string RETURN_OK_PENDING_PAYMENT = "RETURN_OK_PENDING_PAYMENT";
    public const string COMPLETED = "COMPLETED";
    public const string DISPUTE = "DISPUTE";
    public const string CANCELLED = "CANCELLED";

    public static readonly HashSet<string> Blocking = new()
    {
        DEPOSIT_PAID,
        OWNER_CONFIRMED,
        IN_PROGRESS,
        RETURN_OK_PENDING_PAYMENT
    };
}

