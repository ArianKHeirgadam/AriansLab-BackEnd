using Application.Common.Exceptions;
using Application.DTOs.Blog.Admin;
using Application.DTOs.Common;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class BlogAdminPostService : IBlogAdminPostService
{
    private const int MaxPageSize = 50;

    private readonly ApplicationDbContext _dbContext;

    public BlogAdminPostService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResultDto<AdminBlogPostListItemDto>> GetPostsAsync(
        AdminBlogPostQueryParameters parameters,
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
            .AsQueryable();

        if (parameters.IsPublished.HasValue)
        {
            query = query.Where(x => x.IsPublished == parameters.IsPublished.Value);
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
                EF.Functions.Like(x.Excerpt, searchPattern) ||
                EF.Functions.Like(x.Content, searchPattern));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AdminBlogPostListItemDto
            {
                Id = x.Id,
                Title = x.Title,
                Slug = x.Slug,
                Excerpt = x.Excerpt,
                CoverImage = x.CoverImage,
                ReadTime = x.ReadTime,
                ViewCount = x.ViewCount,
                IsPublished = x.IsPublished,
                PublishedAt = x.PublishedAt,
                CategoryName = x.Category.Name,
                CategorySlug = x.Category.Slug,
                AuthorName = x.Author.FullName,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<AdminBlogPostListItemDto>
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

    public async Task<AdminBlogPostDetailDto> GetPostByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var post = await _dbContext.BlogPosts
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Author)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (post is null)
        {
            throw new ApiException("Blog post was not found.", 404);
        }

        return MapToDetailDto(post);
    }

    public async Task<AdminBlogPostDetailDto> CreateAsync(
        Guid authorId,
        CreateBlogPostRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var title = request.Title.Trim();
        var slug = NormalizeSlug(request.Slug);

        ValidatePost(
            title,
            slug,
            request.Excerpt,
            request.Content,
            request.CoverImage,
            request.ReadTime,
            request.CategoryId);

        var authorExists = await _dbContext.Users
            .AnyAsync(x => x.Id == authorId, cancellationToken);

        if (!authorExists)
        {
            throw new ApiException("Author was not found.", 404);
        }

        var categoryExists = await _dbContext.BlogCategories
            .AnyAsync(x => x.Id == request.CategoryId, cancellationToken);

        if (!categoryExists)
        {
            throw new ApiException("Blog category was not found.", 404);
        }

        var slugExists = await _dbContext.BlogPosts
            .AnyAsync(x => x.Slug == slug, cancellationToken);

        if (slugExists)
        {
            throw new ApiException("Blog post slug already exists.", 409);
        }

        var now = DateTime.UtcNow;

        var post = new BlogPost
        {
            Id = Guid.NewGuid(),
            Title = title,
            Slug = slug,
            Excerpt = request.Excerpt.Trim(),
            Content = request.Content.Trim(),
            CoverImage = request.CoverImage.Trim(),
            ReadTime = request.ReadTime,
            ViewCount = 0,
            IsPublished = request.IsPublished,
            PublishedAt = request.IsPublished
                ? request.PublishedAt ?? now
                : null,
            SeoTitle = string.IsNullOrWhiteSpace(request.SeoTitle)
                ? null
                : request.SeoTitle.Trim(),
            SeoDescription = string.IsNullOrWhiteSpace(request.SeoDescription)
                ? null
                : request.SeoDescription.Trim(),
            Keywords = string.IsNullOrWhiteSpace(request.Keywords)
                ? null
                : request.Keywords.Trim(),
            CategoryId = request.CategoryId,
            AuthorId = authorId,
            CreatedAt = now,
            IsDeleted = false
        };

        await _dbContext.BlogPosts.AddAsync(post, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetPostByIdAsync(post.Id, cancellationToken);
    }

    public async Task<AdminBlogPostDetailDto> UpdateAsync(
        Guid id,
        UpdateBlogPostRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var post = await _dbContext.BlogPosts
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (post is null)
        {
            throw new ApiException("Blog post was not found.", 404);
        }

        var title = request.Title.Trim();
        var slug = NormalizeSlug(request.Slug);

        ValidatePost(
            title,
            slug,
            request.Excerpt,
            request.Content,
            request.CoverImage,
            request.ReadTime,
            request.CategoryId);

        var categoryExists = await _dbContext.BlogCategories
            .AnyAsync(x => x.Id == request.CategoryId, cancellationToken);

        if (!categoryExists)
        {
            throw new ApiException("Blog category was not found.", 404);
        }

        var slugExists = await _dbContext.BlogPosts
            .AnyAsync(x => x.Id != id && x.Slug == slug, cancellationToken);

        if (slugExists)
        {
            throw new ApiException("Blog post slug already exists.", 409);
        }

        var now = DateTime.UtcNow;

        post.Title = title;
        post.Slug = slug;
        post.Excerpt = request.Excerpt.Trim();
        post.Content = request.Content.Trim();
        post.CoverImage = request.CoverImage.Trim();
        post.ReadTime = request.ReadTime;
        post.IsPublished = request.IsPublished;
        post.PublishedAt = request.IsPublished
            ? request.PublishedAt ?? post.PublishedAt ?? now
            : null;
        post.SeoTitle = string.IsNullOrWhiteSpace(request.SeoTitle)
            ? null
            : request.SeoTitle.Trim();
        post.SeoDescription = string.IsNullOrWhiteSpace(request.SeoDescription)
            ? null
            : request.SeoDescription.Trim();
        post.Keywords = string.IsNullOrWhiteSpace(request.Keywords)
            ? null
            : request.Keywords.Trim();
        post.CategoryId = request.CategoryId;
        post.UpdatedAt = now;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetPostByIdAsync(post.Id, cancellationToken);
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var post = await _dbContext.BlogPosts
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (post is null)
        {
            throw new ApiException("Blog post was not found.", 404);
        }

        post.IsDeleted = true;
        post.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static AdminBlogPostDetailDto MapToDetailDto(BlogPost post)
    {
        return new AdminBlogPostDetailDto
        {
            Id = post.Id,
            Title = post.Title,
            Slug = post.Slug,
            Excerpt = post.Excerpt,
            Content = post.Content,
            CoverImage = post.CoverImage,
            ReadTime = post.ReadTime,
            ViewCount = post.ViewCount,
            IsPublished = post.IsPublished,
            PublishedAt = post.PublishedAt,
            SeoTitle = post.SeoTitle,
            SeoDescription = post.SeoDescription,
            Keywords = post.Keywords,
            CategoryId = post.CategoryId,
            CategoryName = post.Category.Name,
            CategorySlug = post.Category.Slug,
            AuthorId = post.AuthorId,
            AuthorName = post.Author.FullName,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt
        };
    }

    private static void ValidatePost(
        string title,
        string slug,
        string excerpt,
        string content,
        string coverImage,
        int readTime,
        Guid categoryId)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ApiException("Blog post title is required.", 400);
        }

        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new ApiException("Blog post slug is required.", 400);
        }

        if (string.IsNullOrWhiteSpace(excerpt))
        {
            throw new ApiException("Blog post excerpt is required.", 400);
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ApiException("Blog post content is required.", 400);
        }

        if (string.IsNullOrWhiteSpace(coverImage))
        {
            throw new ApiException("Blog post cover image is required.", 400);
        }

        if (readTime < 1)
        {
            throw new ApiException("Read time must be greater than zero.", 400);
        }

        if (categoryId == Guid.Empty)
        {
            throw new ApiException("Blog category is required.", 400);
        }
    }

    private static string NormalizeSlug(string slug)
    {
        return slug.Trim().ToLowerInvariant();
    }
}