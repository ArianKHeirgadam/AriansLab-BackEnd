using Application.DTOs.SupportTickets;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class SupportTicketService : ISupportTicketService
{
    private readonly ApplicationDbContext _dbContext;

    public SupportTicketService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<SupportTicketListItemDto>> GetMyTicketsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.SupportTickets
            .AsNoTracking()
            .Where(x => x.UserId == userId)
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

    public async Task<SupportTicketDetailDto?> GetMyTicketByIdAsync(
        Guid userId,
        Guid ticketId,
        CancellationToken cancellationToken = default)
    {
        return await GetTicketDetailQuery()
            .Where(x => x.Id == ticketId && x.UserId == userId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<SupportTicketDetailDto> CreateAsync(
        Guid userId,
        CreateSupportTicketRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateCreateRequest(request);

        var userExists = await _dbContext.Users
            .AnyAsync(x => x.Id == userId && x.IsActive, cancellationToken);

        if (!userExists)
        {
            throw new InvalidOperationException("Active user was not found.");
        }

        var ticketNumber = await GenerateTicketNumberAsync(cancellationToken);

        var ticket = new SupportTicket
        {
            TicketNumber = ticketNumber,
            UserId = userId,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Status = TicketStatus.Open,
            Priority = request.Priority,
            Messages = new List<TicketMessage>
            {
                new()
                {
                    SenderId = userId,
                    Message = request.Description.Trim(),
                    Attachment = request.Attachment?.Trim(),
                    FileName = request.FileName?.Trim(),
                    FilePath = request.FilePath?.Trim(),
                    FileSize = request.FileSize,
                    IsRead = false,
                    IsAdminMessage = false
                }
            }
        };

        await _dbContext.SupportTickets.AddAsync(ticket, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var createdTicket = await GetMyTicketByIdAsync(
            userId,
            ticket.Id,
            cancellationToken
        );

        return createdTicket!;
    }

    public async Task<SupportTicketDetailDto?> AddMessageAsync(
        Guid userId,
        Guid ticketId,
        CreateTicketMessageRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateMessageRequest(request);

        var ticket = await _dbContext.SupportTickets
            .FirstOrDefaultAsync(
                x => x.Id == ticketId && x.UserId == userId,
                cancellationToken
            );

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
            SenderId = userId,
            Message = request.Message.Trim(),
            Attachment = request.Attachment?.Trim(),
            FileName = request.FileName?.Trim(),
            FilePath = request.FilePath?.Trim(),
            FileSize = request.FileSize,
            IsRead = false,
            IsAdminMessage = false
        };

        ticket.Status = TicketStatus.Open;

        await _dbContext.TicketMessages.AddAsync(message, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetMyTicketByIdAsync(
            userId,
            ticket.Id,
            cancellationToken
        );
    }

    public async Task<SupportTicketDetailDto?> CloseMyTicketAsync(
        Guid userId,
        Guid ticketId,
        CancellationToken cancellationToken = default)
    {
        var ticket = await _dbContext.SupportTickets
            .FirstOrDefaultAsync(
                x => x.Id == ticketId && x.UserId == userId,
                cancellationToken
            );

        if (ticket is null)
        {
            return null;
        }

        ticket.Status = TicketStatus.Closed;
        ticket.ClosedAt = DateTime.UtcNow;
        ticket.ClosedByUserId = userId;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetMyTicketByIdAsync(
            userId,
            ticket.Id,
            cancellationToken
        );
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

    private async Task<string> GenerateTicketNumberAsync(
        CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow;
        var prefix = $"TCK-{today:yyyyMMdd}";

        var countToday = await _dbContext.SupportTickets
            .CountAsync(
                x => x.TicketNumber.StartsWith(prefix),
                cancellationToken
            );

        return $"{prefix}-{countToday + 1:000}";
    }

    private static void ValidateCreateRequest(
        CreateSupportTicketRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new InvalidOperationException("Ticket title is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            throw new InvalidOperationException("Ticket description is required.");
        }

        if (request.FileSize is < 0)
        {
            throw new InvalidOperationException("File size cannot be negative.");
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