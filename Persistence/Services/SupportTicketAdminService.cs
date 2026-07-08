using Application.DTOs.SupportTickets;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class SupportTicketAdminService : ISupportTicketAdminService
{
    private readonly ApplicationDbContext _dbContext;

    public SupportTicketAdminService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<SupportTicketListItemDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.SupportTickets
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new SupportTicketListItemDto
            {
                Id = x.Id,
                TicketNumber = x.TicketNumber,
                UserId = x.UserId,
                CustomerFullName = x.User.FullName,
                CustomerEmail = x.User.Email,
                Title = x.Title,
                Status = x.Status,
                Priority = x.Priority,
                AssignedToUserId = x.AssignedToUserId,
                AssignedToFullName = x.AssignedToUser != null
                    ? x.AssignedToUser.FullName
                    : null,
                ClosedAt = x.ClosedAt,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                MessagesCount = x.Messages.Count
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<SupportTicketDetailDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await GetTicketDetailQuery()
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<SupportTicketDetailDto?> ReplyAsync(
        Guid adminUserId,
        Guid ticketId,
        CreateTicketMessageRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateMessageRequest(request);

        var adminExists = await _dbContext.Users
            .AnyAsync(x => x.Id == adminUserId && x.IsActive, cancellationToken);

        if (!adminExists)
        {
            throw new InvalidOperationException("Active admin user was not found.");
        }

        var ticket = await _dbContext.SupportTickets
            .FirstOrDefaultAsync(x => x.Id == ticketId, cancellationToken);

        if (ticket is null)
        {
            return null;
        }

        if (ticket.Status == TicketStatus.Closed)
        {
            throw new InvalidOperationException("Closed tickets cannot receive new messages.");
        }

        var message = new TicketMessage
        {
            TicketId = ticket.Id,
            SenderId = adminUserId,
            Message = request.Message.Trim(),
            Attachment = request.Attachment?.Trim(),
            FileName = request.FileName?.Trim(),
            FilePath = request.FilePath?.Trim(),
            FileSize = request.FileSize,
            IsRead = false,
            IsAdminMessage = true
        };

        ticket.Status = TicketStatus.Answered;

        await _dbContext.TicketMessages.AddAsync(message, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(ticket.Id, cancellationToken);
    }

    public async Task<SupportTicketDetailDto?> UpdateStatusAsync(
        Guid ticketId,
        UpdateSupportTicketStatusRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var ticket = await _dbContext.SupportTickets
            .FirstOrDefaultAsync(x => x.Id == ticketId, cancellationToken);

        if (ticket is null)
        {
            return null;
        }

        if (request.AssignedToUserId is not null)
        {
            await ValidateAssignedUserAsync(
                request.AssignedToUserId.Value,
                cancellationToken
            );
        }

        ticket.Status = request.Status;
        ticket.Priority = request.Priority;
        ticket.AssignedToUserId = request.AssignedToUserId;

        if (request.Status == TicketStatus.Closed)
        {
            ticket.ClosedAt ??= DateTime.UtcNow;
            ticket.ClosedByUserId ??= request.AssignedToUserId;
        }
        else
        {
            ticket.ClosedAt = null;
            ticket.ClosedByUserId = null;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(ticket.Id, cancellationToken);
    }

    public async Task<SupportTicketDetailDto?> AssignAsync(
        Guid ticketId,
        AssignSupportTicketRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var ticket = await _dbContext.SupportTickets
            .FirstOrDefaultAsync(x => x.Id == ticketId, cancellationToken);

        if (ticket is null)
        {
            return null;
        }

        if (request.AssignedToUserId is not null)
        {
            await ValidateAssignedUserAsync(
                request.AssignedToUserId.Value,
                cancellationToken
            );
        }

        ticket.AssignedToUserId = request.AssignedToUserId;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(ticket.Id, cancellationToken);
    }

    public async Task<SupportTicketDetailDto?> CloseAsync(
        Guid adminUserId,
        Guid ticketId,
        CancellationToken cancellationToken = default)
    {
        var ticket = await _dbContext.SupportTickets
            .FirstOrDefaultAsync(x => x.Id == ticketId, cancellationToken);

        if (ticket is null)
        {
            return null;
        }

        ticket.Status = TicketStatus.Closed;
        ticket.ClosedAt = DateTime.UtcNow;
        ticket.ClosedByUserId = adminUserId;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(ticket.Id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var ticket = await _dbContext.SupportTickets
            .Include(x => x.Messages)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (ticket is null)
        {
            return false;
        }

        if (ticket.Messages.Any())
        {
            _dbContext.TicketMessages.RemoveRange(ticket.Messages);
        }

        _dbContext.SupportTickets.Remove(ticket);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private IQueryable<SupportTicketDetailDto> GetTicketDetailQuery()
    {
        return _dbContext.SupportTickets
            .AsNoTracking()
            .Select(x => new SupportTicketDetailDto
            {
                Id = x.Id,
                TicketNumber = x.TicketNumber,
                UserId = x.UserId,
                CustomerFullName = x.User.FullName,
                CustomerEmail = x.User.Email,
                Title = x.Title,
                Description = x.Description,
                Status = x.Status,
                Priority = x.Priority,
                AssignedToUserId = x.AssignedToUserId,
                AssignedToFullName = x.AssignedToUser != null
                    ? x.AssignedToUser.FullName
                    : null,
                ClosedByUserId = x.ClosedByUserId,
                ClosedByFullName = x.ClosedByUser != null
                    ? x.ClosedByUser.FullName
                    : null,
                ClosedAt = x.ClosedAt,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                Messages = x.Messages
                    .OrderBy(m => m.CreatedAt)
                    .Select(m => new TicketMessageDto
                    {
                        Id = m.Id,
                        TicketId = m.TicketId,
                        SenderId = m.SenderId,
                        SenderFullName = m.Sender.FullName,
                        SenderEmail = m.Sender.Email,
                        SenderRole = m.Sender.Role,
                        Message = m.Message,
                        Attachment = m.Attachment,
                        FileName = m.FileName,
                        FilePath = m.FilePath,
                        FileSize = m.FileSize,
                        IsRead = m.IsRead,
                        IsAdminMessage = m.IsAdminMessage,
                        CreatedAt = m.CreatedAt
                    })
                    .ToList()
            });
    }

    private async Task ValidateAssignedUserAsync(
        Guid assignedToUserId,
        CancellationToken cancellationToken)
    {
        var userExists = await _dbContext.Users
            .AnyAsync(
                x => x.Id == assignedToUserId && x.IsActive,
                cancellationToken
            );

        if (!userExists)
        {
            throw new InvalidOperationException("Assigned user was not found.");
        }
    }

    private static void ValidateMessageRequest(
        CreateTicketMessageRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            throw new InvalidOperationException("Message is required.");
        }

        if (request.FileSize is < 0)
        {
            throw new InvalidOperationException("File size cannot be negative.");
        }
    }
}