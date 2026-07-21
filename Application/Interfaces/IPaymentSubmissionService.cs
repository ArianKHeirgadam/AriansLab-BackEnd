using Application.DTOs.Payments;

namespace Application.Interfaces;

public interface IPaymentSubmissionService
{
    Task<PaymentDetailDto?> SubmitAsync(
        Guid userId,
        SubmitPaymentRequestDto request,
        CancellationToken cancellationToken = default);
}
