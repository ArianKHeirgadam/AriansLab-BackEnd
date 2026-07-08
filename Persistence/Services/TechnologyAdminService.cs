using Application.DTOs.Technologies;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class TechnologyAdminService : ITechnologyAdminService
{
    private readonly ApplicationDbContext _dbContext;

    public TechnologyAdminService(ApplicationDbContext dbContext)
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

    public async Task<TechnologyDto> CreateAsync(
        CreateTechnologyRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request.Name);

        var normalizedName = NormalizeName(request.Name);

        var nameExists = await _dbContext.Technologies
            .AnyAsync(
                x => x.Name.ToLower() == normalizedName,
                cancellationToken
            );

        if (nameExists)
        {
            throw new InvalidOperationException("Technology name already exists.");
        }

        var technology = new Technology
        {
            Name = request.Name.Trim(),
            Icon = NormalizeNullableString(request.Icon),
            Color = NormalizeNullableString(request.Color)
        };

        await _dbContext.Technologies.AddAsync(
            technology,
            cancellationToken
        );

        await _dbContext.SaveChangesAsync(cancellationToken);

        var createdTechnology = await GetByIdAsync(
            technology.Id,
            cancellationToken
        );

        return createdTechnology!;
    }

    public async Task<TechnologyDto?> UpdateAsync(
        Guid id,
        UpdateTechnologyRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request.Name);

        var technology = await _dbContext.Technologies
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (technology is null)
        {
            return null;
        }

        var normalizedName = NormalizeName(request.Name);

        var duplicateNameExists = await _dbContext.Technologies
            .AnyAsync(
                x => x.Id != id && x.Name.ToLower() == normalizedName,
                cancellationToken
            );

        if (duplicateNameExists)
        {
            throw new InvalidOperationException("Technology name already exists.");
        }

        technology.Name = request.Name.Trim();
        technology.Icon = NormalizeNullableString(request.Icon);
        technology.Color = NormalizeNullableString(request.Color);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(
            technology.Id,
            cancellationToken
        );
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var technology = await _dbContext.Technologies
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (technology is null)
        {
            throw new InvalidOperationException("Technology was not found.");
        }

        var isUsedByPortfolio = await _dbContext.PortfolioTechnologies
            .AnyAsync(x => x.TechnologyId == id, cancellationToken);

        if (isUsedByPortfolio)
        {
            throw new InvalidOperationException(
                "This technology is used by portfolios and cannot be deleted."
            );
        }

        _dbContext.Technologies.Remove(technology);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static void ValidateRequest(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Technology name is required.");
        }
    }

    private static string NormalizeName(string name)
    {
        return name.Trim().ToLowerInvariant();
    }

    private static string? NormalizeNullableString(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}