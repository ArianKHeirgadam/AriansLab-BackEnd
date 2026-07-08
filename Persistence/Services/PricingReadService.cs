using Application.DTOs.Pricing;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class PricingReadService : IPricingReadService
{
    private readonly ApplicationDbContext _dbContext;

    public PricingReadService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<PricingPlanDto>> GetActivePlansAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.PricingPlans
            .AsNoTracking()
            .Include(x => x.Features)
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Price)
            .Select(x => new PricingPlanDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                Price = x.Price,
                Duration = x.Duration,
                DeliveryDays = x.DeliveryDays,
                IsPopular = x.IsPopular,
                DisplayOrder = x.DisplayOrder,
                Features = x.Features
                    .Select(f => new PlanFeatureDto
                    {
                        Id = f.Id,
                        Feature = f.Feature
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PricingPlanDto?> GetPlanByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.PricingPlans
            .AsNoTracking()
            .Include(x => x.Features)
            .Where(x => x.Id == id && x.IsActive)
            .Select(x => new PricingPlanDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                Price = x.Price,
                Duration = x.Duration,
                DeliveryDays = x.DeliveryDays,
                IsPopular = x.IsPopular,
                DisplayOrder = x.DisplayOrder,
                Features = x.Features
                    .Select(f => new PlanFeatureDto
                    {
                        Id = f.Id,
                        Feature = f.Feature
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}