using Application.Common.Exceptions;
using Application.DTOs.Blog;
using Application.DTOs.Blog.Admin;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class BlogAdminCategoryService : IBlogAdminCategoryService
{
    private readonly ApplicationDbContext _dbContext;

    public BlogAdminCategoryService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BlogCategoryDto> CreateAsync(
        CreateBlogCategoryRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();
        var slug = NormalizeSlug(request.Slug);

        ValidateCategory(name, slug);

        var slugExists = await _dbContext.BlogCategories
            .AnyAsync(x => x.Slug == slug, cancellationToken);

        if (slugExists)
        {
            throw new ApiException("Blog category slug already exists.", 409);
        }

        var category = new BlogCategory
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = slug,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _dbContext.BlogCategories.AddAsync(category, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new BlogCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            PublishedPostCount = 0
        };
    }

    public async Task<BlogCategoryDto> UpdateAsync(
        Guid id,
        UpdateBlogCategoryRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.BlogCategories
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (category is null)
        {
            throw new ApiException("Blog category was not found.", 404);
        }

        var name = request.Name.Trim();
        var slug = NormalizeSlug(request.Slug);

        ValidateCategory(name, slug);

        var slugExists = await _dbContext.BlogCategories
            .AnyAsync(x => x.Id != id && x.Slug == slug, cancellationToken);

        if (slugExists)
        {
            throw new ApiException("Blog category slug already exists.", 409);
        }

        category.Name = name;
        category.Slug = slug;
        category.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var publishedPostCount = await _dbContext.BlogPosts
            .CountAsync(x => x.CategoryId == category.Id && x.IsPublished, cancellationToken);

        return new BlogCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            PublishedPostCount = publishedPostCount
        };
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.BlogCategories
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (category is null)
        {
            throw new ApiException("Blog category was not found.", 404);
        }

        var hasPosts = await _dbContext.BlogPosts
            .AnyAsync(x => x.CategoryId == id, cancellationToken);

        if (hasPosts)
        {
            throw new ApiException("This category has blog posts and cannot be deleted.", 409);
        }

        category.IsDeleted = true;
        category.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static void ValidateCategory(string name, string slug)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ApiException("Category name is required.", 400);
        }

        if (name.Length > 150)
        {
            throw new ApiException("Category name cannot exceed 150 characters.", 400);
        }

        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new ApiException("Category slug is required.", 400);
        }

        if (slug.Length > 150)
        {
            throw new ApiException("Category slug cannot exceed 150 characters.", 400);
        }
    }

    private static string NormalizeSlug(string slug)
    {
        return slug.Trim().ToLowerInvariant();
    }
}