using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace AriansLab.Api.Seed;

public static class BlogSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("BlogSeeder");

        var adminEmail = configuration["AdminSeed:Email"] ?? "admin@arianslab.com";
        var normalizedAdminEmail = adminEmail.Trim().ToUpperInvariant();

        var admin = await dbContext.Users
            .FirstOrDefaultAsync(x =>
                x.NormalizedEmail == normalizedAdminEmail &&
                x.Role == UserRole.Admin);

        if (admin is null)
        {
            logger.LogWarning("Blog seed skipped because admin user was not found.");
            return;
        }

        var hasPublishedPosts = await dbContext.BlogPosts.AnyAsync(x => x.IsPublished);

        if (hasPublishedPosts)
        {
            return;
        }

        var now = DateTime.UtcNow;

        var technologyCategory = await dbContext.BlogCategories
            .FirstOrDefaultAsync(x => x.Slug == "technology");

        if (technologyCategory is null)
        {
            technologyCategory = new BlogCategory
            {
                Id = Guid.NewGuid(),
                Name = "Technology",
                Slug = "technology",
                CreatedAt = now,
                IsDeleted = false
            };

            await dbContext.BlogCategories.AddAsync(technologyCategory);
        }

        var businessCategory = await dbContext.BlogCategories
            .FirstOrDefaultAsync(x => x.Slug == "business");

        if (businessCategory is null)
        {
            businessCategory = new BlogCategory
            {
                Id = Guid.NewGuid(),
                Name = "Business",
                Slug = "business",
                CreatedAt = now,
                IsDeleted = false
            };

            await dbContext.BlogCategories.AddAsync(businessCategory);
        }

        await dbContext.SaveChangesAsync();

        var posts = new List<BlogPost>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Getting Started with AriansLab",
                Slug = "getting-started-with-arianslab",
                Excerpt = "A short introduction to AriansLab and how the platform is structured.",
                Content = "AriansLab is a production-grade corporate platform built with ASP.NET Core, SQL Server, Clean Architecture, and a modern frontend stack.",
                CoverImage = "/images/blog/getting-started-with-arianslab.jpg",
                ReadTime = 4,
                ViewCount = 0,
                IsPublished = true,
                PublishedAt = now.AddMinutes(-30),
                SeoTitle = "Getting Started with AriansLab",
                SeoDescription = "Introduction to the AriansLab platform and backend architecture.",
                Keywords = "AriansLab, ASP.NET Core, Clean Architecture",
                CategoryId = technologyCategory.Id,
                AuthorId = admin.Id,
                CreatedAt = now,
                IsDeleted = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Why Custom JWT Authentication Matters",
                Slug = "why-custom-jwt-authentication-matters",
                Excerpt = "A practical look at custom JWT authentication in production APIs.",
                Content = "Custom JWT authentication gives the backend full control over token generation, claims, refresh tokens, and user access rules.",
                CoverImage = "/images/blog/custom-jwt-authentication.jpg",
                ReadTime = 5,
                ViewCount = 0,
                IsPublished = true,
                PublishedAt = now.AddMinutes(-20),
                SeoTitle = "Custom JWT Authentication in ASP.NET Core",
                SeoDescription = "Why custom JWT authentication is useful in production ASP.NET Core APIs.",
                Keywords = "JWT, Authentication, ASP.NET Core, Refresh Token",
                CategoryId = technologyCategory.Id,
                AuthorId = admin.Id,
                CreatedAt = now,
                IsDeleted = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Building a Better Digital Business Presence",
                Slug = "building-a-better-digital-business-presence",
                Excerpt = "How a professional digital platform can improve credibility and growth.",
                Content = "A strong digital presence is no longer optional for modern businesses. A structured website, portfolio, content system, and secure dashboard help businesses grow.",
                CoverImage = "/images/blog/digital-business-presence.jpg",
                ReadTime = 3,
                ViewCount = 0,
                IsPublished = true,
                PublishedAt = now.AddMinutes(-10),
                SeoTitle = "Building a Better Digital Business Presence",
                SeoDescription = "How businesses can use a professional digital platform to improve credibility.",
                Keywords = "Business, Digital Presence, Portfolio, Website",
                CategoryId = businessCategory.Id,
                AuthorId = admin.Id,
                CreatedAt = now,
                IsDeleted = false
            }
        };

        await dbContext.BlogPosts.AddRangeAsync(posts);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Blog seed completed successfully. Posts count: {Count}", posts.Count);
    }
}