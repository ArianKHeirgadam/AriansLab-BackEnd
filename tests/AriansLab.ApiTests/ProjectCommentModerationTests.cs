using Application.DTOs.Projects;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Services;

namespace AriansLab.ApiTests;

public class ProjectCommentModerationTests
{
    [Fact]
    public async Task CustomerComment_IsListedAndCanBeModeratedByAdmin()
    {
        await using var dbContext = CreateDbContext();

        var customer = new User
        {
            FullName = "Project customer",
            Email = "project-customer@example.com",
            NormalizedEmail = "PROJECT-CUSTOMER@EXAMPLE.COM",
            UserName = "project-customer",
            NormalizedUserName = "PROJECT-CUSTOMER",
            PasswordHash = "test-only",
            Role = UserRole.Customer,
            IsActive = true
        };
        var pricingPlan = new PricingPlan
        {
            Title = "Project plan",
            Description = "Plan used by the moderation test.",
            Price = 1_000_000m,
            Duration = 30,
            DeliveryDays = 30,
            IsActive = true
        };
        var project = new Project
        {
            UserId = customer.Id,
            PricingPlanId = pricingPlan.Id,
            ProjectCode = "PRJ-COMMENT-TEST",
            Title = "Moderated project",
            Description = "Project used by the customer comment moderation test.",
            Status = ProjectStatus.InProgress,
            Progress = 50,
            Price = pricingPlan.Price,
            CustomerComment = "Previously approved comment",
            IsCustomerCommentApproved = true
        };

        dbContext.Users.Add(customer);
        dbContext.PricingPlans.Add(pricingPlan);
        dbContext.Projects.Add(project);
        await dbContext.SaveChangesAsync();

        var customerService = new ProjectReadService(dbContext);
        var submitted = await customerService.UpdateMyCustomerCommentAsync(
            customer.Id,
            project.Id,
            new UpdateProjectCustomerCommentRequestDto
            {
                CustomerComment = "  New customer comment  "
            });

        Assert.NotNull(submitted);
        Assert.Equal("New customer comment", submitted!.CustomerComment);
        Assert.False(submitted.IsCustomerCommentApproved);

        var adminService = new ProjectAdminService(dbContext);
        var listed = Assert.Single(
            (await adminService.GetAllAsync())
                .Where(item => !string.IsNullOrWhiteSpace(item.CustomerComment)));

        Assert.Equal(project.Id, listed.Id);
        Assert.Equal(customer.FullName, listed.CustomerFullName);
        Assert.False(listed.IsCustomerCommentApproved);

        var approved = await adminService.UpdateCustomerCommentApprovalAsync(
            project.Id,
            new UpdateProjectCustomerCommentApprovalRequestDto
            {
                IsApproved = true
            });

        Assert.NotNull(approved);
        Assert.True(approved!.IsCustomerCommentApproved);

        var unchanged = await customerService.UpdateMyCustomerCommentAsync(
            customer.Id,
            project.Id,
            new UpdateProjectCustomerCommentRequestDto
            {
                CustomerComment = "New customer comment"
            });

        Assert.NotNull(unchanged);
        Assert.True(unchanged!.IsCustomerCommentApproved);

        var edited = await customerService.UpdateMyCustomerCommentAsync(
            customer.Id,
            project.Id,
            new UpdateProjectCustomerCommentRequestDto
            {
                CustomerComment = "Edited after approval"
            });

        Assert.NotNull(edited);
        Assert.False(edited!.IsCustomerCommentApproved);

        var customerListItem = Assert.Single(
            await customerService.GetMyProjectsAsync(customer.Id));
        Assert.Equal("Edited after approval", customerListItem.CustomerComment);
        Assert.False(customerListItem.IsCustomerCommentApproved);

        Assert.True(await adminService.DeleteCustomerCommentAsync(project.Id));

        var cleared = await adminService.GetByIdAsync(project.Id);
        Assert.NotNull(cleared);
        Assert.Null(cleared!.CustomerComment);
        Assert.False(cleared.IsCustomerCommentApproved);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"ProjectCommentModerationTests-{Guid.NewGuid():N}")
            .Options;

        return new ApplicationDbContext(options);
    }
}
