using Application.DTOs.Payments;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class PaymentAdminService : IPaymentAdminService
{
    private readonly ApplicationDbContext _dbContext;

    public PaymentAdminService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<PaymentDetailDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Payments
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new PaymentDetailDto
            {
                Id = x.Id,
                InvoiceId = x.InvoiceId,
                InvoiceNumber = x.Invoice.InvoiceNumber,
                UserId = x.Invoice.UserId,
                CustomerFullName = x.Invoice.User.FullName,
                CustomerEmail = x.Invoice.User.Email,
                ProjectId = x.Invoice.ProjectId,
                ProjectTitle = x.Invoice.Project.Title,
                Amount = x.Amount,
                Gateway = x.Gateway,
                Authority = x.Authority,
                RefId = x.RefId,
                Status = x.Status,
                CardPan = x.CardPan,
                TrackingCode = x.TrackingCode,
                GatewayResponse = x.GatewayResponse,
                PaidAt = x.PaidAt,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PaymentDetailDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Payments
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new PaymentDetailDto
            {
                Id = x.Id,
                InvoiceId = x.InvoiceId,
                InvoiceNumber = x.Invoice.InvoiceNumber,
                UserId = x.Invoice.UserId,
                CustomerFullName = x.Invoice.User.FullName,
                CustomerEmail = x.Invoice.User.Email,
                ProjectId = x.Invoice.ProjectId,
                ProjectTitle = x.Invoice.Project.Title,
                Amount = x.Amount,
                Gateway = x.Gateway,
                Authority = x.Authority,
                RefId = x.RefId,
                Status = x.Status,
                CardPan = x.CardPan,
                TrackingCode = x.TrackingCode,
                GatewayResponse = x.GatewayResponse,
                PaidAt = x.PaidAt,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PaymentDetailDto> CreateAsync(
        CreatePaymentRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await ValidateInvoiceAsync(request.InvoiceId, cancellationToken);
        ValidatePaymentFields(request.Amount, request.Gateway);
        ValidatePaymentStatus(request.Status);

        if (request.Status == PaymentStatus.Paid)
        {
            await ValidateApprovedPaymentAmountAsync(
                Guid.Empty,
                request.InvoiceId,
                request.Amount,
                cancellationToken);
        }

        var payment = new Payment
        {
            InvoiceId = request.InvoiceId,
            Amount = request.Amount,
            Gateway = request.Gateway.Trim(),
            Authority = string.IsNullOrWhiteSpace(request.Authority)
                ? GenerateManualAuthority()
                : request.Authority.Trim(),
            RefId = request.RefId?.Trim(),
            Status = request.Status,
            CardPan = request.CardPan?.Trim(),
            TrackingCode = request.TrackingCode?.Trim(),
            GatewayResponse = request.GatewayResponse?.Trim(),
            PaidAt = request.Status == PaymentStatus.Paid
                ? NormalizeUtc(request.PaidAt) ?? DateTime.UtcNow
                : null
        };

        await _dbContext.Payments.AddAsync(payment, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await SyncInvoiceAndProjectPaymentStateAsync(
            payment.InvoiceId,
            cancellationToken
        );

        await _dbContext.SaveChangesAsync(cancellationToken);

        var createdPayment = await GetByIdAsync(
            payment.Id,
            cancellationToken
        );

        return createdPayment!;
    }

    public async Task<PaymentDetailDto?> UpdateAsync(
        Guid id,
        UpdatePaymentRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await ValidateInvoiceAsync(request.InvoiceId, cancellationToken);
        ValidatePaymentFields(request.Amount, request.Gateway);
        ValidatePaymentStatus(request.Status);

        var payment = await _dbContext.Payments
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (payment is null)
        {
            return null;
        }

        EnsurePaymentRecordIsEditable(payment);

        ValidateStatusTransition(payment.Status, request.Status);
        var oldInvoiceId = payment.InvoiceId;

        if (request.Status == PaymentStatus.Paid)
        {
            await ValidateApprovedPaymentAmountAsync(
                payment.Id,
                request.InvoiceId,
                request.Amount,
                cancellationToken);
        }

        payment.InvoiceId = request.InvoiceId;
        payment.Amount = request.Amount;
        payment.Gateway = request.Gateway.Trim();
        payment.Authority = string.IsNullOrWhiteSpace(request.Authority)
            ? payment.Authority
            : request.Authority.Trim();
        payment.RefId = request.RefId?.Trim();
        payment.Status = request.Status;
        payment.CardPan = request.CardPan?.Trim();
        payment.TrackingCode = request.TrackingCode?.Trim();
        payment.GatewayResponse = request.GatewayResponse?.Trim();
        payment.PaidAt = request.Status == PaymentStatus.Paid
            ? NormalizeUtc(request.PaidAt) ?? payment.PaidAt ?? DateTime.UtcNow
            : null;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await SyncInvoiceAndProjectPaymentStateAsync(
            oldInvoiceId,
            cancellationToken
        );

        if (oldInvoiceId != payment.InvoiceId)
        {
            await SyncInvoiceAndProjectPaymentStateAsync(
                payment.InvoiceId,
                cancellationToken
            );
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(payment.Id, cancellationToken);
    }

    public async Task<PaymentDetailDto?> UpdateStatusAsync(
        Guid id,
        UpdatePaymentStatusRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidatePaymentStatus(request.Status);

        var payment = await _dbContext.Payments
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (payment is null)
        {
            return null;
        }

        var previousStatus = payment.Status;
        ValidateStatusTransition(previousStatus, request.Status);

        if (request.Status == PaymentStatus.Paid)
        {
            await ValidateApprovedPaymentAmountAsync(
                payment.Id,
                payment.InvoiceId,
                payment.Amount,
                cancellationToken);
        }

        payment.Status = request.Status;
        payment.RefId = request.RefId?.Trim() ?? payment.RefId;
        payment.CardPan = request.CardPan?.Trim() ?? payment.CardPan;
        payment.TrackingCode = request.TrackingCode?.Trim() ?? payment.TrackingCode;
        payment.GatewayResponse = request.GatewayResponse?.Trim() ?? payment.GatewayResponse;

        payment.PaidAt = request.Status == PaymentStatus.Paid
            ? NormalizeUtc(request.PaidAt) ?? payment.PaidAt ?? DateTime.UtcNow
            : null;

        var syncResult = await SyncInvoiceAndProjectPaymentStateAsync(
            payment.InvoiceId,
            cancellationToken
        );

        if (previousStatus != request.Status && syncResult is not null)
        {
            AddCustomerReviewNotification(syncResult, request.Status);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(payment.Id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var payment = await _dbContext.Payments
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (payment is null)
        {
            return false;
        }

        EnsurePaymentRecordIsEditable(payment);

        var invoiceId = payment.InvoiceId;

        _dbContext.Payments.Remove(payment);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await SyncInvoiceAndProjectPaymentStateAsync(
            invoiceId,
            cancellationToken
        );

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static void EnsurePaymentRecordIsEditable(Payment payment)
    {
        if (payment.Status is PaymentStatus.Paid or PaymentStatus.Refunded)
        {
            throw new InvalidOperationException(
                "Approved or refunded payments cannot be edited or deleted. Change their status through the review action.");
        }
    }

    private async Task ValidateInvoiceAsync(
        Guid invoiceId,
        CancellationToken cancellationToken)
    {
        var invoiceExists = await _dbContext.Invoices
            .AnyAsync(x => x.Id == invoiceId, cancellationToken);

        if (!invoiceExists)
        {
            throw new InvalidOperationException("Invoice was not found.");
        }
    }

    private static void ValidatePaymentFields(
        decimal amount,
        string gateway)
    {
        if (amount <= 0)
        {
            throw new InvalidOperationException("Payment amount must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(gateway))
        {
            throw new InvalidOperationException("Payment gateway is required.");
        }
    }

    private async Task<PaymentSyncResult?> SyncInvoiceAndProjectPaymentStateAsync(
        Guid invoiceId,
        CancellationToken cancellationToken)
    {
        var invoice = await _dbContext.Invoices
            .Include(x => x.Project)
            .FirstOrDefaultAsync(x => x.Id == invoiceId, cancellationToken);

        if (invoice is null)
        {
            return null;
        }

        var payments = await _dbContext.Payments
            .Where(x => x.InvoiceId == invoiceId)
            .ToListAsync(cancellationToken);

        var paidAmountForInvoice = payments
            .Where(x => x.Status == PaymentStatus.Paid)
            .Sum(x => x.Amount);

        if (paidAmountForInvoice >= invoice.FinalAmount &&
            invoice.FinalAmount > 0)
        {
            invoice.Status = PaymentStatus.Paid;
            invoice.PaidAt = payments
                .Where(x => x.Status == PaymentStatus.Paid)
                .OrderByDescending(x => x.PaidAt)
                .Select(x => x.PaidAt)
                .FirstOrDefault() ?? DateTime.UtcNow;
        }
        else if (payments.Any(x => x.Status == PaymentStatus.Pending))
        {
            invoice.Status = PaymentStatus.Pending;
            invoice.PaidAt = null;
        }
        else if (payments.Any(x => x.Status == PaymentStatus.Refunded))
        {
            invoice.Status = PaymentStatus.Refunded;
            invoice.PaidAt = null;
        }
        else if (payments.Any(x => x.Status == PaymentStatus.Failed))
        {
            invoice.Status = PaymentStatus.Failed;
            invoice.PaidAt = null;
        }
        else
        {
            invoice.Status = PaymentStatus.Pending;
            invoice.PaidAt = null;
        }

        // Do not filter by status in SQL here: during review, the changed
        // payment is still tracked and intentionally saved together with the
        // invoice/project state in one atomic SaveChanges call.
        var projectPayments = await _dbContext.Payments
            .Where(x => x.Invoice.ProjectId == invoice.ProjectId)
            .ToListAsync(cancellationToken);
        var projectPaidAmount = projectPayments
            .Where(x => x.Status == PaymentStatus.Paid)
            .Sum(x => x.Amount);

        invoice.Project.PaidAmount = Math.Min(
            invoice.Project.Price,
            projectPaidAmount);

        var projectStarted = false;
        if (invoice.Status == PaymentStatus.Paid &&
            invoice.Project.Status == ProjectStatus.Pending)
        {
            invoice.Project.Status = ProjectStatus.InProgress;
            invoice.Project.StartDate ??= DateTime.UtcNow;
            projectStarted = true;
        }

        return new PaymentSyncResult(
            invoice.UserId,
            invoice.InvoiceNumber,
            invoice.Project.Title,
            invoice.Status == PaymentStatus.Paid,
            projectStarted);
    }

    private async Task ValidateApprovedPaymentAmountAsync(
        Guid paymentId,
        Guid invoiceId,
        decimal paymentAmount,
        CancellationToken cancellationToken)
    {
        var finalAmount = await _dbContext.Invoices
            .Where(item => item.Id == invoiceId)
            .Select(item => (decimal?)item.FinalAmount)
            .FirstOrDefaultAsync(cancellationToken);

        if (!finalAmount.HasValue)
        {
            throw new InvalidOperationException("Invoice was not found.");
        }

        var otherApprovedAmount = await _dbContext.Payments
            .Where(item =>
                item.InvoiceId == invoiceId &&
                item.Id != paymentId &&
                item.Status == PaymentStatus.Paid)
            .SumAsync(item => item.Amount, cancellationToken);

        if (otherApprovedAmount + paymentAmount > finalAmount.Value)
        {
            throw new InvalidOperationException(
                "Approved payments cannot be greater than the invoice final amount.");
        }
    }

    private void AddCustomerReviewNotification(
        PaymentSyncResult result,
        PaymentStatus status)
    {
        var notification = status switch
        {
            PaymentStatus.Paid when result.InvoiceFinalized => new Notification
            {
                UserId = result.UserId,
                Type = NotificationType.Success,
                Title = "پرداخت تأیید و فاکتور نهایی شد",
                Message = result.ProjectStarted
                    ? $"پرداخت فاکتور {result.InvoiceNumber} تأیید شد و پروژه {result.ProjectTitle} وارد مرحله اجرا شد."
                    : $"پرداخت فاکتور {result.InvoiceNumber} تأیید و فاکتور نهایی شد."
            },
            PaymentStatus.Paid => new Notification
            {
                UserId = result.UserId,
                Type = NotificationType.Info,
                Title = "پرداخت تأیید شد",
                Message = $"پرداخت ثبت‌شده برای فاکتور {result.InvoiceNumber} تأیید شد."
            },
            PaymentStatus.Failed => new Notification
            {
                UserId = result.UserId,
                Type = NotificationType.Error,
                Title = "پرداخت تأیید نشد",
                Message = $"پرداخت ثبت‌شده برای فاکتور {result.InvoiceNumber} رد شد؛ اطلاعات پرداخت را بررسی و دوباره ثبت کنید."
            },
            PaymentStatus.Refunded => new Notification
            {
                UserId = result.UserId,
                Type = NotificationType.Warning,
                Title = "پرداخت بازپرداخت شد",
                Message = $"پرداخت فاکتور {result.InvoiceNumber} در وضعیت بازپرداخت قرار گرفت."
            },
            _ => null
        };

        if (notification is not null)
        {
            notification.IsRead = false;
            _dbContext.Notifications.Add(notification);
        }
    }

    private static void ValidatePaymentStatus(PaymentStatus status)
    {
        if (!Enum.IsDefined(typeof(PaymentStatus), status))
        {
            throw new InvalidOperationException("Payment status is invalid.");
        }
    }

    private static void ValidateStatusTransition(
        PaymentStatus currentStatus,
        PaymentStatus nextStatus)
    {
        if (currentStatus == nextStatus)
        {
            return;
        }

        var isAllowed = currentStatus switch
        {
            PaymentStatus.Pending => nextStatus is PaymentStatus.Paid or PaymentStatus.Failed,
            PaymentStatus.Failed => nextStatus is PaymentStatus.Pending or PaymentStatus.Paid,
            PaymentStatus.Paid => nextStatus == PaymentStatus.Refunded,
            PaymentStatus.Refunded => false,
            _ => false
        };

        if (!isAllowed)
        {
            throw new InvalidOperationException(
                $"Payment status cannot change from {currentStatus} to {nextStatus}.");
        }
    }

    private static DateTime? NormalizeUtc(DateTime? value)
    {
        if (!value.HasValue)
        {
            return null;
        }

        return value.Value.Kind switch
        {
            DateTimeKind.Utc => value.Value,
            DateTimeKind.Local => value.Value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
        };
    }

    private static string GenerateManualAuthority()
    {
        return $"MANUAL-{Guid.NewGuid():N}".ToUpperInvariant();
    }

    private sealed record PaymentSyncResult(
        Guid UserId,
        string InvoiceNumber,
        string ProjectTitle,
        bool InvoiceFinalized,
        bool ProjectStarted);
}
