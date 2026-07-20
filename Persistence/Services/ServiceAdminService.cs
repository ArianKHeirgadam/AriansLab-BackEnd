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
        ValidateServiceFields(
            request.Title,
            request.Thumbnail,
            request.CoverImage,
            request.ShortDescription,
            request.Description,
            request.Icon,
            request.EstimatedDeliveryDays
        );

        var requestedFeatures =
            request.Features ??
            new List<CreateServiceFeatureRequestDto>();

        ValidateFeatureTitles(
            requestedFeatures.Select(x => x?.Title)
        );

        var slug = NormalizeSlug(
            request.Slug,
            request.Title
        );

        ValidateSlug(slug);

        var slugExists = await _dbContext.Services
            .IgnoreQueryFilters()
            .AnyAsync(
                x => x.Slug == slug,
                cancellationToken
            );

        if (slugExists)
        {
            throw new InvalidOperationException(
                "A service with this slug already exists."
            );
        }

        var service = new Service
        {
            Title = request.Title.Trim(),
            Slug = slug,
            Thumbnail = request.Thumbnail.Trim(),
            CoverImage = request.CoverImage.Trim(),
            ShortDescription = NormalizeOptionalText(
                request.ShortDescription
            ),
            Description = request.Description.Trim(),
            EstimatedDeliveryDays = request.EstimatedDeliveryDays,
            IsFeatured = request.IsFeatured,
            DisplayOrder = request.DisplayOrder,
            Icon = NormalizeOptionalText(request.Icon),
            IsActive = request.IsActive,

            Features = requestedFeatures
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new ServiceFeature
                {
                    Title = x!.Title.Trim(),
                    DisplayOrder = x.DisplayOrder
                })
                .ToList()
        };

        await _dbContext.Services.AddAsync(
            service,
            cancellationToken
        );

        await _dbContext.SaveChangesAsync(
            cancellationToken
        );

        var createdService = await GetByIdAsync(
            service.Id,
            cancellationToken
        );

        return createdService
            ?? throw new InvalidOperationException(
                "Service was created but could not be retrieved."
            );
    }

    public async Task<AdminServiceDto?> UpdateAsync(
        Guid id,
        UpdateServiceRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateServiceFields(
            request.Title,
            request.Thumbnail,
            request.CoverImage,
            request.ShortDescription,
            request.Description,
            request.Icon,
            request.EstimatedDeliveryDays
        );

        var requestedFeatures =
            request.Features ??
            new List<UpdateServiceFeatureRequestDto>();

        ValidateFeatureTitles(
            requestedFeatures.Select(x => x?.Title)
        );

        var service = await _dbContext.Services
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken
            );

        if (service is null)
        {
            return null;
        }

        var slug = NormalizeSlug(
            request.Slug,
            request.Title
        );

        ValidateSlug(slug);

        var slugExists = await _dbContext.Services
            .IgnoreQueryFilters()
            .AnyAsync(
                x => x.Id != id && x.Slug == slug,
                cancellationToken
            );

        if (slugExists)
        {
            throw new InvalidOperationException(
                "A service with this slug already exists."
            );
        }

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                cancellationToken
            );

        try
        {
            service.Title = request.Title.Trim();
            service.Slug = slug;
            service.Thumbnail = request.Thumbnail.Trim();
            service.CoverImage = request.CoverImage.Trim();

            service.ShortDescription = NormalizeOptionalText(
                request.ShortDescription
            );

            service.Description = request.Description.Trim();
            service.EstimatedDeliveryDays =
                request.EstimatedDeliveryDays;

            service.IsFeatured = request.IsFeatured;
            service.DisplayOrder = request.DisplayOrder;
            service.Icon = NormalizeOptionalText(request.Icon);
            service.IsActive = request.IsActive;

            var existingFeatures = await _dbContext.ServiceFeatures
                .Where(x => x.ServiceId == service.Id)
                .ToListAsync(cancellationToken);

            var now = DateTime.UtcNow;

            foreach (var feature in existingFeatures)
            {
                feature.IsDeleted = true;
                feature.DeletedAt = now;
                feature.UpdatedAt = now;
            }

            var newFeatures = requestedFeatures
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new ServiceFeature
                {
                    ServiceId = service.Id,
                    Title = x!.Title.Trim(),
                    DisplayOrder = x.DisplayOrder
                })
                .ToList();

            if (newFeatures.Count > 0)
            {
                await _dbContext.ServiceFeatures.AddRangeAsync(
                    newFeatures,
                    cancellationToken
                );
            }

            await _dbContext.SaveChangesAsync(
                cancellationToken
            );

            await transaction.CommitAsync(
                cancellationToken
            );
        }
        catch
        {
            await transaction.RollbackAsync(
                cancellationToken
            );

            throw;
        }

        return await GetByIdAsync(
            service.Id,
            cancellationToken
        );
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var service = await _dbContext.Services
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken
            );

        if (service is null)
        {
            return false;
        }

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                cancellationToken
            );

        try
        {
            var now = DateTime.UtcNow;

            var existingFeatures = await _dbContext.ServiceFeatures
                .Where(x => x.ServiceId == service.Id)
                .ToListAsync(cancellationToken);

            foreach (var feature in existingFeatures)
            {
                feature.IsDeleted = true;
                feature.DeletedAt = now;
                feature.UpdatedAt = now;
            }

            service.IsDeleted = true;
            service.DeletedAt = now;
            service.UpdatedAt = now;

            await _dbContext.SaveChangesAsync(
                cancellationToken
            );

            await transaction.CommitAsync(
                cancellationToken
            );

            return true;
        }
        catch
        {
            await transaction.RollbackAsync(
                cancellationToken
            );

            throw;
        }
    }

    private static void ValidateServiceFields(
        string? title,
        string? thumbnail,
        string? coverImage,
        string? shortDescription,
        string? description,
        string? icon,
        int estimatedDeliveryDays)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new InvalidOperationException(
                "Service title is required."
            );
        }

        if (title.Trim().Length > 200)
        {
            throw new InvalidOperationException(
                "Service title cannot be longer than 200 characters."
            );
        }

        if (string.IsNullOrWhiteSpace(thumbnail))
        {
            throw new InvalidOperationException(
                "Service thumbnail is required."
            );
        }

        if (thumbnail.Trim().Length > 1000)
        {
            throw new InvalidOperationException(
                "Service thumbnail cannot be longer than 1000 characters."
            );
        }

        if (string.IsNullOrWhiteSpace(coverImage))
        {
            throw new InvalidOperationException(
                "Service cover image is required."
            );
        }

        if (coverImage.Trim().Length > 1000)
        {
            throw new InvalidOperationException(
                "Service cover image cannot be longer than 1000 characters."
            );
        }

        if (!string.IsNullOrWhiteSpace(shortDescription) &&
            shortDescription.Trim().Length > 500)
        {
            throw new InvalidOperationException(
                "Short description cannot be longer than 500 characters."
            );
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new InvalidOperationException(
                "Service description is required."
            );
        }

        if (!string.IsNullOrWhiteSpace(icon) &&
            icon.Trim().Length > 500)
        {
            throw new InvalidOperationException(
                "Service icon cannot be longer than 500 characters."
            );
        }

        if (estimatedDeliveryDays < 0)
        {
            throw new InvalidOperationException(
                "Estimated delivery days cannot be negative."
            );
        }
    }

    private static void ValidateFeatureTitles(
        IEnumerable<string?> featureTitles)
    {
        foreach (var title in featureTitles)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new InvalidOperationException(
                    "Service feature title is required."
                );
            }

            if (title.Trim().Length > 250)
            {
                throw new InvalidOperationException(
                    "Service feature title cannot be longer than 250 characters."
                );
            }
        }
    }

    private static void ValidateSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new InvalidOperationException(
                "Service slug is required."
            );
        }

        if (slug.Length > 220)
        {
            throw new InvalidOperationException(
                "Service slug cannot be longer than 220 characters."
            );
        }
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static string NormalizeSlug(
        string? slug,
        string title)
    {
        var value = string.IsNullOrWhiteSpace(slug)
            ? title
            : slug;

        var parts = value
            .Trim()
            .ToLowerInvariant()
            .Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries
            );

        return string.Join("-", parts);
    }
}