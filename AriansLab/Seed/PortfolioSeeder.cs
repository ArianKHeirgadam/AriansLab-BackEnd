using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace AriansLab.Api.Seed;

public static class PortfolioSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("PortfolioSeeder");

        var hasPortfolioItems = await dbContext.Portfolios.AnyAsync();

        if (hasPortfolioItems)
        {
            return;
        }

        var now = DateTime.UtcNow;

        var webCategory = new PortfolioCategory
        {
            Id = Guid.NewGuid(),
            Name = "Web Development",
            Slug = "web-development",
            DisplayOrder = 1,
            IsActive = true,
            CreatedAt = now,
            IsDeleted = false
        };

        var brandingCategory = new PortfolioCategory
        {
            Id = Guid.NewGuid(),
            Name = "Digital Branding",
            Slug = "digital-branding",
            DisplayOrder = 2,
            IsActive = true,
            CreatedAt = now,
            IsDeleted = false
        };

        await dbContext.PortfolioCategories.AddRangeAsync(
            webCategory,
            brandingCategory);

        var aspNetTechnology = new Technology
        {
            Id = Guid.NewGuid(),
            Name = "ASP.NET Core",
            Icon = "dotnet",
            Color = "#512BD4",
            CreatedAt = now,
            IsDeleted = false
        };

        var nextTechnology = new Technology
        {
            Id = Guid.NewGuid(),
            Name = "Next.js",
            Icon = "nextjs",
            Color = "#000000",
            CreatedAt = now,
            IsDeleted = false
        };

        var sqlTechnology = new Technology
        {
            Id = Guid.NewGuid(),
            Name = "SQL Server",
            Icon = "sqlserver",
            Color = "#CC2927",
            CreatedAt = now,
            IsDeleted = false
        };

        var tailwindTechnology = new Technology
        {
            Id = Guid.NewGuid(),
            Name = "Tailwind CSS",
            Icon = "tailwindcss",
            Color = "#38BDF8",
            CreatedAt = now,
            IsDeleted = false
        };

        await dbContext.Technologies.AddRangeAsync(
            aspNetTechnology,
            nextTechnology,
            sqlTechnology,
            tailwindTechnology);

        var ariansLabPortfolio = new Portfolio
        {
            Id = Guid.NewGuid(),
            Title = "AriansLab Corporate Platform",
            Slug = "arianslab-corporate-platform",
            ShortDescription = "A production-grade corporate platform with blog, portfolio, products, pricing, dashboard, and authentication.",
            Description = "AriansLab Corporate Platform is a complete business website and dashboard system built with ASP.NET Core, SQL Server, Clean Architecture, JWT authentication, and a modern frontend architecture.",
            ClientName = "AriansLab",
            ProjectDate = now.Date.AddDays(-30),
            Thumbnail = "/images/portfolio/arianslab-corporate-platform/thumbnail.jpg",
            WebsiteUrl = "https://arianslab.com",
            GithubUrl = "https://github.com/ArianKHeirgadam/AriansLab-BackEnd",
            IsFeatured = true,
            DisplayOrder = 1,
            PortfolioCategoryId = webCategory.Id,
            CreatedAt = now,
            IsDeleted = false
        };

        var businessWebsitePortfolio = new Portfolio
        {
            Id = Guid.NewGuid(),
            Title = "Modern Business Website",
            Slug = "modern-business-website",
            ShortDescription = "A clean, responsive website for presenting services, portfolio items, and business credibility.",
            Description = "This project focuses on building a professional business website with strong visual hierarchy, responsive layouts, service presentation, portfolio sections, and conversion-focused content structure.",
            ClientName = "Business Client",
            ProjectDate = now.Date.AddDays(-20),
            Thumbnail = "/images/portfolio/modern-business-website/thumbnail.jpg",
            WebsiteUrl = "https://example.com",
            GithubUrl = null,
            IsFeatured = true,
            DisplayOrder = 2,
            PortfolioCategoryId = webCategory.Id,
            CreatedAt = now,
            IsDeleted = false
        };

        var brandIdentityPortfolio = new Portfolio
        {
            Id = Guid.NewGuid(),
            Title = "Digital Brand Identity System",
            Slug = "digital-brand-identity-system",
            ShortDescription = "A digital identity system for consistent brand presence across website and social platforms.",
            Description = "This project includes digital brand structure, visual consistency, content direction, and presentation strategy for a professional online presence.",
            ClientName = "Brand Client",
            ProjectDate = now.Date.AddDays(-10),
            Thumbnail = "/images/portfolio/digital-brand-identity-system/thumbnail.jpg",
            WebsiteUrl = "https://example.org",
            GithubUrl = null,
            IsFeatured = false,
            DisplayOrder = 3,
            PortfolioCategoryId = brandingCategory.Id,
            CreatedAt = now,
            IsDeleted = false
        };

        await dbContext.Portfolios.AddRangeAsync(
            ariansLabPortfolio,
            businessWebsitePortfolio,
            brandIdentityPortfolio);

        await dbContext.PortfolioImages.AddRangeAsync(
            new PortfolioImage
            {
                Id = Guid.NewGuid(),
                PortfolioId = ariansLabPortfolio.Id,
                ImageUrl = "/images/portfolio/arianslab-corporate-platform/01.jpg",
                IsCover = true,
                DisplayOrder = 1,
                CreatedAt = now,
                IsDeleted = false
            },
            new PortfolioImage
            {
                Id = Guid.NewGuid(),
                PortfolioId = ariansLabPortfolio.Id,
                ImageUrl = "/images/portfolio/arianslab-corporate-platform/02.jpg",
                IsCover = false,
                DisplayOrder = 2,
                CreatedAt = now,
                IsDeleted = false
            },
            new PortfolioImage
            {
                Id = Guid.NewGuid(),
                PortfolioId = businessWebsitePortfolio.Id,
                ImageUrl = "/images/portfolio/modern-business-website/01.jpg",
                IsCover = true,
                DisplayOrder = 1,
                CreatedAt = now,
                IsDeleted = false
            },
            new PortfolioImage
            {
                Id = Guid.NewGuid(),
                PortfolioId = brandIdentityPortfolio.Id,
                ImageUrl = "/images/portfolio/digital-brand-identity-system/01.jpg",
                IsCover = true,
                DisplayOrder = 1,
                CreatedAt = now,
                IsDeleted = false
            }
        );

        await dbContext.PortfolioTechnologies.AddRangeAsync(
            new PortfolioTechnology
            {
                Id = Guid.NewGuid(),
                PortfolioId = ariansLabPortfolio.Id,
                TechnologyId = aspNetTechnology.Id,
                CreatedAt = now,
                IsDeleted = false
            },
            new PortfolioTechnology
            {
                Id = Guid.NewGuid(),
                PortfolioId = ariansLabPortfolio.Id,
                TechnologyId = sqlTechnology.Id,
                CreatedAt = now,
                IsDeleted = false
            },
            new PortfolioTechnology
            {
                Id = Guid.NewGuid(),
                PortfolioId = ariansLabPortfolio.Id,
                TechnologyId = nextTechnology.Id,
                CreatedAt = now,
                IsDeleted = false
            },
            new PortfolioTechnology
            {
                Id = Guid.NewGuid(),
                PortfolioId = businessWebsitePortfolio.Id,
                TechnologyId = nextTechnology.Id,
                CreatedAt = now,
                IsDeleted = false
            },
            new PortfolioTechnology
            {
                Id = Guid.NewGuid(),
                PortfolioId = businessWebsitePortfolio.Id,
                TechnologyId = tailwindTechnology.Id,
                CreatedAt = now,
                IsDeleted = false
            }
        );

        await dbContext.SaveChangesAsync();

        logger.LogInformation("Portfolio seed completed successfully. Items count: {Count}", 3);
    }
}