using Application.DTOs.SocialMedias;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class SocialMediaReadService : ISocialMediaReadService
{
    private readonly ApplicationDbContext _dbContext;

    public SocialMediaReadService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<SocialMediaDto>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.SocialMedias
            .AsNoTracking()
            .Where(x => x.IsActive)
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
            .Where(x => x.Id == id && x.IsActive)
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
}