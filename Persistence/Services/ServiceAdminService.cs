using Application.DTOs.Services;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class ServiceAdminService : IServiceAdminService
{
    private readonly ApplicationDbContext _dbContext;

    public ServiceAdminService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<AdminServiceDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Services
            .AsNoTracking()
            .Include(x => x.Features)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Title)
            .Select(x => new AdminServiceDto
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
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                Features = x.Features
                    .OrderBy(f => f.DisplayOrder)
                    .Select(f => new AdminServiceFeatureDto
                    {
                        Id = f.Id,
                        Title = f.Title,
                        DisplayOrder = f.DisplayOrder
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<AdminServiceDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Services
            .AsNoTracking()
            .Include(x => x.Features)
            .Where(x => x.Id == id)
            .Select(x => new AdminServiceDto
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
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                Features = x.Features
                    .OrderBy(f => f.DisplayOrder)
                    .Select(f => new AdminServiceFeatureDto
                    {
                        Id = f.Id,
                        Title = f.Title,
                        DisplayOrder = f.DisplayOrder
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<AdminServiceDto> CreateAsync(
        CreateServiceRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var slug = NormalizeSlug(request.Slug, request.Title);

        var slugExists = await _dbContext.Services
            .AnyAsync(x => x.Slug == slug, cancellationToken);

        if (slugExists)
        {
            throw new InvalidOperationException("A service with this slug already exists.");
        }

        var service = new Service
        {
            Title = request.Title.Trim(),
            Slug = slug,
            Thumbnail = request.Thumbnail.Trim(),
            CoverImage = request.CoverImage.Trim(),
            ShortDescription = request.ShortDescription?.Trim(),
            Description = request.Description.Trim(),
            EstimatedDeliveryDays = request.EstimatedDeliveryDays,
            IsFeatured = request.IsFeatured,
            DisplayOrder = request.DisplayOrder,
            Icon = request.Icon?.Trim(),
            IsActive = request.IsActive,
            Features = request.Features
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new ServiceFeature
                {
                    Title = x.Title.Trim(),
                    DisplayOrder = x.DisplayOrder
                })
                .ToList()
        };

        await _dbContext.Services.AddAsync(service, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var createdService = await GetByIdAsync(service.Id, cancellationToken);

        return createdService!;
    }

    public async Task<AdminServiceDto?> UpdateAsync(
        Guid id,
        UpdateServiceRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var service = await _dbContext.Services
            .Include(x => x.Features)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (service is null)
        {
            return null;
        }

        var slug = NormalizeSlug(request.Slug, request.Title);

        var slugExists = await _dbContext.Services
            .AnyAsync(
                x => x.Id != id && x.Slug == slug,
                cancellationToken
            );

        if (slugExists)
        {
            throw new InvalidOperationException("A service with this slug already exists.");
        }

        service.Title = request.Title.Trim();
        service.Slug = slug;
        service.Thumbnail = request.Thumbnail.Trim();
        service.CoverImage = request.CoverImage.Trim();
        service.ShortDescription = request.ShortDescription?.Trim();
        service.Description = request.Description.Trim();
        service.EstimatedDeliveryDays = request.EstimatedDeliveryDays;
        service.IsFeatured = request.IsFeatured;
        service.DisplayOrder = request.DisplayOrder;
        service.Icon = request.Icon?.Trim();
        service.IsActive = request.IsActive;

        if (service.Features.Any())
        {
            _dbContext.ServiceFeatures.RemoveRange(service.Features);
        }

        service.Features = request.Features
            .OrderBy(x => x.DisplayOrder)
            .Select(x => new ServiceFeature
            {
                ServiceId = service.Id,
                Title = x.Title.Trim(),
                DisplayOrder = x.DisplayOrder
            })
            .ToList();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(service.Id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var service = await _dbContext.Services
            .Include(x => x.Features)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (service is null)
        {
            return false;
        }

        if (service.Features.Any())
        {
            _dbContext.ServiceFeatures.RemoveRange(service.Features);
        }

        _dbContext.Services.Remove(service);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static string NormalizeSlug(string? slug, string title)
    {
        var value = string.IsNullOrWhiteSpace(slug)
            ? title
            : slug;

        return value
            .Trim()
            .ToLowerInvariant()
            .Replace(" ", "-");
    }
}