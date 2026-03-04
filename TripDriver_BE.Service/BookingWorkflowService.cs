using TripDriver_BE.Repo.Data;
using TripDriver_BE.Repo.Entities;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace TripDriver_BE.Service;

public class BookingWorkflowService
{
    private readonly AppDbContext _db;
    private readonly AvailabilityService _availability;

    public BookingWorkflowService(AppDbContext db, AvailabilityService availability)
    {
        _db = db;
        _availability = availability;
    }

    private static readonly Dictionary<string, HashSet<string>> AllowedTransitions = new()
    {
        [BookingStatus.PENDING_DEPOSIT] = new() { BookingStatus.DEPOSIT_PAID, BookingStatus.CANCELLED },
        [BookingStatus.DEPOSIT_PAID] = new() { BookingStatus.OWNER_CONFIRMED, BookingStatus.OWNER_REJECTED, BookingStatus.CANCELLED },
        [BookingStatus.OWNER_CONFIRMED] = new() { BookingStatus.IN_PROGRESS },
        [BookingStatus.IN_PROGRESS] = new() { BookingStatus.RETURN_OK_PENDING_PAYMENT, BookingStatus.DISPUTE },
        [BookingStatus.RETURN_OK_PENDING_PAYMENT] = new() { BookingStatus.COMPLETED },
    };

    private static void EnsureTransition(string from, string to)
    {
        if (!AllowedTransitions.TryGetValue(from, out var allowed) || !allowed.Contains(to))
            throw new InvalidOperationException($"Invalid transition: {from} -> {to}");
    }

    private async Task LogStatusAsync(Guid bookingId, string? from, string to, Guid? changedBy, string? note)
    {
        _db.BookingStatusHistory.Add(new BookingStatusHistory
        {
            BookingId = bookingId,
            FromStatus = from,
            ToStatus = to,
            ChangedBy = changedBy,
            Note = note,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
    }

    private async Task ChangeStatusAsync(Booking booking, string to, Guid actorId, string? note)
    {
        EnsureTransition(booking.Status, to);
        var from = booking.Status;
        booking.Status = to;
        booking.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        await LogStatusAsync(booking.Id, from, to, actorId, note);
    }

    public async Task<Booking> CreateBookingAsync(Guid customerId, Guid carId, DateOnly startDate, DateOnly endDate, string? pickupNote, string? customerNote)
    {
        if (endDate < startDate) throw new InvalidOperationException("EndDate must be >= StartDate");

        var car = await _db.Cars.FirstOrDefaultAsync(c => c.Id == carId && c.Status == CarStatus.ACTIVE)
            ?? throw new InvalidOperationException("Car not found or not ACTIVE");

        // Pricing: (days) * PricePerDay
        var days = (endDate.ToDateTime(TimeOnly.MinValue) - startDate.ToDateTime(TimeOnly.MinValue)).Days;
        if (days <= 0) throw new InvalidOperationException("Invalid rental range (must be at least 1 day)");

        var total = Math.Round(car.PricePerDay * days, 2);
        var deposit = Math.Round(total * car.DepositPercent / 100m, 2);
        var remaining = total - deposit;

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            BookingCode = GenerateBookingCode(),
            CarId = car.Id,
            OwnerId = car.OwnerId,
            CustomerId = customerId,
            StartDate = startDate,
            EndDate = endDate,
            PickupNote = pickupNote,
            CustomerNote = customerNote,
            Status = BookingStatus.PENDING_DEPOSIT,
            TotalAmount = total,
            DepositAmount = deposit,
            RemainingAmount = remaining,
            Currency = "VND",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();
        await LogStatusAsync(booking.Id, null, BookingStatus.PENDING_DEPOSIT, customerId, "Booking created");

        return booking;
    }

    // Called by payment webhook when DEPOSIT payment SUCCESS
    public async Task MarkDepositPaidAsync(Guid bookingId, Guid actorCustomerId)
    {
        using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId)
            ?? throw new KeyNotFoundException("Booking not found");

        if (booking.CustomerId != actorCustomerId)
            throw new UnauthorizedAccessException("Not your booking");

        EnsureTransition(booking.Status, BookingStatus.DEPOSIT_PAID);

        // double-book protection at the moment of holding the schedule
        var blocked = await _availability.HasBlockingOverlapAsync(booking.CarId, booking.StartDate, booking.EndDate);
        if (blocked)
            throw new InvalidOperationException("Car is not available for selected dates");

        var from = booking.Status;
        booking.Status = BookingStatus.DEPOSIT_PAID;
        booking.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        await LogStatusAsync(booking.Id, from, BookingStatus.DEPOSIT_PAID, actorCustomerId, "Deposit paid");

        await tx.CommitAsync();
    }

    // Called by payment webhook when REMAINING payment SUCCESS
    public async Task MarkRemainingPaidAsync(Guid bookingId, Guid actorCustomerId)
    {
        var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId)
            ?? throw new KeyNotFoundException("Booking not found");

        if (booking.CustomerId != actorCustomerId)
            throw new UnauthorizedAccessException("Not your booking");

        await ChangeStatusAsync(booking, BookingStatus.COMPLETED, actorCustomerId, "Remaining paid");
    }

