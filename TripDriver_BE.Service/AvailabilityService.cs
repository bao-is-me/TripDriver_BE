using TripDriver_BE.Repo.Data;
using TripDriver_BE.Repo.Entities;
using Microsoft.EntityFrameworkCore;

namespace TripDriver_BE.Service;

public class AvailabilityService
{
    private readonly AppDbContext _db;
    public AvailabilityService(AppDbContext db) => _db = db;

    public Task<bool> HasBlockingOverlapAsync(Guid carId, DateOnly startDate, DateOnly endDate)
    {
        return _db.Bookings.AnyAsync(b =>
            b.CarId == carId &&
            BookingStatus.Blocking.Contains(b.Status) &&
            b.StartDate < endDate &&
            b.EndDate > startDate
        );
    }
}

