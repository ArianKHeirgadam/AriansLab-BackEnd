using Application.DTOs.HeroSections;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class HeroSectionReadService : IHeroSectionReadService
{
    private readonly ApplicationDbContext _dbContext;

    public HeroSectionReadService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<HeroSectionDto>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.HeroSections
            .AsNoTracking()
            .Where(x => x.IsActive)
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
            .Where(x => x.Id == id && x.IsActive)
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
}