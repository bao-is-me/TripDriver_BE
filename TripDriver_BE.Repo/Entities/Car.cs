namespace TripDriver_BE.Repo.Entities;

public class Car
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }

    public string Brand { get; set; } = "";
    public string Model { get; set; } = "";
    public int? Year { get; set; }
    public string? Transmission { get; set; }
    public string? Fuel { get; set; }
    public int? Seats { get; set; }
    public string? Location { get; set; }

    public decimal PricePerDay { get; set; }
    public decimal DepositPercent { get; set; } = 20m;
    public string? Description { get; set; }

    /// <summary>
    /// DRAFT | PENDING_APPROVAL | ACTIVE | INACTIVE
    /// </summary>
    public string Status { get; set; } = CarStatus.DRAFT;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public static class CarStatus
{
    public const string DRAFT = "DRAFT";
    public const string PENDING_APPROVAL = "PENDING_APPROVAL";
    public const string ACTIVE = "ACTIVE";
    public const string INACTIVE = "INACTIVE";
}

