using Application.DTOs.Projects;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class ProjectReadService : IProjectReadService
{
    private readonly ApplicationDbContext _dbContext;

    public ProjectReadService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ProjectListItemDto>> GetMyProjectsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projects
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ProjectListItemDto
            {
                Id = x.Id,
                ProjectCode = x.ProjectCode,
                Title = x.Title,
                Status = x.Status,
                Progress = x.Progress,
                Price = x.Price,
                PaidAmount = x.PaidAmount,
                EstimatedDeliveryDate = x.EstimatedDeliveryDate,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ProjectDetailDto?> GetMyProjectByIdAsync(
        Guid userId,
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projects
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.PricingPlan)
            .Where(x => x.Id == projectId && x.UserId == userId)
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

    public async Task<ProjectDetailDto?> UpdateMyCustomerCommentAsync(
        Guid userId,
        Guid projectId,
        UpdateProjectCustomerCommentRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var project = await _dbContext.Projects
            .FirstOrDefaultAsync(
                x => x.Id == projectId && x.UserId == userId,
                cancellationToken
            );

        if (project is null)
        {
            return null;
        }

        project.CustomerComment = request.CustomerComment?.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetMyProjectByIdAsync(
            userId,
            project.Id,
            cancellationToken
        );
    }
}