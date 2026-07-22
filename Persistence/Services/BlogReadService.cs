using Application.DTOs.Blog;
using Application.DTOs.Common;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class BlogReadService : IBlogReadService
{
    private const int MaxPageSize = 50;

    private readonly ApplicationDbContext _dbContext;

    public BlogReadService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResultDto<BlogPostListItemDto>> GetPublishedPostsAsync(
        BlogPostQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var pageNumber = parameters.PageNumber < 1 ? 1 : parameters.PageNumber;
        var pageSize = parameters.PageSize < 1 ? 10 : parameters.PageSize;

        if (pageSize > MaxPageSize)
        {
            pageSize = MaxPageSize;
        }

        var query = _dbContext.BlogPosts
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Author)
            .Where(x => x.IsPublished);

        if (!string.IsNullOrWhiteSpace(parameters.CategorySlug))
        {
            var categorySlug = parameters.CategorySlug.Trim();

            query = query.Where(x => x.Category.Slug == categorySlug);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim();
            var searchPattern = $"%{search}%";

            query = query.Where(x =>
                EF.Functions.Like(x.Title, searchPattern) ||
                EF.Functions.Like(x.Excerpt, searchPattern) ||
                EF.Functions.Like(x.Content, searchPattern));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.PublishedAt ?? x.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new BlogPostListItemDto
            {
                Id = x.Id,
                Title = x.Title,
                Slug = x.Slug,
                Excerpt = x.Excerpt,
                CoverImage = x.CoverImage,
                ReadTime = x.ReadTime,
                ViewCount = x.ViewCount,
                PublishedAt = x.PublishedAt,
                CategoryName = x.Category.Name,
                CategorySlug = x.Category.Slug,
                AuthorName = x.Author.FullName,
                Keywords = x.Keywords
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<BlogPostListItemDto>
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

    public async Task<BlogPostDetailDto?> GetPublishedPostBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return null;
        }

        var normalizedSlug = slug.Trim();

        var publishedPost = _dbContext.BlogPosts
            .Where(x => x.IsPublished && x.Slug == normalizedSlug);

        if (_dbContext.Database.IsRelational())
        {
            var updatedRows = await publishedPost.ExecuteUpdateAsync(
                updates => updates.SetProperty(
                    post => post.ViewCount,
                    post => post.ViewCount + 1),
                cancellationToken);

            if (updatedRows == 0)
            {
                return null;
            }
        }
        else
        {
            // EF Core's in-memory provider does not support ExecuteUpdateAsync.
            // Keep the fallback for local/tests while production uses the atomic update above.
            var trackedPost = await publishedPost.FirstOrDefaultAsync(cancellationToken);

            if (trackedPost is null)
            {
                return null;
            }

            trackedPost.ViewCount++;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return await publishedPost
            .AsNoTracking()
            .Select(x => new BlogPostDetailDto
            {
                Id = x.Id,
                Title = x.Title,
                Slug = x.Slug,
                Excerpt = x.Excerpt,
                Content = x.Content,
                CoverImage = x.CoverImage,
                ReadTime = x.ReadTime,
                ViewCount = x.ViewCount,
                PublishedAt = x.PublishedAt,
                SeoTitle = x.SeoTitle,
                SeoDescription = x.SeoDescription,
                Keywords = x.Keywords,
                CategoryId = x.CategoryId,
                CategoryName = x.Category.Name,
                CategorySlug = x.Category.Slug,
                AuthorId = x.AuthorId,
                AuthorName = x.Author.FullName
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BlogCategoryDto>> GetCategoriesAsync(
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
                PublishedPostCount = x.BlogPosts.Count(p => p.IsPublished)
            })
            .ToListAsync(cancellationToken);
    }
}
