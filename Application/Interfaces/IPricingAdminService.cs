using Application.DTOs.Pricing;

namespace Application.Interfaces;

public interface IPricingAdminService
{
    Task<List<AdminPricingPlanDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<AdminPricingPlanDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<AdminPricingPlanDto> CreateAsync(
        CreatePricingPlanRequestDto request,
        CancellationToken cancellationToken = default);

    Task<AdminPricingPlanDto?> UpdateAsync(
        Guid id,
        UpdatePricingPlanRequestDto request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}