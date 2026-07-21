using Application.DTOs.Payments;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public sealed class PaymentSubmissionService : IPaymentSubmissionService
{
    private const string BankTransferGateway = "BankTransfer";
    private readonly ApplicationDbContext _dbContext;

    public PaymentSubmissionService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaymentDetailDto?> SubmitAsync(
        Guid userId,
        SubmitPaymentRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("Authenticated user id is required.");
        }

        if (request.InvoiceId == Guid.Empty)
        {
            throw new ArgumentException("Invoice id is required.");
        }

        var trackingCode = NormalizeDigits(request.TrackingCode ?? string.Empty).Trim();
        ValidateTrackingCode(trackingCode);

        var cardLastFour = NormalizeCardLastFour(request.CardLastFour);
        var invoice = await _dbContext.Invoices
            .Include(item => item.User)
            .Include(item => item.Project)
            .Include(item => item.Payments)
            .FirstOrDefaultAsync(
                item => item.Id == request.InvoiceId && item.UserId == userId,
                cancellationToken);

        if (invoice is null)
        {
            return null;
        }

        if (invoice.Status == PaymentStatus.Paid)
        {
            throw new InvalidOperationException("This invoice is already finalized and paid.");
        }

        if (invoice.Status == PaymentStatus.Refunded)
        {
            throw new InvalidOperationException("A refunded invoice cannot receive a new payment.");
        }

        if (invoice.Payments.Any(item => item.Status == PaymentStatus.Pending))
        {
            throw new InvalidOperationException(
                "A payment for this invoice is already waiting for administrator approval.");
        }

        var trackingCodeExists = await _dbContext.Payments.AnyAsync(
            item => item.TrackingCode == trackingCode &&
                    item.Status != PaymentStatus.Failed,
            cancellationToken);

        if (trackingCodeExists)
        {
            throw new InvalidOperationException("This payment tracking code has already been submitted.");
        }

        var approvedAmount = invoice.Payments
            .Where(item => item.Status == PaymentStatus.Paid)
            .Sum(item => item.Amount);
        var remainingAmount = invoice.FinalAmount - approvedAmount;

        if (remainingAmount <= 0)
        {
            throw new InvalidOperationException("This invoice has no remaining payable amount.");
        }

        var payment = new Payment
        {
            InvoiceId = invoice.Id,
            Amount = remainingAmount,
            Gateway = BankTransferGateway,
            Authority = $"CUSTOMER-{Guid.NewGuid():N}".ToUpperInvariant(),
            TrackingCode = trackingCode,
            CardPan = cardLastFour is null ? null : $"****-****-****-{cardLastFour}",
            Status = PaymentStatus.Pending
        };

        invoice.Status = PaymentStatus.Pending;
        invoice.PaidAt = null;
        _dbContext.Payments.Add(payment);

        var administratorIds = await _dbContext.Users
            .AsNoTracking()
            .Where(item => item.IsActive && item.Role == UserRole.Admin)
            .Select(item => item.Id)
            .ToListAsync(cancellationToken);

        foreach (var administratorId in administratorIds)
        {
            _dbContext.Notifications.Add(new Notification
            {
                UserId = administratorId,
                Type = NotificationType.Warning,
                Title = "پرداخت جدید در انتظار تأیید",
                Message = $"پرداخت فاکتور {invoice.InvoiceNumber} با کد پیگیری {trackingCode} ثبت شد.",
                IsRead = false
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new PaymentDetailDto
        {
            Id = payment.Id,
            InvoiceId = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            UserId = invoice.UserId,
            CustomerFullName = invoice.User.FullName,
            CustomerEmail = invoice.User.Email,
            ProjectId = invoice.ProjectId,
            ProjectTitle = invoice.Project.Title,
            Amount = payment.Amount,
            Gateway = payment.Gateway,
            Authority = payment.Authority,
            Status = payment.Status,
            CardPan = payment.CardPan,
            TrackingCode = payment.TrackingCode,
            CreatedAt = payment.CreatedAt
        };
    }

    private static void ValidateTrackingCode(string trackingCode)
    {
        if (trackingCode.Length is < 4 or > 100)
        {
            throw new ArgumentException("Payment tracking code must be between 4 and 100 characters.");
        }

        if (!trackingCode.Any(char.IsDigit) || trackingCode.Any(character =>
                !char.IsLetterOrDigit(character) &&
                character is not '-' and not '_' and not '/'))
        {
            throw new ArgumentException("Payment tracking code contains invalid characters.");
        }
    }

    private static string? NormalizeCardLastFour(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = NormalizeDigits(value).Trim();
        if (normalized.Length != 4 || normalized.Any(character => !char.IsDigit(character)))
        {
            throw new ArgumentException("Card number must contain only its last four digits.");
        }

        return normalized;
    }

    private static string NormalizeDigits(string value)
    {
        return string.Concat(value.Select(character => character switch
        {
            >= '\u06F0' and <= '\u06F9' => (char)('0' + character - '\u06F0'),
            >= '\u0660' and <= '\u0669' => (char)('0' + character - '\u0660'),
            _ => character
        }));
    }
}