    public async Task OwnerConfirmAsync(Guid bookingId, Guid ownerId)
    {
        var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId)
            ?? throw new KeyNotFoundException("Booking not found");

        if (booking.OwnerId != ownerId)
            throw new UnauthorizedAccessException("Not your booking");

        await ChangeStatusAsync(booking, BookingStatus.OWNER_CONFIRMED, ownerId, "Owner confirmed");
    }

    public async Task OwnerRejectAsync(Guid bookingId, Guid ownerId, string? reason)
    {
        var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId)
            ?? throw new KeyNotFoundException("Booking not found");

        if (booking.OwnerId != ownerId)
            throw new UnauthorizedAccessException("Not your booking");

        await ChangeStatusAsync(booking, BookingStatus.OWNER_REJECTED, ownerId, reason ?? "Owner rejected");
    }

    public async Task OwnerHandOverAsync(Guid bookingId, Guid ownerId)
    {
        var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId)
            ?? throw new KeyNotFoundException("Booking not found");

        if (booking.OwnerId != ownerId)
            throw new UnauthorizedAccessException("Not your booking");

        await ChangeStatusAsync(booking, BookingStatus.IN_PROGRESS, ownerId, "Hand-over");
    }

    public async Task OwnerReturnOkAsync(Guid bookingId, Guid ownerId)
    {
        var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId)
            ?? throw new KeyNotFoundException("Booking not found");

        if (booking.OwnerId != ownerId)
            throw new UnauthorizedAccessException("Not your booking");

        await ChangeStatusAsync(booking, BookingStatus.RETURN_OK_PENDING_PAYMENT, ownerId, "Return OK");
    }

    public async Task OwnerReportDamageAsync(Guid bookingId, Guid ownerId, string? note)
    {
        var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId)
            ?? throw new KeyNotFoundException("Booking not found");

        if (booking.OwnerId != ownerId)
            throw new UnauthorizedAccessException("Not your booking");

        await ChangeStatusAsync(booking, BookingStatus.DISPUTE, ownerId, note ?? "Damage reported");
    }

    public async Task AdminCancelAsync(Guid bookingId, Guid adminId, string? reason)
    {
        var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId)
            ?? throw new KeyNotFoundException("Booking not found");

        // As required: cancel only early stage (at least before IN_PROGRESS)
        if (booking.Status == BookingStatus.IN_PROGRESS ||
            booking.Status == BookingStatus.RETURN_OK_PENDING_PAYMENT ||
            booking.Status == BookingStatus.COMPLETED)
        {
            throw new InvalidOperationException("Cannot cancel at this stage");
        }

        // Allow cancelling from statuses that have rule in FSM
        if (booking.Status != BookingStatus.PENDING_DEPOSIT && booking.Status != BookingStatus.DEPOSIT_PAID)
            throw new InvalidOperationException("Admin cancel is allowed only in early stages");

        await ChangeStatusAsync(booking, BookingStatus.CANCELLED, adminId, reason ?? "Admin cancelled");
    }

    private static string GenerateBookingCode()
    {
        // BK + yyyyMMddHHmmss + random
        return $"BK{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(100, 999)}";
    }
}

