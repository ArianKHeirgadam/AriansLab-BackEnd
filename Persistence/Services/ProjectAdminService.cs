using Application.DTOs.Projects;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class ProjectAdminService : IProjectAdminService
{
    private readonly ApplicationDbContext _dbContext;

    public ProjectAdminService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ProjectDetailDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projects
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.PricingPlan)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ProjectDetailDto
            {
                Id = x.Id,
                UserId = x.UserId,
                CustomerFullName = x.User.FullName,
                CustomerEmail = x.User.Email,
                PricingPlanId = x.PricingPlanId,
                PricingPlanTitle = x.PricingPlan.Title,
                ProjectCode = x.ProjectCode,
                Title = x.Title,
                Description = x.Description,
                Status = x.Status,
                Progress = x.Progress,
                Price = x.Price,
                PaidAmount = x.PaidAmount,
                EstimatedDeliveryDate = x.EstimatedDeliveryDate,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                AdminNote = x.AdminNote,
                CustomerComment = x.CustomerComment,
                IsCustomerCommentApproved = x.IsCustomerCommentApproved,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ProjectDetailDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projects
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.PricingPlan)
            .Where(x => x.Id == id)
            .Select(x => new ProjectDetailDto
            {
                Id = x.Id,
                UserId = x.UserId,
                CustomerFullName = x.User.FullName,
                CustomerEmail = x.User.Email,
                PricingPlanId = x.PricingPlanId,
                PricingPlanTitle = x.PricingPlan.Title,
                ProjectCode = x.ProjectCode,
                Title = x.Title,
                Description = x.Description,
                Status = x.Status,
                Progress = x.Progress,
                Price = x.Price,
                PaidAmount = x.PaidAmount,
                EstimatedDeliveryDate = x.EstimatedDeliveryDate,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                AdminNote = x.AdminNote,
                CustomerComment = x.CustomerComment,
                IsCustomerCommentApproved = x.IsCustomerCommentApproved,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ProjectDetailDto> CreateAsync(
        CreateProjectRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await ValidateUserAndPricingPlanAsync(
            request.UserId,
            request.PricingPlanId,
            cancellationToken
        );

        ValidateProjectFields(
            request.Title,
            request.Description,
            request.Progress,
            request.Price,
            request.PaidAmount
        );

        DateTime? invoiceDueDate = null;
        if (request.CreateInitialInvoice)
        {
            ValidateInitialInvoice(
                request.Price,
                request.InvoiceDiscountAmount,
                request.InvoiceTaxAmount);
            invoiceDueDate = NormalizeUtc(request.InvoiceDueDate)
                ?? DateTime.UtcNow.AddDays(7);

            if (invoiceDueDate.Value < DateTime.UtcNow.Date)
            {
                throw new InvalidOperationException(
                    "Invoice due date cannot be in the past.");
            }
        }

        var estimatedDeliveryDate = NormalizeUtc(
            request.EstimatedDeliveryDate
        );

        var startDate = NormalizeUtc(
            request.StartDate
        );

        var endDate = NormalizeUtc(
            request.EndDate
        );

        ValidateProjectDates(
            startDate,
            endDate,
            estimatedDeliveryDate
        );

        var projectCode = string.IsNullOrWhiteSpace(request.ProjectCode)
            ? await GenerateProjectCodeAsync(cancellationToken)
            : request.ProjectCode.Trim().ToUpperInvariant();

        var projectCodeExists = await _dbContext.Projects
            .IgnoreQueryFilters()
            .AnyAsync(
                x => x.ProjectCode == projectCode,
                cancellationToken
            );

        if (projectCodeExists)
        {
            throw new InvalidOperationException(
                "A project with this project code already exists."
            );
        }

        var project = new Project
        {
            UserId = request.UserId,
            PricingPlanId = request.PricingPlanId,
            ProjectCode = projectCode,
            EstimatedDeliveryDate = estimatedDeliveryDate,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Status = request.CreateInitialInvoice
                ? ProjectStatus.Pending
                : request.Status,
            Progress = request.CreateInitialInvoice
                ? (byte)0
                : request.Progress,
            Price = request.Price,
            PaidAmount = request.CreateInitialInvoice ? 0 : request.PaidAmount,
            StartDate = request.CreateInitialInvoice ? null : startDate,
            EndDate = request.CreateInitialInvoice ? null : endDate,
            AdminNote = NormalizeOptionalText(request.AdminNote),
            CustomerComment = NormalizeOptionalText(request.CustomerComment),
            IsCustomerCommentApproved = false
        };

        await _dbContext.Projects.AddAsync(
            project,
            cancellationToken
        );

        if (request.CreateInitialInvoice)
        {
            var invoiceNumber = await GenerateInvoiceNumberAsync(cancellationToken);
            var invoice = new Invoice
            {
                UserId = project.UserId,
                ProjectId = project.Id,
                InvoiceNumber = invoiceNumber,
                Amount = request.Price,
                DiscountAmount = request.InvoiceDiscountAmount,
                TaxAmount = request.InvoiceTaxAmount,
                FinalAmount = request.Price -
                    request.InvoiceDiscountAmount +
                    request.InvoiceTaxAmount,
                Status = PaymentStatus.Pending,
                Description = NormalizeOptionalText(request.InvoiceDescription)
                    ?? $"پیش‌فاکتور شروع پروژه {project.Title}",
                DueDate = invoiceDueDate!.Value,
                IsDeletedInvoice = false
            };

            await _dbContext.Invoices.AddAsync(invoice, cancellationToken);
            await _dbContext.Notifications.AddAsync(new Notification
            {
                UserId = project.UserId,
                Type = NotificationType.Info,
                Title = "پیش‌فاکتور پروژه صادر شد",
                Message = $"پیش‌فاکتور {invoiceNumber} برای پروژه {project.Title} آماده پرداخت است.",
                IsRead = false
            }, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken
        );

        var createdProject = await GetByIdAsync(
            project.Id,
            cancellationToken
        );

        return createdProject
            ?? throw new InvalidOperationException(
                "Project was created but could not be retrieved."
            );
    }

    public async Task<ProjectDetailDto?> UpdateAsync(
        Guid id,
        UpdateProjectRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await ValidateUserAndPricingPlanAsync(
            request.UserId,
            request.PricingPlanId,
            cancellationToken
        );

        ValidateProjectFields(
            request.Title,
            request.Description,
            request.Progress,
            request.Price,
            request.PaidAmount
        );

        var estimatedDeliveryDate = NormalizeUtc(
            request.EstimatedDeliveryDate
        );

        var startDate = NormalizeUtc(
            request.StartDate
        );

        var endDate = NormalizeUtc(
            request.EndDate
        );

        ValidateProjectDates(
            startDate,
            endDate,
            estimatedDeliveryDate
        );

        var project = await _dbContext.Projects
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken
            );

        if (project is null)
        {
            return null;
        }

        await EnsureProjectCanStartAsync(
            project,
            request.Status,
            cancellationToken);

        project.UserId = request.UserId;
        project.PricingPlanId = request.PricingPlanId;
        project.EstimatedDeliveryDate = estimatedDeliveryDate;
        project.Title = request.Title.Trim();
        project.Description = request.Description.Trim();
        project.Status = request.Status;
        project.Progress = request.Progress;
        project.Price = request.Price;
        // PaidAmount is derived exclusively from administrator-approved
        // payment records and cannot be edited directly.
        project.StartDate = startDate;
        project.EndDate = endDate;
        project.AdminNote = NormalizeOptionalText(request.AdminNote);
        var normalizedCustomerComment = NormalizeOptionalText(
            request.CustomerComment
        );

        if (!string.Equals(
                project.CustomerComment,
                normalizedCustomerComment,
                StringComparison.Ordinal))
        {
            project.CustomerComment = normalizedCustomerComment;
            project.IsCustomerCommentApproved = false;
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken
        );

        return await GetByIdAsync(
            project.Id,
            cancellationToken
        );
    }

    public async Task<ProjectDetailDto?> UpdateStatusAsync(
        Guid id,
        UpdateProjectStatusRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.Progress > 100)
        {
            throw new InvalidOperationException(
                "Progress cannot be greater than 100."
            );
        }

        var project = await _dbContext.Projects
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken
            );

        if (project is null)
        {
            return null;
        }

        await EnsureProjectCanStartAsync(
            project,
            request.Status,
            cancellationToken);

        project.Status = request.Status;
        project.Progress = request.Progress;
        project.AdminNote = NormalizeOptionalText(request.AdminNote);

        if (request.Status == ProjectStatus.InProgress &&
            project.StartDate is null)
        {
            project.StartDate = DateTime.UtcNow;
        }

        if (request.Status == ProjectStatus.Compeleted)
        {
            project.Progress = 100;
            project.EndDate ??= DateTime.UtcNow;
        }

        if (request.Status == ProjectStatus.Cancelled)
        {
            project.EndDate ??= DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken
        );

        return await GetByIdAsync(
            project.Id,
            cancellationToken
        );
    }

    public async Task<ProjectDetailDto?> UpdateCustomerCommentApprovalAsync(
        Guid id,
        UpdateProjectCustomerCommentApprovalRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var project = await _dbContext.Projects
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken
            );

        if (project is null || string.IsNullOrWhiteSpace(project.CustomerComment))
        {
            return null;
        }

        project.IsCustomerCommentApproved = request.IsApproved;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(project.Id, cancellationToken);
    }

    public async Task<bool> DeleteCustomerCommentAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var project = await _dbContext.Projects
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken
            );

        if (project is null || string.IsNullOrWhiteSpace(project.CustomerComment))
        {
            return false;
        }

        project.CustomerComment = null;
        project.IsCustomerCommentApproved = false;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var project = await _dbContext.Projects
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken
            );

        if (project is null)
        {
            return false;
        }

        _dbContext.Projects.Remove(project);

        await _dbContext.SaveChangesAsync(
            cancellationToken
        );

        return true;
    }

    private async Task ValidateUserAndPricingPlanAsync(
        Guid userId,
        Guid pricingPlanId,
        CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "User id is required."
            );
        }

        if (pricingPlanId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "Pricing plan id is required."
            );
        }

        var userExists = await _dbContext.Users
            .AnyAsync(
                x => x.Id == userId && x.IsActive,
                cancellationToken
            );

        if (!userExists)
        {
            throw new InvalidOperationException(
                "Active user was not found."
            );
        }

        var pricingPlanExists = await _dbContext.PricingPlans
            .AnyAsync(
                x => x.Id == pricingPlanId && x.IsActive,
                cancellationToken
            );

        if (!pricingPlanExists)
        {
            throw new InvalidOperationException(
                "Active pricing plan was not found."
            );
        }
    }

    private static void ValidateProjectFields(
        string? title,
        string? description,
        byte progress,
        decimal price,
        decimal paidAmount)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new InvalidOperationException(
                "Project title is required."
            );
        }

        if (title.Trim().Length > 200)
        {
            throw new InvalidOperationException(
                "Project title cannot be longer than 200 characters."
            );
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new InvalidOperationException(
                "Project description is required."
            );
        }

        if (description.Trim().Length > 3000)
        {
            throw new InvalidOperationException(
                "Project description cannot be longer than 3000 characters."
            );
        }

        if (progress > 100)
        {
            throw new InvalidOperationException(
                "Progress cannot be greater than 100."
            );
        }

        if (price < 0)
        {
            throw new InvalidOperationException(
                "Project price cannot be negative."
            );
        }

        if (paidAmount < 0)
        {
            throw new InvalidOperationException(
                "Paid amount cannot be negative."
            );
        }

        if (paidAmount > price)
        {
            throw new InvalidOperationException(
                "Paid amount cannot be greater than project price."
            );
        }
    }

    private static void ValidateProjectDates(
        DateTime? startDate,
        DateTime? endDate,
        DateTime? estimatedDeliveryDate)
    {
        if (startDate.HasValue &&
            endDate.HasValue &&
            endDate.Value < startDate.Value)
        {
            throw new InvalidOperationException(
                "Project end date cannot be before the start date."
            );
        }

        if (startDate.HasValue &&
            estimatedDeliveryDate.HasValue &&
            estimatedDeliveryDate.Value < startDate.Value)
        {
            throw new InvalidOperationException(
                "Estimated delivery date cannot be before the start date."
            );
        }
    }

    private async Task EnsureProjectCanStartAsync(
        Project project,
        ProjectStatus nextStatus,
        CancellationToken cancellationToken)
    {
        if (project.Status != ProjectStatus.Pending ||
            nextStatus != ProjectStatus.InProgress)
        {
            return;
        }

        var invoiceStates = await _dbContext.Invoices
            .AsNoTracking()
            .Where(item => item.ProjectId == project.Id)
            .Select(item => item.Status)
            .ToListAsync(cancellationToken);

        if (invoiceStates.Count > 0 &&
            invoiceStates.All(status => status != PaymentStatus.Paid))
        {
            throw new InvalidOperationException(
                "Approve a project payment before moving it to in progress.");
        }
    }

    private static void ValidateInitialInvoice(
        decimal amount,
        decimal discountAmount,
        decimal taxAmount)
    {
        if (amount <= 0)
        {
            throw new InvalidOperationException(
                "Project price must be greater than zero when creating an invoice.");
        }

        if (discountAmount < 0 || taxAmount < 0)
        {
            throw new InvalidOperationException(
                "Invoice discount and tax cannot be negative.");
        }

        if (discountAmount > amount)
        {
            throw new InvalidOperationException(
                "Invoice discount cannot be greater than the project price.");
        }

        if (amount - discountAmount + taxAmount <= 0)
        {
            throw new InvalidOperationException(
                "Invoice final amount must be greater than zero.");
        }
    }

    private static DateTime? NormalizeUtc(DateTime? value)
    {
        if (!value.HasValue)
        {
            return null;
        }

        return value.Value.Kind switch
        {
            DateTimeKind.Utc => value.Value,

            DateTimeKind.Local =>
                value.Value.ToUniversalTime(),

            _ => DateTime.SpecifyKind(
                value.Value,
                DateTimeKind.Utc
            )
        };
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private async Task<string> GenerateProjectCodeAsync(
        CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow;
        var prefix = $"PRJ-{today:yyyyMMdd}";

        var sequence = await _dbContext.Projects
            .IgnoreQueryFilters()
            .CountAsync(
                x => x.ProjectCode.StartsWith(prefix),
                cancellationToken
            ) + 1;

        while (true)
        {
            var projectCode = $"{prefix}-{sequence:000}";

            var exists = await _dbContext.Projects
                .IgnoreQueryFilters()
                .AnyAsync(
                    x => x.ProjectCode == projectCode,
                    cancellationToken
                );

            if (!exists)
            {
                return projectCode;
            }

            sequence++;
        }
    }

    private async Task<string> GenerateInvoiceNumberAsync(
        CancellationToken cancellationToken)
    {
        var prefix = $"INV-{DateTime.UtcNow:yyyyMMdd}";

        while (true)
        {
            var suffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
            var invoiceNumber = $"{prefix}-{suffix}";
            var exists = await _dbContext.Invoices
                .IgnoreQueryFilters()
                .AnyAsync(
                    item => item.InvoiceNumber == invoiceNumber,
                    cancellationToken);

            if (!exists)
            {
                return invoiceNumber;
            }
        }
    }
}
