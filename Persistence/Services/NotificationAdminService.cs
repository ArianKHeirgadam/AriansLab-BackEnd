using Application.DTOs.Notifications;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class NotificationAdminService : INotificationAdminService
{
    private readonly ApplicationDbContext _dbContext;

    public NotificationAdminService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<NotificationDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Notifications
            .AsNoTracking()
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

    public async Task<List<NotificationDto>> GetUnreadAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Notifications
            .AsNoTracking()
            .Where(x => !x.IsRead)
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

    public async Task<NotificationDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Notifications
            .AsNoTracking()
            .Where(x => x.Id == id)
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

    public async Task<NotificationDto> CreateAsync(
        CreateNotificationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await ValidateUserAsync(request.UserId, cancellationToken);
        ValidateNotificationText(request.Title, request.Message);

        var notification = new Notification
        {
            UserId = request.UserId,
            Type = request.Type,
            Title = request.Title.Trim(),
            Message = request.Message.Trim(),
            IsRead = request.IsRead
        };

        await _dbContext.Notifications.AddAsync(notification, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var createdNotification = await GetByIdAsync(
            notification.Id,
            cancellationToken
        );

        return createdNotification!;
    }

    public async Task<NotificationDto?> UpdateAsync(
        Guid id,
        UpdateNotificationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await ValidateUserAsync(request.UserId, cancellationToken);
        ValidateNotificationText(request.Title, request.Message);

        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (notification is null)
        {
            return null;
        }

        notification.UserId = request.UserId;
        notification.Type = request.Type;
        notification.Title = request.Title.Trim();
        notification.Message = request.Message.Trim();
        notification.IsRead = request.IsRead;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(notification.Id, cancellationToken);
    }

    public async Task<NotificationDto?> UpdateReadStatusAsync(
        Guid id,
        UpdateNotificationReadStatusRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (notification is null)
        {
            return null;
        }

        notification.IsRead = request.IsRead;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(notification.Id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (notification is null)
        {
            return false;
        }

        _dbContext.Notifications.Remove(notification);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task ValidateUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var userExists = await _dbContext.Users
            .AnyAsync(x => x.Id == userId && x.IsActive, cancellationToken);

        if (!userExists)
        {
            throw new InvalidOperationException("Active user was not found.");
        }
    }

    private static void ValidateNotificationText(
        string title,
        string message)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new InvalidOperationException("Notification title is required.");
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new InvalidOperationException("Notification message is required.");
        }
    }
}