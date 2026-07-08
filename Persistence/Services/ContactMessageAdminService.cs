using Application.DTOs.ContactMessages;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class ContactMessageAdminService : IContactMessageAdminService
{
    private readonly ApplicationDbContext _dbContext;

    public ContactMessageAdminService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<AdminContactMessageDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ContactMessages
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new AdminContactMessageDto
            {
                Id = x.Id,
                FullName = x.FullName,
                Email = x.Email,
                PhoneNumber = x.PhoneNumber,
                Subject = x.Subject,
                Message = x.Message,
                Company = x.Company,
                IsRead = x.IsRead,
                RepliedAt = x.RepliedAt,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AdminContactMessageDto>> GetUnreadAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ContactMessages
            .AsNoTracking()
            .Where(x => !x.IsRead)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new AdminContactMessageDto
            {
                Id = x.Id,
                FullName = x.FullName,
                Email = x.Email,
                PhoneNumber = x.PhoneNumber,
                Subject = x.Subject,
                Message = x.Message,
                Company = x.Company,
                IsRead = x.IsRead,
                RepliedAt = x.RepliedAt,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<AdminContactMessageDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ContactMessages
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new AdminContactMessageDto
            {
                Id = x.Id,
                FullName = x.FullName,
                Email = x.Email,
                PhoneNumber = x.PhoneNumber,
                Subject = x.Subject,
                Message = x.Message,
                Company = x.Company,
                IsRead = x.IsRead,
                RepliedAt = x.RepliedAt,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<AdminContactMessageDto?> MarkAsReadAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var contactMessage = await _dbContext.ContactMessages
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (contactMessage is null)
        {
            return null;
        }

        contactMessage.IsRead = true;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(contactMessage.Id, cancellationToken);
    }

    public async Task<AdminContactMessageDto?> MarkAsRepliedAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var contactMessage = await _dbContext.ContactMessages
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (contactMessage is null)
        {
            return null;
        }

        contactMessage.IsRead = true;
        contactMessage.RepliedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(contactMessage.Id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var contactMessage = await _dbContext.ContactMessages
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (contactMessage is null)
        {
            return false;
        }

        _dbContext.ContactMessages.Remove(contactMessage);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}