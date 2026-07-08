using Application.DTOs.FAQs;

namespace Application.Interfaces;

public interface IFaqReadService
{
    Task<List<FaqDto>> GetActiveFaqsAsync(
        CancellationToken cancellationToken = default);
}