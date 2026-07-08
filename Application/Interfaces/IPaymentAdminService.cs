using Application.DTOs.Payments;

namespace Application.Interfaces;

public interface IPaymentAdminService
{
    Task<List<PaymentDetailDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<PaymentDetailDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<PaymentDetailDto> CreateAsync(
        CreatePaymentRequestDto request,
        CancellationToken cancellationToken = default);

    Task<PaymentDetailDto?> UpdateAsync(
        Guid id,
        UpdatePaymentRequestDto request,
        CancellationToken cancellationToken = default);

    Task<PaymentDetailDto?> UpdateStatusAsync(
        Guid id,
        UpdatePaymentStatusRequestDto request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}