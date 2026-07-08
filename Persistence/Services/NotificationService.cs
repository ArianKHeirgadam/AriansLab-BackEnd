using Application.DTOs.Notifications;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _dbContext;

    public NotificationService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<NotificationDto>> GetMyNotificationsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Notifications
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new NotificationDto
            {
                Id = x.Id,
                UserId = x.UserId,
                UserFullName = x.User.FullName,
                UserEmail = x.User.Email,
                Type = x.Type,
                Title = x.Title,
                Message = x.Message,
                IsRead = x.IsRead,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<NotificationDto>> GetMyUnreadNotificationsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Notifications
            .AsNoTracking()
            .Where(x => x.UserId == userId && !x.IsRead)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new NotificationDto
            {
                Id = x.Id,
                UserId = x.UserId,
                UserFullName = x.User.FullName,
                UserEmail = x.User.Email,
                Type = x.Type,
                Title = x.Title,
                Message = x.Message,
                IsRead = x.IsRead,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<NotificationDto?> GetMyNotificationByIdAsync(
        Guid userId,
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Notifications
            .AsNoTracking()
            .Where(x => x.Id == notificationId && x.UserId == userId)
            .Select(x => new NotificationDto
            {
                Id = x.Id,
                UserId = x.UserId,
                UserFullName = x.User.FullName,
                UserEmail = x.User.Email,
                Type = x.Type,
                Title = x.Title,
                Message = x.Message,
                IsRead = x.IsRead,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<NotificationDto?> MarkAsReadAsync(
        Guid userId,
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(
                x => x.Id == notificationId && x.UserId == userId,
                cancellationToken
            );

        if (notification is null)
        {
            return null;
        }

        notification.IsRead = true;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetMyNotificationByIdAsync(
            userId,
            notification.Id,
            cancellationToken
        );
    }

    public async Task<int> MarkAllAsReadAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var notifications = await _dbContext.Notifications
            .Where(x => x.UserId == userId && !x.IsRead)
            .ToListAsync(cancellationToken);

        if (!notifications.Any())
        {
            return 0;
        }

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return notifications.Count;
    }

    public async Task<bool> DeleteMyNotificationAsync(
        Guid userId,
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(
                x => x.Id == notificationId && x.UserId == userId,
                cancellationToken
            );

        if (notification is null)
        {
            return false;
        }

        _dbContext.Notifications.Remove(notification);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}