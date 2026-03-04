using TripDriver_BE.Repo.Entities;
using Microsoft.EntityFrameworkCore;

namespace TripDriver_BE.Repo.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Car> Cars => Set<Car>();
    public DbSet<CarImage> CarImages => Set<CarImage>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingStatusHistory> BookingStatusHistory => Set<BookingStatusHistory>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<CarReview> CarReviews => Set<CarReview>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dbo");

        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("Users");
            e.HasKey(x => x.Id);
            e.Property(x => x.Email).HasMaxLength(255).IsRequired();
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Role).HasMaxLength(20).IsRequired();
            e.Property(x => x.PasswordHash).IsRequired();
        });

        modelBuilder.Entity<UserProfile>(e =>
        {
            e.ToTable("UserProfiles");
            e.HasKey(x => x.UserId);
            e.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            e.Property(x => x.Phone).HasMaxLength(30).IsRequired();
            e.HasIndex(x => x.Phone).IsUnique();
            e.HasOne<User>().WithOne().HasForeignKey<UserProfile>(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Car>(e =>
        {
            e.ToTable("Cars");
            e.HasKey(x => x.Id);
            e.Property(x => x.Brand).HasMaxLength(100).IsRequired();
            e.Property(x => x.Model).HasMaxLength(100).IsRequired();
            e.Property(x => x.Status).HasMaxLength(30).IsRequired();
            e.HasIndex(x => x.OwnerId);
            e.HasIndex(x => x.Status);
        });

        modelBuilder.Entity<CarImage>(e =>
        {
            e.ToTable("CarImages");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.CarId);
        });

        modelBuilder.Entity<Booking>(e =>
        {
            e.ToTable("Bookings");
            e.HasKey(x => x.Id);
            e.Property(x => x.BookingCode).HasMaxLength(30).IsRequired();
            e.HasIndex(x => x.BookingCode).IsUnique();
            e.Property(x => x.Status).HasMaxLength(40).IsRequired();
            e.Property(x => x.Currency).HasMaxLength(10).IsRequired();
            e.HasIndex(x => x.CustomerId);
            e.HasIndex(x => x.OwnerId);
            e.HasIndex(x => x.CarId);
            e.HasIndex(x => x.Status);
        });

        modelBuilder.Entity<BookingStatusHistory>(e =>
        {
            e.ToTable("BookingStatusHistory");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.BookingId);
        });

        modelBuilder.Entity<Payment>(e =>
        {
            e.ToTable("Payments");
            e.HasKey(x => x.Id);
            e.Property(x => x.Type).HasMaxLength(20).IsRequired();
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.Property(x => x.Method).HasMaxLength(30).IsRequired();
            e.HasIndex(x => new { x.BookingId, x.Type }).IsUnique();
            e.HasIndex(x => x.Status);
        });

        modelBuilder.Entity<CarReview>(e =>
        {
            e.ToTable("CarReviews");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.BookingId).IsUnique();
            e.HasIndex(x => new { x.CarId, x.CreatedAt });
        });

        base.OnModelCreating(modelBuilder);
    }
}

