using Application.DTOs.HeroSections;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class HeroSectionAdminService : IHeroSectionAdminService
{
    private readonly ApplicationDbContext _dbContext;

    public HeroSectionAdminService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<HeroSectionDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.HeroSections
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new HeroSectionDto
            {
                Id = x.Id,
                Title = x.Title,
                Subtitle = x.Subtitle,
                Description = x.Description,
                PrimaryButtonText = x.PrimaryButtonText,
                PrimaryButtonUrl = x.PrimaryButtonUrl,
                SecondaryButtonText = x.SecondaryButtonText,
                SecondaryButtonUrl = x.SecondaryButtonUrl,
                BackgroundImage = x.BackgroundImage,
                VideoUrl = x.VideoUrl,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<HeroSectionDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.HeroSections
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new HeroSectionDto
            {
                Id = x.Id,
                Title = x.Title,
                Subtitle = x.Subtitle,
                Description = x.Description,
                PrimaryButtonText = x.PrimaryButtonText,
                PrimaryButtonUrl = x.PrimaryButtonUrl,
                SecondaryButtonText = x.SecondaryButtonText,
                SecondaryButtonUrl = x.SecondaryButtonUrl,
                BackgroundImage = x.BackgroundImage,
                VideoUrl = x.VideoUrl,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<HeroSectionDto> CreateAsync(
        CreateHeroSectionRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateHeroSection(
            request.Title,
            request.Subtitle,
            request.Description,
            request.PrimaryButtonText,
            request.PrimaryButtonUrl
        );

        var heroSection = new HeroSection
        {
            Title = request.Title.Trim(),
            Subtitle = request.Subtitle.Trim(),
            Description = request.Description.Trim(),
            PrimaryButtonText = request.PrimaryButtonText.Trim(),
            PrimaryButtonUrl = request.PrimaryButtonUrl.Trim(),
            SecondaryButtonText = NormalizeNullableString(request.SecondaryButtonText),
            SecondaryButtonUrl = NormalizeNullableString(request.SecondaryButtonUrl),
            BackgroundImage = NormalizeNullableString(request.BackgroundImage),
            VideoUrl = NormalizeNullableString(request.VideoUrl),
            IsActive = request.IsActive
        };

        await _dbContext.HeroSections.AddAsync(heroSection, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var createdHeroSection = await GetByIdAsync(
            heroSection.Id,
            cancellationToken
        );

        return createdHeroSection!;
    }

    public async Task<HeroSectionDto?> UpdateAsync(
        Guid id,
        UpdateHeroSectionRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateHeroSection(
            request.Title,
            request.Subtitle,
            request.Description,
            request.PrimaryButtonText,
            request.PrimaryButtonUrl
        );

        var heroSection = await _dbContext.HeroSections
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (heroSection is null)
        {
            return null;
        }

        heroSection.Title = request.Title.Trim();
        heroSection.Subtitle = request.Subtitle.Trim();
        heroSection.Description = request.Description.Trim();
        heroSection.PrimaryButtonText = request.PrimaryButtonText.Trim();
        heroSection.PrimaryButtonUrl = request.PrimaryButtonUrl.Trim();
        heroSection.SecondaryButtonText = NormalizeNullableString(request.SecondaryButtonText);
        heroSection.SecondaryButtonUrl = NormalizeNullableString(request.SecondaryButtonUrl);
        heroSection.BackgroundImage = NormalizeNullableString(request.BackgroundImage);
        heroSection.VideoUrl = NormalizeNullableString(request.VideoUrl);
        heroSection.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(heroSection.Id, cancellationToken);
    }

    public async Task<HeroSectionDto?> UpdateActiveStatusAsync(
        Guid id,
        UpdateHeroSectionActiveStatusRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var heroSection = await _dbContext.HeroSections
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (heroSection is null)
        {
            return null;
        }

        heroSection.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(heroSection.Id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var heroSection = await _dbContext.HeroSections
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (heroSection is null)
        {
            return false;
        }

        _dbContext.HeroSections.Remove(heroSection);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static void ValidateHeroSection(
        string title,
        string subtitle,
        string description,
        string primaryButtonText,
        string primaryButtonUrl)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new InvalidOperationException("Hero section title is required.");
        }

        if (string.IsNullOrWhiteSpace(subtitle))
        {
            throw new InvalidOperationException("Hero section subtitle is required.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new InvalidOperationException("Hero section description is required.");
        }

        if (string.IsNullOrWhiteSpace(primaryButtonText))
        {
            throw new InvalidOperationException("Primary button text is required.");
        }

        if (string.IsNullOrWhiteSpace(primaryButtonUrl))
        {
            throw new InvalidOperationException("Primary button url is required.");
        }
    }

    private static string? NormalizeNullableString(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}