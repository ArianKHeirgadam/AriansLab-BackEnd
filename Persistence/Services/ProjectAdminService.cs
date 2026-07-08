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

        var projectCode = string.IsNullOrWhiteSpace(request.ProjectCode)
            ? await GenerateProjectCodeAsync(cancellationToken)
            : request.ProjectCode.Trim().ToUpperInvariant();

        var projectCodeExists = await _dbContext.Projects
            .AnyAsync(x => x.ProjectCode == projectCode, cancellationToken);

        if (projectCodeExists)
        {
            throw new InvalidOperationException("A project with this project code already exists.");
        }

        var project = new Project
        {
            UserId = request.UserId,
            PricingPlanId = request.PricingPlanId,
            ProjectCode = projectCode,
            EstimatedDeliveryDate = request.EstimatedDeliveryDate,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Status = request.Status,
            Progress = request.Progress,
            Price = request.Price,
            PaidAmount = request.PaidAmount,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            AdminNote = request.AdminNote?.Trim(),
            CustomerComment = request.CustomerComment?.Trim()
        };

        await _dbContext.Projects.AddAsync(project, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var createdProject = await GetByIdAsync(project.Id, cancellationToken);

        return createdProject!;
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

        var project = await _dbContext.Projects
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (project is null)
        {
            return null;
        }

        project.UserId = request.UserId;
        project.PricingPlanId = request.PricingPlanId;
        project.EstimatedDeliveryDate = request.EstimatedDeliveryDate;
        project.Title = request.Title.Trim();
        project.Description = request.Description.Trim();
        project.Status = request.Status;
        project.Progress = request.Progress;
        project.Price = request.Price;
        project.PaidAmount = request.PaidAmount;
        project.StartDate = request.StartDate;
        project.EndDate = request.EndDate;
        project.AdminNote = request.AdminNote?.Trim();
        project.CustomerComment = request.CustomerComment?.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(project.Id, cancellationToken);
    }

    public async Task<ProjectDetailDto?> UpdateStatusAsync(
        Guid id,
        UpdateProjectStatusRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.Progress > 100)
        {
            throw new InvalidOperationException("Progress cannot be greater than 100.");
        }

        var project = await _dbContext.Projects
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (project is null)
        {
            return null;
        }

        project.Status = request.Status;
        project.Progress = request.Progress;
        project.AdminNote = request.AdminNote?.Trim();

        if (request.Status == ProjectStatus.InProgress && project.StartDate is null)
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

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(project.Id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var project = await _dbContext.Projects
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (project is null)
        {
            return false;
        }

        _dbContext.Projects.Remove(project);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task ValidateUserAndPricingPlanAsync(
        Guid userId,
        Guid pricingPlanId,
        CancellationToken cancellationToken)
    {
        var userExists = await _dbContext.Users
            .AnyAsync(x => x.Id == userId && x.IsActive, cancellationToken);

        if (!userExists)
        {
            throw new InvalidOperationException("Active user was not found.");
        }

        var pricingPlanExists = await _dbContext.PricingPlans
            .AnyAsync(x => x.Id == pricingPlanId && x.IsActive, cancellationToken);

        if (!pricingPlanExists)
        {
            throw new InvalidOperationException("Active pricing plan was not found.");
        }
    }

    private static void ValidateProjectFields(
        string title,
        string description,
        byte progress,
        decimal price,
        decimal paidAmount)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new InvalidOperationException("Project title is required.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new InvalidOperationException("Project description is required.");
        }

        if (progress > 100)
        {
            throw new InvalidOperationException("Progress cannot be greater than 100.");
        }

        if (price < 0)
        {
            throw new InvalidOperationException("Project price cannot be negative.");
        }

        if (paidAmount < 0)
        {
            throw new InvalidOperationException("Paid amount cannot be negative.");
        }

        if (paidAmount > price)
        {
            throw new InvalidOperationException("Paid amount cannot be greater than project price.");
        }
    }

    private async Task<string> GenerateProjectCodeAsync(
        CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow;
        var prefix = $"PRJ-{today:yyyyMMdd}";

        var countToday = await _dbContext.Projects
            .CountAsync(
                x => x.ProjectCode.StartsWith(prefix),
                cancellationToken
            );

        return $"{prefix}-{countToday + 1:000}";
    }
}