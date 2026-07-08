using Application.DTOs.Payments;

namespace Application.Interfaces;

public interface IPaymentReadService
{
    Task<List<PaymentListItemDto>> GetMyPaymentsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<PaymentDetailDto?> GetMyPaymentByIdAsync(
        Guid userId,
        Guid paymentId,
        CancellationToken cancellationToken = default);
}