using Application.DTOs.Payments;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class PaymentReadService : IPaymentReadService
{
    private readonly ApplicationDbContext _dbContext;

    public PaymentReadService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<PaymentListItemDto>> GetMyPaymentsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Payments
            .AsNoTracking()
            .Where(x => x.Invoice.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new PaymentListItemDto
            {
                Id = x.Id,
                InvoiceId = x.InvoiceId,
                InvoiceNumber = x.Invoice.InvoiceNumber,
                Amount = x.Amount,
                Gateway = x.Gateway,
                Authority = x.Authority,
                RefId = x.RefId,
                Status = x.Status,
                TrackingCode = x.TrackingCode,
                PaidAt = x.PaidAt,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PaymentDetailDto?> GetMyPaymentByIdAsync(
        Guid userId,
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Payments
            .AsNoTracking()
            .Where(x => x.Id == paymentId && x.Invoice.UserId == userId)
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
                CardPan = x.CardPan == null
                    ? null
                    : x.CardPan.Length <= 4
                        ? x.CardPan
                        : "****-****-****-" + x.CardPan.Substring(x.CardPan.Length - 4),
                TrackingCode = x.TrackingCode,
                PaidAt = x.PaidAt,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
