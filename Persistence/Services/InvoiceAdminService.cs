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
        await ValidateUserAndProjectAsync(
            request.UserId,
            request.ProjectId,
            cancellationToken
        );

        ValidateAmounts(
            request.Amount,
            request.DiscountAmount,
            request.TaxAmount
        );

        var invoiceNumber = string.IsNullOrWhiteSpace(request.InvoiceNumber)
            ? await GenerateInvoiceNumberAsync(cancellationToken)
            : request.InvoiceNumber.Trim().ToUpperInvariant();

        var invoiceNumberExists = await _dbContext.Invoices
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
            UserId = request.UserId,
            ProjectId = request.ProjectId,
            InvoiceNumber = invoiceNumber,
            Amount = request.Amount,
            DiscountAmount = request.DiscountAmount,
            TaxAmount = request.TaxAmount,
            FinalAmount = finalAmount,
            Status = request.Status,
            Description = request.Description?.Trim(),
            DueDate = request.DueDate,
            PaidAt = request.Status == PaymentStatus.Paid
                ? DateTime.UtcNow
                : null,
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
        await ValidateUserAndProjectAsync(
            request.UserId,
            request.ProjectId,
            cancellationToken
        );

        ValidateAmounts(
            request.Amount,
            request.DiscountAmount,
            request.TaxAmount
        );

        var invoice = await _dbContext.Invoices
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (invoice is null)
        {
            return null;
        }

        invoice.UserId = request.UserId;
        invoice.ProjectId = request.ProjectId;
        invoice.Amount = request.Amount;
        invoice.DiscountAmount = request.DiscountAmount;
        invoice.TaxAmount = request.TaxAmount;
        invoice.FinalAmount = CalculateFinalAmount(
            request.Amount,
            request.DiscountAmount,
            request.TaxAmount
        );
        invoice.Status = request.Status;
        invoice.Description = request.Description?.Trim();
        invoice.DueDate = request.DueDate;
        invoice.PaidAt = request.Status == PaymentStatus.Paid
            ? request.PaidAt ?? DateTime.UtcNow
            : null;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(invoice.Id, cancellationToken);
    }

    public async Task<InvoiceDetailDto?> UpdateStatusAsync(
        Guid id,
        UpdateInvoiceStatusRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var invoice = await _dbContext.Invoices
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (invoice is null)
        {
            return null;
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

        invoice.IsDeletedInvoice = true;

        _dbContext.Invoices.Remove(invoice);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task ValidateUserAndProjectAsync(
        Guid userId,
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var userExists = await _dbContext.Users
            .AnyAsync(x => x.Id == userId && x.IsActive, cancellationToken);

        if (!userExists)
        {
            throw new InvalidOperationException("Active user was not found.");
        }

        var projectBelongsToUser = await _dbContext.Projects
            .AnyAsync(
                x => x.Id == projectId && x.UserId == userId,
                cancellationToken
            );

        if (!projectBelongsToUser)
        {
            throw new InvalidOperationException("Project was not found for this user.");
        }
    }

    private static void ValidateAmounts(
        decimal amount,
        decimal discountAmount,
        decimal taxAmount)
    {
        if (amount < 0)
        {
            throw new InvalidOperationException("Invoice amount cannot be negative.");
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

    private async Task<string> GenerateInvoiceNumberAsync(
        CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow;
        var prefix = $"INV-{today:yyyyMMdd}";

        var countToday = await _dbContext.Invoices
            .CountAsync(
                x => x.InvoiceNumber.StartsWith(prefix),
                cancellationToken
            );

        return $"{prefix}-{countToday + 1:000}";
    }
}