using TripDriver_BE.Repo.Data;
using TripDriver_BE.Repo.Entities;
using Microsoft.EntityFrameworkCore;

namespace TripDriver_BE.Service;

public class PaymentService
{
    private readonly AppDbContext _db;
    private readonly BookingWorkflowService _workflow;
    private const string MockMethod = "MOCK";

    public PaymentService(AppDbContext db, BookingWorkflowService workflow)
    {
        _db = db;
        _workflow = workflow;
    }

    public async Task<Payment> CreatePaymentIntentAsync(Guid bookingId, Guid customerId, string type, string method)
    {
        type = type.Trim().ToUpperInvariant();
        if (type is not (PaymentTypes.DEPOSIT or PaymentTypes.REMAINING))
            throw new InvalidOperationException("Type must be DEPOSIT or REMAINING");

        var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId)
            ?? throw new KeyNotFoundException("Booking not found");

        if (booking.CustomerId != customerId)
            throw new UnauthorizedAccessException("Not your booking");

        // Idempotency: 1 booking has 1 payment per type
        var normalizedMethod = string.IsNullOrWhiteSpace(method) ? MockMethod : method.Trim().ToUpperInvariant();
        var existing = await _db.Payments.FirstOrDefaultAsync(p => p.BookingId == bookingId && p.Type == type);
        if (existing != null)
        {
            if (normalizedMethod == MockMethod && existing.Status != PaymentStatus.SUCCESS)
            {
                existing.Method = MockMethod;
                await MarkPaymentSuccessAsync(existing, booking);
            }

            return existing;
        }

        var amount = type == PaymentTypes.DEPOSIT ? booking.DepositAmount : booking.RemainingAmount;

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            BookingId = bookingId,
            Type = type,
            Status = PaymentStatus.PENDING,
            Method = normalizedMethod,
            Amount = amount,
            Currency = booking.Currency,
            ProviderTxnId = null,
            PaymentUrl = $"https://mock-payments.local/pay?bookingId={bookingId}&type={type}",
            PaidAt = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();

        if (normalizedMethod == MockMethod)
        {
            await MarkPaymentSuccessAsync(payment, booking);
        }

        return payment;
    }

    public async Task<object> HandleWebhookAsync(PaymentWebhookRequest req)
    {
        var type = req.Type.Trim().ToUpperInvariant();
        var status = req.Status.Trim().ToUpperInvariant();

        if (type is not (PaymentTypes.DEPOSIT or PaymentTypes.REMAINING))
            throw new InvalidOperationException("Type must be DEPOSIT or REMAINING");

        if (status is not (PaymentStatus.SUCCESS or PaymentStatus.FAILED))
            throw new InvalidOperationException("Status must be SUCCESS or FAILED");

        var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == req.BookingId)
            ?? throw new KeyNotFoundException("Booking not found");

        var payment = await _db.Payments.FirstOrDefaultAsync(p => p.BookingId == req.BookingId && p.Type == type)
            ?? throw new InvalidOperationException("Payment intent not found. Create intent first.");

        // Idempotent: if already SUCCESS, do nothing
        if (payment.Status == PaymentStatus.SUCCESS)
        {
            return new { ok = true, message = "Already processed" };
        }

        payment.Status = status;
        payment.ProviderTxnId = req.ExternalTxnId;
        payment.UpdatedAt = DateTime.UtcNow;

        if (status == PaymentStatus.SUCCESS)
        {
            payment.PaidAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            // Trigger booking workflow
            if (type == PaymentTypes.DEPOSIT)
            {
                await _workflow.MarkDepositPaidAsync(booking.Id, booking.CustomerId);
            }
            else
            {
                await _workflow.MarkRemainingPaidAsync(booking.Id, booking.CustomerId);
            }

            return new { ok = true, message = "Payment success processed" };
        }

        await _db.SaveChangesAsync();
        return new { ok = true, message = "Payment failed recorded" };
    }

    private async Task MarkPaymentSuccessAsync(Payment payment, Booking booking)
    {
        if (payment.Status == PaymentStatus.SUCCESS)
            return;

        payment.Status = PaymentStatus.SUCCESS;
        payment.PaidAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;
        payment.ProviderTxnId ??= $"MOCK-{payment.Id:N}";

        await _db.SaveChangesAsync();

        if (payment.Type == PaymentTypes.DEPOSIT)
        {
            await _workflow.MarkDepositPaidAsync(booking.Id, booking.CustomerId);
        }
        else
        {
            await _workflow.MarkRemainingPaidAsync(booking.Id, booking.CustomerId);
        }
    }
}


