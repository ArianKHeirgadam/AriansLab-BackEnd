using Application.DTOs.Invoices;

namespace Application.Interfaces;

public interface IInvoiceReadService
{
    Task<List<InvoiceListItemDto>> GetMyInvoicesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<InvoiceDetailDto?> GetMyInvoiceByIdAsync(
        Guid userId,
        Guid invoiceId,
        CancellationToken cancellationToken = default);
}