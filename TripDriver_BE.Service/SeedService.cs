using Microsoft.Extensions.Logging;
using BCrypt.Net;
using TripDriver_BE.Repo.Data;
using TripDriver_BE.Repo.Entities;
using Microsoft.EntityFrameworkCore;

namespace TripDriver_BE.Service;

public class SeedService
{
    private readonly AppDbContext _db;
    private readonly ILogger<SeedService> _logger;

    public SeedService(AppDbContext db, ILogger<SeedService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        // Ensure DB exists (the schema is created by exe222.sql)
        // If you want EF migrations, you can add them later.

        if (!await _db.Users.AnyAsync())
        {
            _logger.LogInformation("Seeding users...");

            var admin = new User
            {
                Id = Guid.NewGuid(),
                Email = "admin@demo.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role = UserRoles.ADMIN,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var owner = new User
            {
                Id = Guid.NewGuid(),
                Email = "owner@demo.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Owner123!"),
                Role = UserRoles.OWNER,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var customer = new User
            {
                Id = Guid.NewGuid(),
                Email = "customer@demo.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Customer123!"),
                Role = UserRoles.CUSTOMER,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Users.AddRange(admin, owner, customer);
            _db.UserProfiles.AddRange(
                new UserProfile { UserId = admin.Id, FullName = "Admin Demo", Phone = "0900000001", City = "HCM", Address = "", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new UserProfile { UserId = owner.Id, FullName = "Owner Demo", Phone = "0900000002", City = "HCM", Address = "", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new UserProfile { UserId = customer.Id, FullName = "Customer Demo", Phone = "0900000003", City = "HCM", Address = "", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            );

            await _db.SaveChangesAsync();

            _logger.LogInformation("Seeding cars...");

            var car1 = new Car
            {
                Id = Guid.NewGuid(),
                OwnerId = owner.Id,
                Brand = "Toyota",
                Model = "Vios",
                Year = 2020,
                Transmission = "AT",
                Fuel = "Gasoline",
                Seats = 5,
                Location = "Ho Chi Minh City",
                PricePerDay = 600000,
                DepositPercent = 20m,
                Description = "Demo car 1",
                Status = CarStatus.ACTIVE,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var car2 = new Car
            {
                Id = Guid.NewGuid(),
                OwnerId = owner.Id,
                Brand = "Honda",
                Model = "City",
                Year = 2021,
                Transmission = "AT",
                Fuel = "Gasoline",
                Seats = 5,
                Location = "Ho Chi Minh City",
                PricePerDay = 650000,
                DepositPercent = 20m,
                Description = "Demo car 2",
                Status = CarStatus.ACTIVE,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Cars.AddRange(car1, car2);
            await _db.SaveChangesAsync();

            _db.CarImages.AddRange(
                new CarImage { Id = Guid.NewGuid(), CarId = car1.Id, ImageUrl = "https://picsum.photos/seed/car1/800/600", SortOrder = 0, CreatedAt = DateTime.UtcNow },
                new CarImage { Id = Guid.NewGuid(), CarId = car2.Id, ImageUrl = "https://picsum.photos/seed/car2/800/600", SortOrder = 0, CreatedAt = DateTime.UtcNow }
            );

            await _db.SaveChangesAsync();
        }
    }
}


