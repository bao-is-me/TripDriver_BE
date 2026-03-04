namespace TripDriver_BE.Repo.Entities;

public class CarImage
{
    public Guid Id { get; set; }
    public Guid CarId { get; set; }
    public string ImageUrl { get; set; } = "";
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}

