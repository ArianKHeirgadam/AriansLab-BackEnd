using Application.DTOs.ContactMessages;
using Application.Interfaces;
using Domain.Entities;
using Persistence.Context;

namespace Persistence.Services;

public class ContactMessageService : IContactMessageService
{
    private readonly ApplicationDbContext _dbContext;

    public ContactMessageService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ContactMessageSubmissionResultDto> CreateAsync(
        CreateContactMessageRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var contactMessage = new ContactMessage
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            PhoneNumber = request.PhoneNumber?.Trim() ?? string.Empty,
            Subject = request.Subject.Trim(),
            Message = request.Message.Trim(),
            Company = request.Company?.Trim(),
            IsRead = false,
            RepliedAt = null
        };

        await _dbContext.ContactMessages.AddAsync(
            contactMessage,
            cancellationToken
        );

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ContactMessageSubmissionResultDto
        {
            Id = contactMessage.Id,
            CreatedAt = contactMessage.CreatedAt
        };
    }

    private static void ValidateRequest(CreateContactMessageRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            throw new InvalidOperationException("Full name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new InvalidOperationException("Email is required.");
        }

        if (!request.Email.Contains('@'))
        {
            throw new InvalidOperationException("Email format is invalid.");
        }

        if (string.IsNullOrWhiteSpace(request.Subject))
        {
            throw new InvalidOperationException("Subject is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            throw new InvalidOperationException("Message is required.");
        }
    }
}