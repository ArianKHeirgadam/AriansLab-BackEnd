using Application.DTOs.SocialMedias;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class SocialMediaAdminService : ISocialMediaAdminService
{
    private readonly ApplicationDbContext _dbContext;

    public SocialMediaAdminService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<SocialMediaDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.SocialMedias
            .AsNoTracking()
            .OrderBy(x => x.DisplayOrder)
            .ThenByDescending(x => x.CreatedAt)
            .Select(x => new SocialMediaDto
            {
                Id = x.Id,
                Platform = x.Platform,
                Url = x.Url,
                Icon = x.Icon,
                DisplayOrder = x.DisplayOrder,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<SocialMediaDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.SocialMedias
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new SocialMediaDto
            {
                Id = x.Id,
                Platform = x.Platform,
                Url = x.Url,
                Icon = x.Icon,
                DisplayOrder = x.DisplayOrder,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<SocialMediaDto> CreateAsync(
        CreateSocialMediaRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateSocialMedia(
            request.Platform,
            request.Url,
            request.DisplayOrder
        );

        var socialMedia = new SocialMedia
        {
            Platform = request.Platform.Trim(),
            Url = request.Url.Trim(),
            Icon = NormalizeNullableString(request.Icon),
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive
        };

        await _dbContext.SocialMedias.AddAsync(
            socialMedia,
            cancellationToken
        );

        await _dbContext.SaveChangesAsync(cancellationToken);

        var createdSocialMedia = await GetByIdAsync(
            socialMedia.Id,
            cancellationToken
        );

        return createdSocialMedia!;
    }

    public async Task<SocialMediaDto?> UpdateAsync(
        Guid id,
        UpdateSocialMediaRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateSocialMedia(
            request.Platform,
            request.Url,
            request.DisplayOrder
        );

        var socialMedia = await _dbContext.SocialMedias
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (socialMedia is null)
        {
            return null;
        }

        socialMedia.Platform = request.Platform.Trim();
        socialMedia.Url = request.Url.Trim();
        socialMedia.Icon = NormalizeNullableString(request.Icon);
        socialMedia.DisplayOrder = request.DisplayOrder;
        socialMedia.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(
            socialMedia.Id,
            cancellationToken
        );
    }

    public async Task<SocialMediaDto?> UpdateActiveStatusAsync(
        Guid id,
        UpdateSocialMediaActiveStatusRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var socialMedia = await _dbContext.SocialMedias
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (socialMedia is null)
        {
            return null;
        }

        socialMedia.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(
            socialMedia.Id,
            cancellationToken
        );
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var socialMedia = await _dbContext.SocialMedias
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (socialMedia is null)
        {
            return false;
        }

        _dbContext.SocialMedias.Remove(socialMedia);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static void ValidateSocialMedia(
        string platform,
        string url,
        int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(platform))
        {
            throw new InvalidOperationException("Social media platform is required.");
        }

        if (string.IsNullOrWhiteSpace(url))
        {
            throw new InvalidOperationException("Social media url is required.");
        }

        if (displayOrder < 0)
        {
            throw new InvalidOperationException("Display order cannot be negative.");
        }
    }

    private static string? NormalizeNullableString(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}