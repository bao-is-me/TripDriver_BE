using System.ComponentModel.DataAnnotations;

namespace TripDriver_BE.Api.Dtos;

public class CreateBookingRequest
{
    [Required]
    public Guid CarId { get; set; }

    [Required]
    public DateOnly StartDate { get; set; }

    [Required]
    public DateOnly EndDate { get; set; }

    public string? PickupNote { get; set; }
    public string? CustomerNote { get; set; }
}

public class RejectBookingRequest
{
    public string? Reason { get; set; }
}

public class CancelBookingRequest
{
    public string? Reason { get; set; }
}

