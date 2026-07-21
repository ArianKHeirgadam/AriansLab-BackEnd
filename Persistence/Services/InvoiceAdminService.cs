using Application.DTOs.Invoices;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class InvoiceAdminService : IInvoiceAdminService
{
    private readonly ApplicationDbContext _dbContext;

    public InvoiceAdminService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<InvoiceDetailDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Invoices
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Project)
            .Include(x => x.Payments)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new InvoiceDetailDto
            {
                Id = x.Id,
                UserId = x.UserId,
                CustomerFullName = x.User.FullName,
                CustomerEmail = x.User.Email,
                ProjectId = x.ProjectId,
                ProjectTitle = x.Project.Title,
                ProjectCode = x.Project.ProjectCode,
                InvoiceNumber = x.InvoiceNumber,
                Amount = x.Amount,
                DiscountAmount = x.DiscountAmount,
                TaxAmount = x.TaxAmount,
                FinalAmount = x.FinalAmount,
                Status = x.Status,
                IsPaid = x.IsPaid,
                IsFinalized = x.Status == PaymentStatus.Paid,
                HasPendingPayment = x.Payments.Any(p => p.Status == PaymentStatus.Pending),
                PaidAmount = x.Payments
                    .Where(p => p.Status == PaymentStatus.Paid)
                    .Sum(p => p.Amount),
                RemainingAmount = x.FinalAmount - x.Payments
                    .Where(p => p.Status == PaymentStatus.Paid)
                    .Sum(p => p.Amount) > 0
                        ? x.FinalAmount - x.Payments
                            .Where(p => p.Status == PaymentStatus.Paid)
                            .Sum(p => p.Amount)
                        : 0,
                Description = x.Description,
                PaidAt = x.PaidAt,
                DueDate = x.DueDate,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                Payments = x.Payments
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(p => new InvoicePaymentDto
                    {
                        Id = p.Id,
                        Amount = p.Amount,
                        Gateway = p.Gateway,
                        Authority = p.Authority,
                        RefId = p.RefId,
                        Status = p.Status,
                        TrackingCode = p.TrackingCode,
                        PaidAt = p.PaidAt,
                        CreatedAt = p.CreatedAt
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<InvoiceDetailDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Invoices
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Project)
            .Include(x => x.Payments)
            .Where(x => x.Id == id)
            .Select(x => new InvoiceDetailDto
            {
                Id = x.Id,
                UserId = x.UserId,
                CustomerFullName = x.User.FullName,
                CustomerEmail = x.User.Email,
                ProjectId = x.ProjectId,
                ProjectTitle = x.Project.Title,
                ProjectCode = x.Project.ProjectCode,
                InvoiceNumber = x.InvoiceNumber,
                Amount = x.Amount,
                DiscountAmount = x.DiscountAmount,
                TaxAmount = x.TaxAmount,
                FinalAmount = x.FinalAmount,
                Status = x.Status,
                IsPaid = x.IsPaid,
                IsFinalized = x.Status == PaymentStatus.Paid,
                HasPendingPayment = x.Payments.Any(p => p.Status == PaymentStatus.Pending),
                PaidAmount = x.Payments
                    .Where(p => p.Status == PaymentStatus.Paid)
                    .Sum(p => p.Amount),
                RemainingAmount = x.FinalAmount - x.Payments
                    .Where(p => p.Status == PaymentStatus.Paid)
                    .Sum(p => p.Amount) > 0
                        ? x.FinalAmount - x.Payments
                            .Where(p => p.Status == PaymentStatus.Paid)
                            .Sum(p => p.Amount)
                        : 0,
                Description = x.Description,
                PaidAt = x.PaidAt,
                DueDate = x.DueDate,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                Payments = x.Payments
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(p => new InvoicePaymentDto
                    {
                        Id = p.Id,
                        Amount = p.Amount,
                        Gateway = p.Gateway,
                        Authority = p.Authority,
                        RefId = p.RefId,
                        Status = p.Status,
                        TrackingCode = p.TrackingCode,
                        PaidAt = p.PaidAt,
                        CreatedAt = p.CreatedAt
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<InvoiceDetailDto> CreateAsync(
        CreateInvoiceRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var projectOwnerId = await ValidateAndGetProjectOwnerAsync(
            request.UserId,
            request.ProjectId,
            cancellationToken
        );

        ValidateAmounts(
            request.Amount,
            request.DiscountAmount,
            request.TaxAmount
        );

        if (request.Status != PaymentStatus.Pending)
        {
            throw new InvalidOperationException(
                "A new invoice must start as a provisional pending invoice.");
        }

        var dueDate = NormalizeRequiredUtc(request.DueDate, "Invoice due date");

        var invoiceNumber = string.IsNullOrWhiteSpace(request.InvoiceNumber)
            ? await GenerateInvoiceNumberAsync(cancellationToken)
            : request.InvoiceNumber.Trim().ToUpperInvariant();

        var invoiceNumberExists = await _dbContext.Invoices
            .IgnoreQueryFilters()
            .AnyAsync(x => x.InvoiceNumber == invoiceNumber, cancellationToken);

        if (invoiceNumberExists)
        {
            throw new InvalidOperationException("An invoice with this invoice number already exists.");
        }

        var finalAmount = CalculateFinalAmount(
            request.Amount,
            request.DiscountAmount,
            request.TaxAmount
        );

        var invoice = new Invoice
        {
            UserId = projectOwnerId,
            ProjectId = request.ProjectId,
            InvoiceNumber = invoiceNumber,
            Amount = request.Amount,
            DiscountAmount = request.DiscountAmount,
            TaxAmount = request.TaxAmount,
            FinalAmount = finalAmount,
            Status = PaymentStatus.Pending,
            Description = request.Description?.Trim(),
            DueDate = dueDate,
            PaidAt = null,
            IsDeletedInvoice = false
        };

        await _dbContext.Invoices.AddAsync(invoice, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var createdInvoice = await GetByIdAsync(
            invoice.Id,
            cancellationToken
        );

        return createdInvoice!;
    }

    public async Task<InvoiceDetailDto?> UpdateAsync(
        Guid id,
        UpdateInvoiceRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var projectOwnerId = await ValidateAndGetProjectOwnerAsync(
            request.UserId,
            request.ProjectId,
            cancellationToken
        );

        ValidateAmounts(
            request.Amount,
            request.DiscountAmount,
            request.TaxAmount
        );

        ValidatePaymentStatus(request.Status);
        var dueDate = NormalizeRequiredUtc(request.DueDate, "Invoice due date");

        var invoice = await _dbContext.Invoices
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (invoice is null)
        {
            return null;
        }

        EnsureInvoiceIsEditable(invoice);

        var finalAmount = CalculateFinalAmount(
            request.Amount,
            request.DiscountAmount,
            request.TaxAmount);

        if (request.Status == PaymentStatus.Paid)
        {
            await EnsureInvoiceIsFullyPaidAsync(
                invoice.Id,
                finalAmount,
                cancellationToken);
        }

        invoice.UserId = projectOwnerId;
        invoice.ProjectId = request.ProjectId;
        invoice.Amount = request.Amount;
        invoice.DiscountAmount = request.DiscountAmount;
        invoice.TaxAmount = request.TaxAmount;
        invoice.FinalAmount = finalAmount;
        invoice.Status = request.Status;
        invoice.Description = request.Description?.Trim();
        invoice.DueDate = dueDate;
        invoice.PaidAt = request.Status == PaymentStatus.Paid
            ? NormalizeUtc(request.PaidAt) ?? invoice.PaidAt ?? DateTime.UtcNow
            : null;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(invoice.Id, cancellationToken);
    }

    public async Task<InvoiceDetailDto?> UpdateStatusAsync(
        Guid id,
        UpdateInvoiceStatusRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidatePaymentStatus(request.Status);

        var invoice = await _dbContext.Invoices
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (invoice is null)
        {
            return null;
        }

        if (invoice.Status == PaymentStatus.Paid &&
            request.Status != PaymentStatus.Paid)
        {
            throw new InvalidOperationException(
                "A finalized invoice cannot be changed. Refund the approved payment instead.");
        }

        if (request.Status == PaymentStatus.Paid)
        {
            await EnsureInvoiceIsFullyPaidAsync(
                invoice.Id,
                invoice.FinalAmount,
                cancellationToken);
        }

        invoice.Status = request.Status;

        invoice.PaidAt = request.Status == PaymentStatus.Paid
            ? invoice.PaidAt ?? DateTime.UtcNow
            : null;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(invoice.Id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var invoice = await _dbContext.Invoices
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (invoice is null)
        {
            return false;
        }

        EnsureInvoiceIsEditable(invoice);

        invoice.IsDeletedInvoice = true;

        _dbContext.Invoices.Remove(invoice);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static void EnsureInvoiceIsEditable(Invoice invoice)
    {
        if (invoice.Status == PaymentStatus.Paid)
        {
            throw new InvalidOperationException(
                "A finalized invoice cannot be edited or deleted.");
        }
    }

    private async Task<Guid> ValidateAndGetProjectOwnerAsync(
        Guid userId,
        Guid projectId,
        CancellationToken cancellationToken)
    {
        if (projectId == Guid.Empty)
        {
            throw new InvalidOperationException("Project id is required.");
        }

        var project = await _dbContext.Projects
            .AsNoTracking()
            .Where(item => item.Id == projectId && item.User.IsActive)
            .Select(item => new { item.UserId })
            .FirstOrDefaultAsync(cancellationToken);

        if (project is null)
        {
            throw new InvalidOperationException("An active project and customer were not found.");
        }

        if (userId != Guid.Empty && userId != project.UserId)
        {
            throw new InvalidOperationException(
                "The selected project does not belong to the selected customer.");
        }

        return project.UserId;
    }

    private static void ValidateAmounts(
        decimal amount,
        decimal discountAmount,
        decimal taxAmount)
    {
        if (amount <= 0)
        {
            throw new InvalidOperationException("Invoice amount must be greater than zero.");
        }

        if (discountAmount < 0)
        {
            throw new InvalidOperationException("Discount amount cannot be negative.");
        }

        if (taxAmount < 0)
        {
            throw new InvalidOperationException("Tax amount cannot be negative.");
        }

        if (discountAmount > amount)
        {
            throw new InvalidOperationException("Discount amount cannot be greater than invoice amount.");
        }

        var finalAmount = CalculateFinalAmount(
            amount,
            discountAmount,
            taxAmount
        );

        if (finalAmount < 0)
        {
            throw new InvalidOperationException("Final amount cannot be negative.");
        }
    }

    private static decimal CalculateFinalAmount(
        decimal amount,
        decimal discountAmount,
        decimal taxAmount)
    {
        return amount - discountAmount + taxAmount;
    }

    private async Task EnsureInvoiceIsFullyPaidAsync(
        Guid invoiceId,
        decimal finalAmount,
        CancellationToken cancellationToken)
    {
        var approvedAmount = await _dbContext.Payments
            .Where(item =>
                item.InvoiceId == invoiceId &&
                item.Status == PaymentStatus.Paid)
            .SumAsync(item => item.Amount, cancellationToken);

        if (approvedAmount < finalAmount)
        {
            throw new InvalidOperationException(
                "Approve the customer's payment before finalizing the invoice.");
        }
    }

    private static void ValidatePaymentStatus(PaymentStatus status)
    {
        if (!Enum.IsDefined(typeof(PaymentStatus), status))
        {
            throw new InvalidOperationException("Invoice status is invalid.");
        }
    }

    private static DateTime NormalizeRequiredUtc(DateTime value, string fieldName)
    {
        if (value == default)
        {
            throw new InvalidOperationException($"{fieldName} is required.");
        }

        return NormalizeUtc(value)!.Value;
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

    private async Task<string> GenerateInvoiceNumberAsync(
        CancellationToken cancellationToken)
    {
        var prefix = $"INV-{DateTime.UtcNow:yyyyMMdd}";

        while (true)
        {
            var suffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
            var invoiceNumber = $"{prefix}-{suffix}";
            var exists = await _dbContext.Invoices
                .IgnoreQueryFilters()
                .AnyAsync(
                    item => item.InvoiceNumber == invoiceNumber,
                    cancellationToken);

            if (!exists)
            {
                return invoiceNumber;
            }
        }
    }
}
