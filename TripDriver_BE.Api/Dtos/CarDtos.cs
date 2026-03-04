using System.ComponentModel.DataAnnotations;

namespace TripDriver_BE.Api.Dtos;

public class OwnerCreateCarRequest
{
    [Required, MaxLength(100)]
    public string Brand { get; set; } = "";

    [Required, MaxLength(100)]
    public string Model { get; set; } = "";

    public int? Year { get; set; }

    [MaxLength(30)]
    public string? Transmission { get; set; }

    [MaxLength(30)]
    public string? Fuel { get; set; }

    public int? Seats { get; set; }

    [MaxLength(255)]
    public string? Location { get; set; }

    [Range(1, 1_000_000_000)]
    public decimal PricePerDay { get; set; }

    [Range(0, 100)]
    public decimal DepositPercent { get; set; } = 20m;

    public string? Description { get; set; }
}

public class OwnerUpdateCarRequest : OwnerCreateCarRequest { }

