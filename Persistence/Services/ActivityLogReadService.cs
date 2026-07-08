using Application.DTOs.Logs;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class ActivityLogReadService : IActivityLogReadService
{
    private readonly ApplicationDbContext _dbContext;

    public ActivityLogReadService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ActivityLogDto>> GetAllAsync(
        Guid? userId = null,
        string? activity = null,
        DateTime? from = null,
        DateTime? to = null,
        int skip = 0,
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        skip = Math.Max(0, skip);
        take = Math.Clamp(take, 1, 500);

        var query = _dbContext.ActivityLogs
            .AsNoTracking()
            .AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(x => x.UserId == userId.Value);
        }

        if (!string.IsNullOrWhiteSpace(activity))
        {
            var normalizedActivity = activity.Trim();

            query = query.Where(x =>
                EF.Functions.Like(x.Activity, $"%{normalizedActivity}%"));
        }

        if (from.HasValue)
        {
            query = query.Where(x => x.CreatedAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(x => x.CreatedAt <= to.Value);
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip(skip)
            .Take(take)
            .Select(x => new ActivityLogDto
            {
                Id = x.Id,
                UserId = x.UserId,
                UserFullName = x.User.FullName,
                UserEmail = x.User.Email,
                Activity = x.Activity,
                Description = x.Description,
                IpAddress = x.IpAddress,
                UserAgent = x.UserAgent,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ActivityLogDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ActivityLogs
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ActivityLogDto
            {
                Id = x.Id,
                UserId = x.UserId,
                UserFullName = x.User.FullName,
                UserEmail = x.User.Email,
                Activity = x.Activity,
                Description = x.Description,
                IpAddress = x.IpAddress,
                UserAgent = x.UserAgent,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}