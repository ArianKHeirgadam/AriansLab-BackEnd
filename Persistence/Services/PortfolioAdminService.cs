using Application.DTOs.Portfolio;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class PortfolioAdminService : IPortfolioAdminService
{
    private readonly ApplicationDbContext _dbContext;

    public PortfolioAdminService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<AdminPortfolioDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await SelectAdminPortfolio(_dbContext.Portfolios.AsNoTracking())
            .OrderBy(x => x.DisplayOrder)
            .ThenByDescending(x => x.ProjectDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<AdminPortfolioDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await SelectAdminPortfolio(
                _dbContext.Portfolios
                    .AsNoTracking()
                    .Where(x => x.Id == id))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<AdminPortfolioDto> CreateAsync(
        CreatePortfolioRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(
            request.Title,
            request.Slug,
            request.Description,
            request.ShortDescription,
            request.ClientName,
            request.ProjectDate,
            request.Thumbnail,
            request.WebsiteUrl,
            request.GithubUrl,
            request.DisplayOrder,
            request.CategoryId);

        var slug = NormalizeSlug(request.Slug);
        var projectDate = NormalizeUtc(request.ProjectDate);

        await EnsureCategoryExistsAsync(request.CategoryId, cancellationToken);
        await EnsureSlugIsAvailableAsync(slug, null, cancellationToken);

        var portfolio = new Portfolio
        {
            Title = request.Title.Trim(),
            Slug = slug,
            Description = request.Description.Trim(),
            ShortDescription = NormalizeOptionalText(request.ShortDescription),
            ClientName = request.ClientName.Trim(),
            ProjectDate = projectDate,
            Thumbnail = request.Thumbnail.Trim(),
            WebsiteUrl = request.WebsiteUrl.Trim(),
            GithubUrl = NormalizeOptionalText(request.GithubUrl),
            IsFeatured = request.IsFeatured,
            DisplayOrder = request.DisplayOrder,
            PortfolioCategoryId = request.CategoryId
        };

        await _dbContext.Portfolios.AddAsync(portfolio, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(portfolio.Id, cancellationToken)
            ?? throw new InvalidOperationException(
                "Portfolio item was created but could not be retrieved.");
    }

    public async Task<AdminPortfolioDto?> UpdateAsync(
        Guid id,
        UpdatePortfolioRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(
            request.Title,
            request.Slug,
            request.Description,
            request.ShortDescription,
            request.ClientName,
            request.ProjectDate,
            request.Thumbnail,
            request.WebsiteUrl,
            request.GithubUrl,
            request.DisplayOrder,
            request.CategoryId);

        var portfolio = await _dbContext.Portfolios
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (portfolio is null)
        {
            return null;
        }

        var slug = NormalizeSlug(request.Slug);
        var projectDate = NormalizeUtc(request.ProjectDate);

        await EnsureCategoryExistsAsync(request.CategoryId, cancellationToken);
        await EnsureSlugIsAvailableAsync(slug, id, cancellationToken);

        portfolio.Title = request.Title.Trim();
        portfolio.Slug = slug;
        portfolio.Description = request.Description.Trim();
        portfolio.ShortDescription = NormalizeOptionalText(request.ShortDescription);
        portfolio.ClientName = request.ClientName.Trim();
        portfolio.ProjectDate = projectDate;
        portfolio.Thumbnail = request.Thumbnail.Trim();
        portfolio.WebsiteUrl = request.WebsiteUrl.Trim();
        portfolio.GithubUrl = NormalizeOptionalText(request.GithubUrl);
        portfolio.IsFeatured = request.IsFeatured;
        portfolio.DisplayOrder = request.DisplayOrder;
        portfolio.PortfolioCategoryId = request.CategoryId;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(portfolio.Id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var portfolio = await _dbContext.Portfolios
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (portfolio is null)
        {
            return false;
        }

        var images = await _dbContext.PortfolioImages
            .Where(x => x.PortfolioId == id)
            .ToListAsync(cancellationToken);

        var technologies = await _dbContext.PortfolioTechnologies
            .Where(x => x.PortfolioId == id)
            .ToListAsync(cancellationToken);

        _dbContext.PortfolioImages.RemoveRange(images);
        _dbContext.PortfolioTechnologies.RemoveRange(technologies);
        _dbContext.Portfolios.Remove(portfolio);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task EnsureCategoryExistsAsync(
        Guid categoryId,
        CancellationToken cancellationToken)
    {
        var categoryExists = await _dbContext.PortfolioCategories
            .AnyAsync(x => x.Id == categoryId, cancellationToken);

        if (!categoryExists)
        {
            throw new InvalidOperationException(
                "Portfolio category was not found.");
        }
    }

    private async Task EnsureSlugIsAvailableAsync(
        string slug,
        Guid? currentPortfolioId,
        CancellationToken cancellationToken)
    {
        var slugExists = await _dbContext.Portfolios
            .IgnoreQueryFilters()
            .AnyAsync(
                x => x.Slug == slug &&
                     (!currentPortfolioId.HasValue || x.Id != currentPortfolioId.Value),
                cancellationToken);

        if (slugExists)
        {
            throw new InvalidOperationException(
                "A portfolio item with this slug already exists.");
        }
    }

    private static IQueryable<AdminPortfolioDto> SelectAdminPortfolio(
        IQueryable<Portfolio> query)
    {
        return query.Select(x => new AdminPortfolioDto
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
                .OrderBy(image => image.DisplayOrder)
                .Select(image => new PortfolioImageDto
                {
                    Id = image.Id,
                    ImageUrl = image.ImageUrl,
                    IsCover = image.IsCover,
                    DisplayOrder = image.DisplayOrder
                })
                .ToList(),
            Technologies = x.Technologies
                .OrderBy(item => item.Technology.Name)
                .Select(item => new PortfolioTechnologyDto
                {
                    Id = item.Technology.Id,
                    Name = item.Technology.Name,
                    Icon = item.Technology.Icon,
                    Color = item.Technology.Color
                })
                .ToList(),
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        });
    }

    private static void ValidateRequest(
        string? title,
        string? slug,
        string? description,
        string? shortDescription,
        string? clientName,
        DateTime projectDate,
        string? thumbnail,
        string? websiteUrl,
        string? githubUrl,
        int displayOrder,
        Guid categoryId)
    {
        ValidateRequiredText(title, "Portfolio title", 250);
        ValidateRequiredText(slug, "Portfolio slug", 280);
        ValidateRequiredText(description, "Portfolio description");
        ValidateOptionalText(shortDescription, "Short description", 500);
        ValidateRequiredText(clientName, "Client name", 150);
        ValidateRequiredText(thumbnail, "Portfolio thumbnail", 1000);
        ValidateRequiredText(websiteUrl, "Portfolio website URL", 1000);
        ValidateOptionalText(githubUrl, "Portfolio GitHub URL", 1000);

        if (projectDate == default)
        {
            throw new InvalidOperationException(
                "Portfolio project date is required.");
        }

        if (displayOrder < 0)
        {
            throw new InvalidOperationException(
                "Portfolio display order cannot be negative.");
        }

        if (categoryId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "Portfolio category is required.");
        }
    }

    private static void ValidateRequiredText(
        string? value,
        string fieldName,
        int? maximumLength = null)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{fieldName} is required.");
        }

        if (maximumLength.HasValue && value.Trim().Length > maximumLength.Value)
        {
            throw new InvalidOperationException(
                $"{fieldName} cannot be longer than {maximumLength.Value} characters.");
        }
    }

    private static void ValidateOptionalText(
        string? value,
        string fieldName,
        int maximumLength)
    {
        if (!string.IsNullOrWhiteSpace(value) && value.Trim().Length > maximumLength)
        {
            throw new InvalidOperationException(
                $"{fieldName} cannot be longer than {maximumLength} characters.");
        }
    }

    private static string NormalizeSlug(string slug)
    {
        var parts = slug
            .Trim()
            .ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return string.Join("-", parts);
    }

    private static DateTime NormalizeUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
