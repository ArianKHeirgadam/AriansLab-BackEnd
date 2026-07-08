using Application.DTOs.Pricing;

namespace Application.Interfaces;

public interface IPricingReadService
{
    Task<IReadOnlyList<PricingPlanDto>> GetActivePlansAsync(
        CancellationToken cancellationToken = default);

    Task<PricingPlanDto?> GetPlanByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}