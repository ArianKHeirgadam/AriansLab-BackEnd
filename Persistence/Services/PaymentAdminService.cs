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
                ? request.PaidAt ?? DateTime.UtcNow
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

        var payment = await _dbContext.Payments
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (payment is null)
        {
            return null;
        }

        var oldInvoiceId = payment.InvoiceId;

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
            ? request.PaidAt ?? payment.PaidAt ?? DateTime.UtcNow
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
        var payment = await _dbContext.Payments
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (payment is null)
        {
            return null;
        }

        payment.Status = request.Status;
        payment.RefId = request.RefId?.Trim() ?? payment.RefId;
        payment.CardPan = request.CardPan?.Trim() ?? payment.CardPan;
        payment.TrackingCode = request.TrackingCode?.Trim() ?? payment.TrackingCode;
        payment.GatewayResponse = request.GatewayResponse?.Trim() ?? payment.GatewayResponse;

        payment.PaidAt = request.Status == PaymentStatus.Paid
            ? request.PaidAt ?? payment.PaidAt ?? DateTime.UtcNow
            : null;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await SyncInvoiceAndProjectPaymentStateAsync(
            payment.InvoiceId,
            cancellationToken
        );

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

    private async Task SyncInvoiceAndProjectPaymentStateAsync(
        Guid invoiceId,
        CancellationToken cancellationToken)
    {
        var invoice = await _dbContext.Invoices
            .Include(x => x.Project)
            .FirstOrDefaultAsync(x => x.Id == invoiceId, cancellationToken);

        if (invoice is null)
        {
            return;
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

        var projectPaidAmount = await _dbContext.Payments
            .Where(x =>
                x.Status == PaymentStatus.Paid &&
                x.Invoice.ProjectId == invoice.ProjectId)
            .SumAsync(x => x.Amount, cancellationToken);

        invoice.Project.PaidAmount = projectPaidAmount;
    }

    private static string GenerateManualAuthority()
    {
        return $"MANUAL-{Guid.NewGuid():N}".ToUpperInvariant();
    }
}