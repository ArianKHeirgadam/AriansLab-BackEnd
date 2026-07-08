using Application.DTOs.Blog;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class BlogCategoryReadService : IBlogCategoryReadService
{
    private readonly ApplicationDbContext _dbContext;

    public BlogCategoryReadService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<BlogCategoryDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.BlogCategories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new BlogCategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<BlogCategoryDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.BlogCategories
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new BlogCategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<BlogCategoryDto?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var normalizedSlug = NormalizeSlug(slug);

        return await _dbContext.BlogCategories
            .AsNoTracking()
            .Where(x => x.Slug.ToLower() == normalizedSlug)
            .Select(x => new BlogCategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static string NormalizeSlug(string slug)
    {
        return slug.Trim().ToLowerInvariant();
    }
}