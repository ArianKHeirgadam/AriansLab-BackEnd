using Application.DTOs.Invoices;

namespace Application.Interfaces;

public interface IInvoiceAdminService
{
    Task<List<InvoiceDetailDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<InvoiceDetailDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<InvoiceDetailDto> CreateAsync(
        CreateInvoiceRequestDto request,
        CancellationToken cancellationToken = default);

    Task<InvoiceDetailDto?> UpdateAsync(
        Guid id,
        UpdateInvoiceRequestDto request,
        CancellationToken cancellationToken = default);

    Task<InvoiceDetailDto?> UpdateStatusAsync(
        Guid id,
        UpdateInvoiceStatusRequestDto request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}