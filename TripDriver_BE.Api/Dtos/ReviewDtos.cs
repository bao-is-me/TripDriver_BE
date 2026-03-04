using System.ComponentModel.DataAnnotations;

namespace TripDriver_BE.Api.Dtos;

public class CreateReviewRequest
{
    [Required]
    public Guid BookingId { get; set; }

    [Range(1,5)]
    public int Rating { get; set; }

    [MaxLength(2000)]
    public string? Comment { get; set; }
}

