using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace AriansLab.Api.Seed;

public static class ServicesSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("ServicesSeeder");

        var hasServices = await dbContext.Services.AnyAsync();

        if (hasServices)
        {
            return;
        }

        var now = DateTime.UtcNow;

        var webDevelopmentService = new Service
        {
            Id = Guid.NewGuid(),
            Title = "Web Development",
            Slug = "web-development",
            Thumbnail = "/images/services/web-development/thumbnail.jpg",
            CoverImage = "/images/services/web-development/cover.jpg",
            ShortDescription = "Modern, responsive, and scalable websites for businesses.",
            Description = "We build modern business websites and web platforms with clean architecture, responsive design, SEO-ready structure, and production-grade backend integration.",
            EstimatedDeliveryDays = 14,
            IsFeatured = true,
            DisplayOrder = 1,
            Icon = "code",
            IsActive = true,
            CreatedAt = now,
            IsDeleted = false,
            Features = new List<ServiceFeature>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Responsive frontend implementation",
                    DisplayOrder = 1,
                    CreatedAt = now,
                    IsDeleted = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Clean backend API integration",
                    DisplayOrder = 2,
                    CreatedAt = now,
                    IsDeleted = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "SEO-ready page structure",
                    DisplayOrder = 3,
                    CreatedAt = now,
                    IsDeleted = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Performance-focused development",
                    DisplayOrder = 4,
                    CreatedAt = now,
                    IsDeleted = false
                }
            }
        };

        var backendDevelopmentService = new Service
        {
            Id = Guid.NewGuid(),
            Title = "Backend Development",
            Slug = "backend-development",
            Thumbnail = "/images/services/backend-development/thumbnail.jpg",
            CoverImage = "/images/services/backend-development/cover.jpg",
            ShortDescription = "Secure and maintainable APIs for real business workflows.",
            Description = "We design and develop backend systems using ASP.NET Core, SQL Server, authentication, authorization, layered architecture, and scalable data access patterns.",
            EstimatedDeliveryDays = 21,
            IsFeatured = true,
            DisplayOrder = 2,
            Icon = "server",
            IsActive = true,
            CreatedAt = now,
            IsDeleted = false,
            Features = new List<ServiceFeature>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "RESTful API development",
                    DisplayOrder = 1,
                    CreatedAt = now,
                    IsDeleted = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "SQL Server database design",
                    DisplayOrder = 2,
                    CreatedAt = now,
                    IsDeleted = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "JWT authentication and authorization",
                    DisplayOrder = 3,
                    CreatedAt = now,
                    IsDeleted = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Clean Architecture structure",
                    DisplayOrder = 4,
                    CreatedAt = now,
                    IsDeleted = false
                }
            }
        };

        var uiUxService = new Service
        {
            Id = Guid.NewGuid(),
            Title = "UI/UX Design",
            Slug = "ui-ux-design",
            Thumbnail = "/images/services/ui-ux-design/thumbnail.jpg",
            CoverImage = "/images/services/ui-ux-design/cover.jpg",
            ShortDescription = "User-focused interfaces for websites, dashboards, and platforms.",
            Description = "We create clear, modern, and conversion-focused user interfaces with strong visual hierarchy, consistent components, responsive layouts, and practical user experience decisions.",
            EstimatedDeliveryDays = 10,
            IsFeatured = true,
            DisplayOrder = 3,
            Icon = "palette",
            IsActive = true,
            CreatedAt = now,
            IsDeleted = false,
            Features = new List<ServiceFeature>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Landing page UI design",
                    DisplayOrder = 1,
                    CreatedAt = now,
                    IsDeleted = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Dashboard layout design",
                    DisplayOrder = 2,
                    CreatedAt = now,
                    IsDeleted = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Responsive design system",
                    DisplayOrder = 3,
                    CreatedAt = now,
                    IsDeleted = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "User journey improvement",
                    DisplayOrder = 4,
                    CreatedAt = now,
                    IsDeleted = false
                }
            }
        };

        var seoService = new Service
        {
            Id = Guid.NewGuid(),
            Title = "SEO Optimization",
            Slug = "seo-optimization",
            Thumbnail = "/images/services/seo-optimization/thumbnail.jpg",
            CoverImage = "/images/services/seo-optimization/cover.jpg",
            ShortDescription = "Technical and content structure improvements for better visibility.",
            Description = "We improve website structure, metadata, page performance, semantic content, and technical SEO foundations to help search engines understand and rank your pages better.",
            EstimatedDeliveryDays = 7,
            IsFeatured = false,
            DisplayOrder = 4,
            Icon = "search",
            IsActive = true,
            CreatedAt = now,
            IsDeleted = false,
            Features = new List<ServiceFeature>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Metadata optimization",
                    DisplayOrder = 1,
                    CreatedAt = now,
                    IsDeleted = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Technical SEO review",
                    DisplayOrder = 2,
                    CreatedAt = now,
                    IsDeleted = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Content structure improvement",
                    DisplayOrder = 3,
                    CreatedAt = now,
                    IsDeleted = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Performance recommendations",
                    DisplayOrder = 4,
                    CreatedAt = now,
                    IsDeleted = false
                }
            }
        };

        await dbContext.Services.AddRangeAsync(
            webDevelopmentService,
            backendDevelopmentService,
            uiUxService,
            seoService
        );

        await dbContext.SaveChangesAsync();

        logger.LogInformation(
            "Services seed completed successfully. Items count: {Count}",
            4
        );
    }
}