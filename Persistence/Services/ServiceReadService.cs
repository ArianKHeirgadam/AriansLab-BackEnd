using Application.DTOs.Services;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class ServiceReadService : IServiceReadService
{
    private readonly ApplicationDbContext _dbContext;

    public ServiceReadService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ServiceListItemDto>> GetServicesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Services
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Title)
            .Select(x => new ServiceListItemDto
            {
                Id = x.Id,
                Title = x.Title,
                Slug = x.Slug,
                Thumbnail = x.Thumbnail,
                ShortDescription = x.ShortDescription,
                EstimatedDeliveryDays = x.EstimatedDeliveryDays,
                IsFeatured = x.IsFeatured,
                DisplayOrder = x.DisplayOrder,
                Icon = x.Icon
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ServiceListItemDto>> GetFeaturedServicesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Services
            .AsNoTracking()
            .Where(x => x.IsActive && x.IsFeatured)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Title)
            .Select(x => new ServiceListItemDto
            {
                Id = x.Id,
                Title = x.Title,
                Slug = x.Slug,
                Thumbnail = x.Thumbnail,
                ShortDescription = x.ShortDescription,
                EstimatedDeliveryDays = x.EstimatedDeliveryDays,
                IsFeatured = x.IsFeatured,
                DisplayOrder = x.DisplayOrder,
                Icon = x.Icon
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ServiceDetailDto?> GetServiceBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return null;
        }

        var normalizedSlug = slug.Trim().ToLowerInvariant();

        return await _dbContext.Services
            .AsNoTracking()
            .Include(x => x.Features)
            .Where(x => x.IsActive && x.Slug == normalizedSlug)
            .Select(x => new ServiceDetailDto
            {
                Id = x.Id,
                Title = x.Title,
                Slug = x.Slug,
                Thumbnail = x.Thumbnail,
                CoverImage = x.CoverImage,
                ShortDescription = x.ShortDescription,
                Description = x.Description,
                EstimatedDeliveryDays = x.EstimatedDeliveryDays,
                IsFeatured = x.IsFeatured,
                DisplayOrder = x.DisplayOrder,
                Icon = x.Icon,
                Features = x.Features
                    .OrderBy(f => f.DisplayOrder)
                    .Select(f => new ServiceFeatureDto
                    {
                        Id = f.Id,
                        Title = f.Title,
                        DisplayOrder = f.DisplayOrder
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}