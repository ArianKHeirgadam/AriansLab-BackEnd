using Application.DTOs.Logs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class AuditLogWriteService : IAuditLogWriteService
{
    private readonly ApplicationDbContext _dbContext;

    public AuditLogWriteService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task CreateAsync(
        CreateAuditLogRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Action))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(request.EntityName))
        {
            return;
        }

        Guid? userId = null;

        if (request.UserId.HasValue && request.UserId.Value != Guid.Empty)
        {
            var userExists = await _dbContext.Users
                .AnyAsync(x => x.Id == request.UserId.Value, cancellationToken);

            if (userExists)
            {
                userId = request.UserId.Value;
            }
        }

        var auditLog = new AuditLog
        {
            UserId = userId,
            Action = request.Action.Trim(),
            EntityName = request.EntityName.Trim(),
            EntityId = NormalizeString(request.EntityId),
            OldValues = NormalizeNullableString(request.OldValues),
            NewValues = NormalizeNullableString(request.NewValues),
            IpAddress = NormalizeNullableString(request.IpAddress),
            UserAgent = NormalizeNullableString(request.UserAgent)
        };

        await _dbContext.AuditLogs.AddAsync(
            auditLog,
            cancellationToken
        );

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string NormalizeString(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim();
    }

    private static string? NormalizeNullableString(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}