using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Services;

namespace AriansLab.ApiTests;

public class BlogViewCountTests
{
    [Fact]
    public async Task PublishedPostDetail_IncrementsAndReturnsViewCount()
    {
        await using var dbContext = CreateDbContext();

        var author = new User
        {
            FullName = "Blog author",
            Email = "blog-author@example.com",
            NormalizedEmail = "BLOG-AUTHOR@EXAMPLE.COM",
            UserName = "blog-author",
            NormalizedUserName = "BLOG-AUTHOR",
            PasswordHash = "test-only",
            Role = UserRole.Admin,
            IsActive = true
        };
        var category = new BlogCategory
        {
            Name = "Development",
            Slug = "development"
        };
        var publishedPost = new BlogPost
        {
            AuthorId = author.Id,
            CategoryId = category.Id,
            Title = "Published post",
            Slug = "published-post",
            Excerpt = "Published post excerpt.",
            Content = "Published post content.",
            CoverImage = string.Empty,
            ReadTime = 5,
            ViewCount = 7,
            IsPublished = true,
            PublishedAt = DateTime.UtcNow
        };
        var draftPost = new BlogPost
        {
            AuthorId = author.Id,
            CategoryId = category.Id,
            Title = "Draft post",
            Slug = "draft-post",
            Excerpt = "Draft post excerpt.",
            Content = "Draft post content.",
            CoverImage = string.Empty,
            ReadTime = 3,
            ViewCount = 2,
            IsPublished = false
        };

        dbContext.Users.Add(author);
        dbContext.BlogCategories.Add(category);
        dbContext.BlogPosts.AddRange(publishedPost, draftPost);
        await dbContext.SaveChangesAsync();

        var service = new BlogReadService(dbContext);

        var firstView = await service.GetPublishedPostBySlugAsync(publishedPost.Slug);
        var secondView = await service.GetPublishedPostBySlugAsync(publishedPost.Slug);
        var draftView = await service.GetPublishedPostBySlugAsync(draftPost.Slug);

        Assert.NotNull(firstView);
        Assert.Equal(8, firstView!.ViewCount);
        Assert.NotNull(secondView);
        Assert.Equal(9, secondView!.ViewCount);
        Assert.Null(draftView);

        Assert.Equal(9, (await dbContext.BlogPosts.FindAsync(publishedPost.Id))!.ViewCount);
        Assert.Equal(2, (await dbContext.BlogPosts.FindAsync(draftPost.Id))!.ViewCount);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"BlogViewCountTests-{Guid.NewGuid():N}")
            .Options;

        return new ApplicationDbContext(options);
    }
}
