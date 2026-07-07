using Application.DTOs.Common;
using Application.DTOs.Portfolio;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class PortfolioReadService : IPortfolioReadService
{
    private const int MaxPageSize = 50;

    private readonly ApplicationDbContext _dbContext;

    public PortfolioReadService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResultDto<PortfolioListItemDto>> GetPortfoliosAsync(
        PortfolioQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var pageNumber = parameters.PageNumber < 1 ? 1 : parameters.PageNumber;
        var pageSize = parameters.PageSize < 1 ? 10 : parameters.PageSize;

        if (pageSize > MaxPageSize)
        {
            pageSize = MaxPageSize;
        }

        var query = _dbContext.Portfolios
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Technologies)
                .ThenInclude(x => x.Technology)
            .AsQueryable();

        if (parameters.IsFeatured.HasValue)
        {
            query = query.Where(x => x.IsFeatured == parameters.IsFeatured.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.CategorySlug))
        {
            var categorySlug = parameters.CategorySlug.Trim().ToLowerInvariant();

            query = query.Where(x => x.Category.Slug == categorySlug);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim();
            var searchPattern = $"%{search}%";

            query = query.Where(x =>
                EF.Functions.Like(x.Title, searchPattern) ||
                EF.Functions.Like(x.Description, searchPattern) ||
                EF.Functions.Like(x.ClientName, searchPattern));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.DisplayOrder)
            .ThenByDescending(x => x.ProjectDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new PortfolioListItemDto
            {
                Id = x.Id,
                Title = x.Title,
                Slug = x.Slug,
                ShortDescription = x.ShortDescription,
                Thumbnail = x.Thumbnail,
                ClientName = x.ClientName,
                ProjectDate = x.ProjectDate,
                IsFeatured = x.IsFeatured,
                DisplayOrder = x.DisplayOrder,
                CategoryName = x.Category.Name,
                CategorySlug = x.Category.Slug,
                Technologies = x.Technologies
                    .Select(t => new PortfolioTechnologyDto
                    {
                        Id = t.Technology.Id,
                        Name = t.Technology.Name,
                        Icon = t.Technology.Icon,
                        Color = t.Technology.Color
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<PortfolioListItemDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalCount == 0
                ? 0
                : (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<PortfolioDetailDto?> GetPortfolioBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return null;
        }

        var normalizedSlug = slug.Trim().ToLowerInvariant();

        return await _dbContext.Portfolios
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Images)
            .Include(x => x.Technologies)
                .ThenInclude(x => x.Technology)
            .Where(x => x.Slug == normalizedSlug)
            .Select(x => new PortfolioDetailDto
            {
                Id = x.Id,
                Title = x.Title,
                Slug = x.Slug,
                Description = x.Description,
                ShortDescription = x.ShortDescription,
                ClientName = x.ClientName,
                ProjectDate = x.ProjectDate,
                Thumbnail = x.Thumbnail,
                WebsiteUrl = x.WebsiteUrl,
                GithubUrl = x.GithubUrl,
                IsFeatured = x.IsFeatured,
                DisplayOrder = x.DisplayOrder,
                CategoryId = x.PortfolioCategoryId,
                CategoryName = x.Category.Name,
                CategorySlug = x.Category.Slug,
                Images = x.Images
                    .OrderBy(i => i.DisplayOrder)
                    .Select(i => new PortfolioImageDto
                    {
                        Id = i.Id,
                        ImageUrl = i.ImageUrl,
                        IsCover = i.IsCover,
                        DisplayOrder = i.DisplayOrder
                    })
                    .ToList(),
                Technologies = x.Technologies
                    .Select(t => new PortfolioTechnologyDto
                    {
                        Id = t.Technology.Id,
                        Name = t.Technology.Name,
                        Icon = t.Technology.Icon,
                        Color = t.Technology.Color
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PortfolioCategoryDto>> GetCategoriesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.PortfolioCategories
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .Select(x => new PortfolioCategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                DisplayOrder = x.DisplayOrder,
                PortfolioCount = x.Portfolios.Count
            })
            .ToListAsync(cancellationToken);
    }
}