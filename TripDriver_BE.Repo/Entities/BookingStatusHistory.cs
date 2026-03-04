namespace TripDriver_BE.Repo.Entities;

public class BookingStatusHistory
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }

    public string? FromStatus { get; set; }
    public string ToStatus { get; set; } = "";

    public Guid? ChangedBy { get; set; }
    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }
}

