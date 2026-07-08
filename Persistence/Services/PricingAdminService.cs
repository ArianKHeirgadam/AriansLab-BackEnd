using Application.DTOs.Pricing;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class PricingAdminService : IPricingAdminService
{
    private readonly ApplicationDbContext _dbContext;

    public PricingAdminService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<AdminPricingPlanDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.PricingPlans
            .AsNoTracking()
            .Include(x => x.Features)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Price)
            .Select(x => new AdminPricingPlanDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                Price = x.Price,
                Duration = x.Duration,
                DeliveryDays = x.DeliveryDays,
                IsPopular = x.IsPopular,
                DisplayOrder = x.DisplayOrder,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
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

    public async Task<AdminPricingPlanDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.PricingPlans
            .AsNoTracking()
            .Include(x => x.Features)
            .Where(x => x.Id == id)
            .Select(x => new AdminPricingPlanDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                Price = x.Price,
                Duration = x.Duration,
                DeliveryDays = x.DeliveryDays,
                IsPopular = x.IsPopular,
                DisplayOrder = x.DisplayOrder,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
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

    public async Task<AdminPricingPlanDto> CreateAsync(
        CreatePricingPlanRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var pricingPlan = new PricingPlan
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Price = request.Price,
            Duration = request.Duration,
            DeliveryDays = request.DeliveryDays,
            IsPopular = request.IsPopular,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive,
            Features = request.Features
                .Where(x => !string.IsNullOrWhiteSpace(x.Feature))
                .Select(x => new PlanFeature
                {
                    Feature = x.Feature.Trim()
                })
                .ToList()
        };

        await _dbContext.PricingPlans.AddAsync(
            pricingPlan,
            cancellationToken
        );

        await _dbContext.SaveChangesAsync(cancellationToken);

        var createdPlan = await GetByIdAsync(
            pricingPlan.Id,
            cancellationToken
        );

        return createdPlan!;
    }

    public async Task<AdminPricingPlanDto?> UpdateAsync(
        Guid id,
        UpdatePricingPlanRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var pricingPlan = await _dbContext.PricingPlans
            .Include(x => x.Features)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (pricingPlan is null)
        {
            return null;
        }

        pricingPlan.Title = request.Title.Trim();
        pricingPlan.Description = request.Description.Trim();
        pricingPlan.Price = request.Price;
        pricingPlan.Duration = request.Duration;
        pricingPlan.DeliveryDays = request.DeliveryDays;
        pricingPlan.IsPopular = request.IsPopular;
        pricingPlan.DisplayOrder = request.DisplayOrder;
        pricingPlan.IsActive = request.IsActive;

        if (pricingPlan.Features.Any())
        {
            _dbContext.PlanFeatures.RemoveRange(pricingPlan.Features);
        }

        pricingPlan.Features = request.Features
            .Where(x => !string.IsNullOrWhiteSpace(x.Feature))
            .Select(x => new PlanFeature
            {
                PricingPlanId = pricingPlan.Id,
                Feature = x.Feature.Trim()
            })
            .ToList();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(
            pricingPlan.Id,
            cancellationToken
        );
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var pricingPlan = await _dbContext.PricingPlans
            .Include(x => x.Features)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (pricingPlan is null)
        {
            return false;
        }

        if (pricingPlan.Features.Any())
        {
            _dbContext.PlanFeatures.RemoveRange(pricingPlan.Features);
        }

        _dbContext.PricingPlans.Remove(pricingPlan);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}