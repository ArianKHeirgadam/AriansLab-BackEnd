using Application.DTOs.Logs;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class AuditLogReadService : IAuditLogReadService
{
    private readonly ApplicationDbContext _dbContext;

    public AuditLogReadService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<AuditLogDto>> GetAllAsync(
        Guid? userId = null,
        string? action = null,
        string? entityName = null,
        string? entityId = null,
        DateTime? from = null,
        DateTime? to = null,
        int skip = 0,
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        skip = Math.Max(0, skip);
        take = Math.Clamp(take, 1, 500);

        var query = _dbContext.AuditLogs
            .AsNoTracking()
            .AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(x => x.UserId == userId.Value);
        }

        if (!string.IsNullOrWhiteSpace(action))
        {
            var normalizedAction = action.Trim();

            query = query.Where(x =>
                EF.Functions.Like(x.Action, $"%{normalizedAction}%"));
        }

        if (!string.IsNullOrWhiteSpace(entityName))
        {
            var normalizedEntityName = entityName.Trim();

            query = query.Where(x =>
                EF.Functions.Like(x.EntityName, $"%{normalizedEntityName}%"));
        }

        if (!string.IsNullOrWhiteSpace(entityId))
        {
            var normalizedEntityId = entityId.Trim();

            query = query.Where(x =>
                EF.Functions.Like(x.EntityId, $"%{normalizedEntityId}%"));
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
            .Select(x => new AuditLogDto
            {
                Id = x.Id,
                UserId = x.UserId,
                UserFullName = x.User != null ? x.User.FullName : null,
                UserEmail = x.User != null ? x.User.Email : null,
                Action = x.Action,
                EntityName = x.EntityName,
                EntityId = x.EntityId,
                OldValues = x.OldValues,
                NewValues = x.NewValues,
                IpAddress = x.IpAddress,
                UserAgent = x.UserAgent,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<AuditLogDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.AuditLogs
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new AuditLogDto
            {
                Id = x.Id,
                UserId = x.UserId,
                UserFullName = x.User != null ? x.User.FullName : null,
                UserEmail = x.User != null ? x.User.Email : null,
                Action = x.Action,
                EntityName = x.EntityName,
                EntityId = x.EntityId,
                OldValues = x.OldValues,
                NewValues = x.NewValues,
                IpAddress = x.IpAddress,
                UserAgent = x.UserAgent,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}