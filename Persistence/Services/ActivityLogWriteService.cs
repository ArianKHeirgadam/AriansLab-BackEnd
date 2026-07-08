using Application.DTOs.Logs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class ActivityLogWriteService : IActivityLogWriteService
{
    private readonly ApplicationDbContext _dbContext;

    public ActivityLogWriteService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task CreateAsync(
        CreateActivityLogRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.UserId == Guid.Empty)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(request.Activity))
        {
            return;
        }

        var userExists = await _dbContext.Users
            .AnyAsync(x => x.Id == request.UserId, cancellationToken);

        if (!userExists)
        {
            return;
        }

        var activityLog = new ActivityLog
        {
            UserId = request.UserId,
            Activity = request.Activity.Trim(),
            Description = NormalizeNullableString(request.Description),
            IpAddress = NormalizeNullableString(request.IpAddress),
            UserAgent = NormalizeNullableString(request.UserAgent)
        };

        await _dbContext.ActivityLogs.AddAsync(
            activityLog,
            cancellationToken
        );

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string? NormalizeNullableString(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}