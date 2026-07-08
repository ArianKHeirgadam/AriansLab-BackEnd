using Application.DTOs.Invoices;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class InvoiceReadService : IInvoiceReadService
{
    private readonly ApplicationDbContext _dbContext;

    public InvoiceReadService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<InvoiceListItemDto>> GetMyInvoicesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Invoices
            .AsNoTracking()
            .Include(x => x.Project)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new InvoiceListItemDto
            {
                Id = x.Id,
                ProjectId = x.ProjectId,
                ProjectTitle = x.Project.Title,
                InvoiceNumber = x.InvoiceNumber,
                Amount = x.Amount,
                DiscountAmount = x.DiscountAmount,
                TaxAmount = x.TaxAmount,
                FinalAmount = x.FinalAmount,
                Status = x.Status,
                IsPaid = x.IsPaid,
                PaidAt = x.PaidAt,
                DueDate = x.DueDate,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<InvoiceDetailDto?> GetMyInvoiceByIdAsync(
        Guid userId,
        Guid invoiceId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Invoices
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Project)
            .Include(x => x.Payments)
            .Where(x => x.Id == invoiceId && x.UserId == userId)
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
}