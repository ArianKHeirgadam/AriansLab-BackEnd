using Application.DTOs.Blog;
using Application.DTOs.Blog.Admin;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class BlogCategoryAdminService : IBlogCategoryAdminService
{
    private readonly ApplicationDbContext _dbContext;

    public BlogCategoryAdminService(ApplicationDbContext dbContext)
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

    public async Task<BlogCategoryDto> CreateAsync(
        CreateBlogCategoryRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateBlogCategory(request.Name, request.Slug);

        var normalizedSlug = NormalizeSlug(request.Slug);

        var slugExists = await _dbContext.BlogCategories
            .AnyAsync(
                x => x.Slug.ToLower() == normalizedSlug,
                cancellationToken
            );

        if (slugExists)
        {
            throw new InvalidOperationException("Blog category slug already exists.");
        }

        var category = new BlogCategory
        {
            Name = request.Name.Trim(),
            Slug = normalizedSlug
        };

        await _dbContext.BlogCategories.AddAsync(
            category,
            cancellationToken
        );

        await _dbContext.SaveChangesAsync(cancellationToken);

        var createdCategory = await GetByIdAsync(
            category.Id,
            cancellationToken
        );

        return createdCategory!;
    }

    public async Task<BlogCategoryDto?> UpdateAsync(
        Guid id,
        UpdateBlogCategoryRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateBlogCategory(request.Name, request.Slug);

        var category = await _dbContext.BlogCategories
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (category is null)
        {
            return null;
        }

        var normalizedSlug = NormalizeSlug(request.Slug);

        var duplicateSlugExists = await _dbContext.BlogCategories
            .AnyAsync(
                x => x.Id != id && x.Slug.ToLower() == normalizedSlug,
                cancellationToken
            );

        if (duplicateSlugExists)
        {
            throw new InvalidOperationException("Blog category slug already exists.");
        }

        category.Name = request.Name.Trim();
        category.Slug = normalizedSlug;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(category.Id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.BlogCategories
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (category is null)
        {
            return false;
        }

        var hasBlogPosts = await _dbContext.BlogPosts
            .AnyAsync(x => x.CategoryId == id, cancellationToken);

        if (hasBlogPosts)
        {
            throw new InvalidOperationException(
                "This category has blog posts and cannot be deleted."
            );
        }

        _dbContext.BlogCategories.Remove(category);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static void ValidateBlogCategory(
        string name,
        string slug)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Blog category name is required.");
        }

        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new InvalidOperationException("Blog category slug is required.");
        }
    }

    private static string NormalizeSlug(string slug)
    {
        return slug.Trim().ToLowerInvariant();
    }
}