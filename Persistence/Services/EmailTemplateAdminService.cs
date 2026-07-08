using Application.DTOs.EmailTemplates;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class EmailTemplateAdminService : IEmailTemplateAdminService
{
    private readonly ApplicationDbContext _dbContext;

    public EmailTemplateAdminService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<EmailTemplateDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.EmailTemplates
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new EmailTemplateDto
            {
                Id = x.Id,
                Name = x.Name,
                Subject = x.Subject,
                Body = x.Body,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<EmailTemplateDto>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.EmailTemplates
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new EmailTemplateDto
            {
                Id = x.Id,
                Name = x.Name,
                Subject = x.Subject,
                Body = x.Body,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<EmailTemplateDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.EmailTemplates
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new EmailTemplateDto
            {
                Id = x.Id,
                Name = x.Name,
                Subject = x.Subject,
                Body = x.Body,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<EmailTemplateDto?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = NormalizeName(name);

        return await _dbContext.EmailTemplates
            .AsNoTracking()
            .Where(x => x.Name.ToLower() == normalizedName)
            .Select(x => new EmailTemplateDto
            {
                Id = x.Id,
                Name = x.Name,
                Subject = x.Subject,
                Body = x.Body,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<EmailTemplateDto> CreateAsync(
        CreateEmailTemplateRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateEmailTemplate(
            request.Name,
            request.Subject,
            request.Body
        );

        var normalizedName = NormalizeName(request.Name);

        var nameExists = await _dbContext.EmailTemplates
            .AnyAsync(
                x => x.Name.ToLower() == normalizedName,
                cancellationToken
            );

        if (nameExists)
        {
            throw new InvalidOperationException("An email template with this name already exists.");
        }

        var emailTemplate = new EmailTemplate
        {
            Name = request.Name.Trim(),
            Subject = request.Subject.Trim(),
            Body = request.Body.Trim(),
            IsActive = request.IsActive
        };

        await _dbContext.EmailTemplates.AddAsync(
            emailTemplate,
            cancellationToken
        );

        await _dbContext.SaveChangesAsync(cancellationToken);

        var createdTemplate = await GetByIdAsync(
            emailTemplate.Id,
            cancellationToken
        );

        return createdTemplate!;
    }

    public async Task<EmailTemplateDto?> UpdateAsync(
        Guid id,
        UpdateEmailTemplateRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateEmailTemplate(
            request.Name,
            request.Subject,
            request.Body
        );

        var emailTemplate = await _dbContext.EmailTemplates
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (emailTemplate is null)
        {
            return null;
        }

        var normalizedName = NormalizeName(request.Name);

        var duplicateNameExists = await _dbContext.EmailTemplates
            .AnyAsync(
                x => x.Id != id && x.Name.ToLower() == normalizedName,
                cancellationToken
            );

        if (duplicateNameExists)
        {
            throw new InvalidOperationException("An email template with this name already exists.");
        }

        emailTemplate.Name = request.Name.Trim();
        emailTemplate.Subject = request.Subject.Trim();
        emailTemplate.Body = request.Body.Trim();
        emailTemplate.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(
            emailTemplate.Id,
            cancellationToken
        );
    }

    public async Task<EmailTemplateDto?> UpdateActiveStatusAsync(
        Guid id,
        UpdateEmailTemplateActiveStatusRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var emailTemplate = await _dbContext.EmailTemplates
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (emailTemplate is null)
        {
            return null;
        }

        emailTemplate.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(
            emailTemplate.Id,
            cancellationToken
        );
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var emailTemplate = await _dbContext.EmailTemplates
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (emailTemplate is null)
        {
            return false;
        }

        _dbContext.EmailTemplates.Remove(emailTemplate);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static void ValidateEmailTemplate(
        string name,
        string subject,
        string body)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Email template name is required.");
        }

        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new InvalidOperationException("Email template subject is required.");
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            throw new InvalidOperationException("Email template body is required.");
        }
    }

    private static string NormalizeName(string name)
    {
        return name.Trim().ToLowerInvariant();
    }
}