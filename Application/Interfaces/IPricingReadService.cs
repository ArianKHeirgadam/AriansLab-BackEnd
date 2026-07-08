using Application.DTOs.Pricing;

namespace Application.Interfaces;

public interface IPricingReadService
{
    Task<List<PricingPlanDto>> GetActivePlansAsync(
        CancellationToken cancellationToken = default);

    Task<PricingPlanDto?> GetPlanByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}