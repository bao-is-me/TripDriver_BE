namespace TripDriver_BE.Repo.Entities;

public class CarReview
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid CarId { get; set; }
    public Guid OwnerId { get; set; }
    public Guid CustomerId { get; set; }

    public int Rating { get; set; }
    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

