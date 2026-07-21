using Application.DTOs.Invoices;
using Application.DTOs.Payments;
using Application.DTOs.Projects;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Services;

namespace AriansLab.ApiTests;

public sealed class PaymentWorkflowTests
{
    [Fact]
    public async Task ProjectPaymentApproval_FinalizesInvoiceAndStartsProject()
    {
        await using var dbContext = CreateDbContext();
        var (customer, _, plan) = await SeedUsersAndPlanAsync(dbContext);
        var projectService = new ProjectAdminService(dbContext);

        var project = await projectService.CreateAsync(new CreateProjectRequestDto
        {
            UserId = customer.Id,
            PricingPlanId = plan.Id,
            Title = "Payment workflow project",
            Description = "Created with a provisional invoice.",
            Status = ProjectStatus.InProgress,
            Progress = 25,
            Price = 1_000_000m,
            PaidAmount = 500_000m,
            CreateInitialInvoice = true,
            InvoiceDueDate = DateTime.SpecifyKind(
                DateTime.UtcNow.Date.AddDays(5),
                DateTimeKind.Unspecified),
            InvoiceDiscountAmount = 100_000m,
            InvoiceTaxAmount = 50_000m
        });

        Assert.Equal(ProjectStatus.Pending, project.Status);
        Assert.Equal((byte)0, project.Progress);
        Assert.Equal(0m, project.PaidAmount);

        var invoice = await dbContext.Invoices.SingleAsync(item =>
            item.ProjectId == project.Id);
        Assert.Equal(PaymentStatus.Pending, invoice.Status);
        Assert.Equal(950_000m, invoice.FinalAmount);
        Assert.Equal(DateTimeKind.Utc, invoice.DueDate.Kind);

        var submissionService = new PaymentSubmissionService(dbContext);
        var submitted = await submissionService.SubmitAsync(
            customer.Id,
            new SubmitPaymentRequestDto
            {
                InvoiceId = invoice.Id,
                TrackingCode = "۱۲۳۴۵۶۷۸۹۰",
                CardLastFour = "۴۳۲۱"
            });

        Assert.NotNull(submitted);
        Assert.Equal(950_000m, submitted!.Amount);
        Assert.Equal(PaymentStatus.Pending, submitted.Status);
        Assert.Equal("1234567890", submitted.TrackingCode);
        Assert.EndsWith("4321", submitted.CardPan);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            submissionService.SubmitAsync(
                customer.Id,
                new SubmitPaymentRequestDto
                {
                    InvoiceId = invoice.Id,
                    TrackingCode = "9876543210"
                }));

        var approved = await new PaymentAdminService(dbContext).UpdateStatusAsync(
            submitted.Id,
            new UpdatePaymentStatusRequestDto
            {
                Status = PaymentStatus.Paid,
                PaidAt = DateTime.SpecifyKind(
                    DateTime.UtcNow,
                    DateTimeKind.Unspecified)
            });

        Assert.NotNull(approved);
        var finalizedInvoice = await dbContext.Invoices.SingleAsync(item =>
            item.Id == invoice.Id);
        var startedProject = await dbContext.Projects.SingleAsync(item =>
            item.Id == project.Id);

        Assert.Equal(PaymentStatus.Paid, finalizedInvoice.Status);
        Assert.NotNull(finalizedInvoice.PaidAt);
        Assert.Equal(ProjectStatus.InProgress, startedProject.Status);
        Assert.NotNull(startedProject.StartDate);
        Assert.Equal(950_000m, startedProject.PaidAmount);
        Assert.Contains(
            await dbContext.Notifications.Where(item => item.UserId == customer.Id).ToListAsync(),
            item => item.Type == NotificationType.Success &&
                    item.Title.Contains("فاکتور نهایی"));

        var invoiceAdminService = new InvoiceAdminService(dbContext);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            invoiceAdminService.DeleteAsync(invoice.Id));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new PaymentAdminService(dbContext).DeleteAsync(submitted.Id));
    }

    [Fact]
    public async Task InvoiceCreation_DerivesCustomerFromProjectAndNormalizesDueDate()
    {
        await using var dbContext = CreateDbContext();
        var (customer, otherCustomer, plan) = await SeedUsersAndPlanAsync(dbContext);
        var project = await new ProjectAdminService(dbContext).CreateAsync(
            new CreateProjectRequestDto
            {
                UserId = customer.Id,
                PricingPlanId = plan.Id,
                Title = "Invoice owner project",
                Description = "Invoice owner validation project.",
                Status = ProjectStatus.Pending,
                Price = 500_000m
            });

        var dueDate = DateTime.SpecifyKind(
            DateTime.UtcNow.Date.AddDays(3),
            DateTimeKind.Unspecified);
        var service = new InvoiceAdminService(dbContext);
        var invoice = await service.CreateAsync(new CreateInvoiceRequestDto
        {
            UserId = Guid.Empty,
            ProjectId = project.Id,
            Amount = 500_000m,
            Status = PaymentStatus.Pending,
            DueDate = dueDate
        });

        Assert.Equal(customer.Id, invoice.UserId);
        Assert.Equal(DateTimeKind.Utc, invoice.DueDate.Kind);
        Assert.Equal(DateTime.SpecifyKind(dueDate, DateTimeKind.Utc), invoice.DueDate);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateInvoiceRequestDto
            {
                UserId = otherCustomer.Id,
                ProjectId = project.Id,
                Amount = 250_000m,
                Status = PaymentStatus.Pending,
                DueDate = dueDate
            }));
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"PaymentWorkflowTests-{Guid.NewGuid():N}")
            .Options;
        return new ApplicationDbContext(options);
    }

    private static async Task<(User Customer, User OtherCustomer, PricingPlan Plan)>
        SeedUsersAndPlanAsync(ApplicationDbContext dbContext)
    {
        var customer = CreateUser("customer", UserRole.Customer);
        var otherCustomer = CreateUser("other-customer", UserRole.Customer);
        var administrator = CreateUser("administrator", UserRole.Admin);
        var plan = new PricingPlan
        {
            Title = "Workflow plan",
            Description = "Test plan",
            Price = 1_000_000m,
            Duration = 30,
            DeliveryDays = 30,
            IsActive = true
        };

        dbContext.Users.AddRange(customer, otherCustomer, administrator);
        dbContext.PricingPlans.Add(plan);
        await dbContext.SaveChangesAsync();
        return (customer, otherCustomer, plan);
    }

    private static User CreateUser(string name, UserRole role)
    {
        var normalizedName = name.Replace("-", "_");
        var email = $"{name}@example.com";
        return new User
        {
            FullName = name,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            UserName = normalizedName,
            NormalizedUserName = normalizedName.ToUpperInvariant(),
            PasswordHash = "test-only",
            Role = role,
            IsActive = true
        };
    }
}
