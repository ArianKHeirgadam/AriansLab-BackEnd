using Application.DTOs.Technologies;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class TechnologyReadService : ITechnologyReadService
{
    private readonly ApplicationDbContext _dbContext;

    public TechnologyReadService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<TechnologyDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Technologies
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new TechnologyDto
            {
                Id = x.Id,
                Name = x.Name,
                Icon = x.Icon,
                Color = x.Color,
                PortfolioCount = _dbContext.PortfolioTechnologies.Count(pt =>
                    pt.TechnologyId == x.Id),
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<TechnologyDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Technologies
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new TechnologyDto
            {
                Id = x.Id,
                Name = x.Name,
                Icon = x.Icon,
                Color = x.Color,
                PortfolioCount = _dbContext.PortfolioTechnologies.Count(pt =>
                    pt.TechnologyId == x.Id),
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}