using Application.DTOs.Portfolio;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Services;

namespace AriansLab.ApiTests;

public class PortfolioAdminServiceTests
{
    [Fact]
    public async Task CreateAndUpdate_NormalizeProjectDateToUtc()
    {
        await using var dbContext = CreateDbContext();
        var category = await AddCategoryAsync(dbContext);
        var service = new PortfolioAdminService(dbContext);

        var unspecifiedProjectDate = DateTime.SpecifyKind(
            new DateTime(2026, 7, 21, 14, 30, 0),
            DateTimeKind.Unspecified);

        var created = await service.CreateAsync(new CreatePortfolioRequestDto
        {
            Title = "Portfolio test",
            Slug = " Portfolio Test ",
            Description = "Created by the portfolio admin test.",
            ShortDescription = "Test item",
            ClientName = "Test client",
            ProjectDate = unspecifiedProjectDate,
            Thumbnail = "/images/portfolio-test.jpg",
            WebsiteUrl = "https://example.com",
            GithubUrl = "https://github.com/example/portfolio-test",
            IsFeatured = true,
            DisplayOrder = 1,
            CategoryId = category.Id
        });

        Assert.Equal("portfolio-test", created.Slug);
        Assert.Equal(DateTimeKind.Utc, created.ProjectDate.Kind);
        Assert.Equal(
            DateTime.SpecifyKind(unspecifiedProjectDate, DateTimeKind.Utc),
            created.ProjectDate);

        var localProjectDate = DateTime.SpecifyKind(
            new DateTime(2026, 8, 1, 10, 15, 0),
            DateTimeKind.Local);

        var updated = await service.UpdateAsync(created.Id, new UpdatePortfolioRequestDto
        {
            Title = "Updated portfolio test",
            Slug = "updated portfolio test",
            Description = "Updated by the portfolio admin test.",
            ClientName = "Updated client",
            ProjectDate = localProjectDate,
            Thumbnail = "/images/updated-portfolio-test.jpg",
            WebsiteUrl = "https://example.org",
            IsFeatured = false,
            DisplayOrder = 2,
            CategoryId = category.Id
        });

        Assert.NotNull(updated);
        Assert.Equal("updated-portfolio-test", updated!.Slug);
        Assert.Equal(DateTimeKind.Utc, updated.ProjectDate.Kind);
        Assert.Equal(localProjectDate.ToUniversalTime(), updated.ProjectDate);
    }

    [Fact]
    public async Task Delete_SoftDeletesPortfolioAndRelatedRecords()
    {
        await using var dbContext = CreateDbContext();
        var category = await AddCategoryAsync(dbContext);
        var service = new PortfolioAdminService(dbContext);

        var created = await service.CreateAsync(new CreatePortfolioRequestDto
        {
            Title = "Portfolio delete test",
            Slug = "portfolio-delete-test",
            Description = "Portfolio delete test.",
            ClientName = "Test client",
            ProjectDate = DateTime.UtcNow,
            Thumbnail = "/images/portfolio-delete-test.jpg",
            WebsiteUrl = "https://example.com",
            DisplayOrder = 0,
            CategoryId = category.Id
        });

        var technology = new Technology { Name = "Test technology" };
        dbContext.Technologies.Add(technology);
        dbContext.PortfolioImages.Add(new PortfolioImage
        {
            PortfolioId = created.Id,
            ImageUrl = "/images/portfolio-delete-detail.jpg",
            IsCover = true
        });
        dbContext.PortfolioTechnologies.Add(new PortfolioTechnology
        {
            PortfolioId = created.Id,
            TechnologyId = technology.Id
        });
        await dbContext.SaveChangesAsync();

        Assert.True(await service.DeleteAsync(created.Id));
        Assert.Null(await service.GetByIdAsync(created.Id));

        var deletedPortfolio = await dbContext.Portfolios
            .IgnoreQueryFilters()
            .SingleAsync(item => item.Id == created.Id);
        var deletedImage = await dbContext.PortfolioImages
            .IgnoreQueryFilters()
            .SingleAsync(item => item.PortfolioId == created.Id);
        var deletedTechnology = await dbContext.PortfolioTechnologies
            .IgnoreQueryFilters()
            .SingleAsync(item => item.PortfolioId == created.Id);

        Assert.True(deletedPortfolio.IsDeleted);
        Assert.True(deletedImage.IsDeleted);
        Assert.True(deletedTechnology.IsDeleted);
        Assert.NotNull(deletedPortfolio.DeletedAt);
        Assert.NotNull(deletedImage.DeletedAt);
        Assert.NotNull(deletedTechnology.DeletedAt);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"PortfolioAdminTests-{Guid.NewGuid():N}")
            .Options;

        return new ApplicationDbContext(options);
    }

    private static async Task<PortfolioCategory> AddCategoryAsync(
        ApplicationDbContext dbContext)
    {
        var category = new PortfolioCategory
        {
            Name = "Web development",
            Slug = $"web-development-{Guid.NewGuid():N}",
            IsActive = true
        };

        dbContext.PortfolioCategories.Add(category);
        await dbContext.SaveChangesAsync();

        return category;
    }
}
