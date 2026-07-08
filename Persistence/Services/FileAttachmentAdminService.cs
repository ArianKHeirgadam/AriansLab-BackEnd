using Application.DTOs.Files;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class FileAttachmentAdminService : IFileAttachmentAdminService
{
    private readonly ApplicationDbContext _dbContext;

    public FileAttachmentAdminService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<FileAttachmentDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.FileAttachments
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new FileAttachmentDto
            {
                Id = x.Id,
                FileName = x.FileName,
                OriginalFileName = x.OriginalFileName,
                FilePath = x.FilePath,
                Extension = x.Extension,
                ContentType = x.ContentType,
                Size = x.Size,
                UploadedByUserId = x.UploadedByUserId,
                UploadedByFullName = x.UploadedByUser.FullName,
                UploadedByEmail = x.UploadedByUser.Email,
                Module = x.Module,
                ReferenceId = x.ReferenceId,
                IsPublic = x.IsPublic,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<FileAttachmentDto>> GetByReferenceAsync(
        string module,
        Guid referenceId,
        CancellationToken cancellationToken = default)
    {
        var normalizedModule = module.Trim().ToLowerInvariant();

        return await _dbContext.FileAttachments
            .AsNoTracking()
            .Where(x =>
                x.Module.ToLower() == normalizedModule &&
                x.ReferenceId == referenceId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new FileAttachmentDto
            {
                Id = x.Id,
                FileName = x.FileName,
                OriginalFileName = x.OriginalFileName,
                FilePath = x.FilePath,
                Extension = x.Extension,
                ContentType = x.ContentType,
                Size = x.Size,
                UploadedByUserId = x.UploadedByUserId,
                UploadedByFullName = x.UploadedByUser.FullName,
                UploadedByEmail = x.UploadedByUser.Email,
                Module = x.Module,
                ReferenceId = x.ReferenceId,
                IsPublic = x.IsPublic,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<FileAttachmentDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.FileAttachments
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new FileAttachmentDto
            {
                Id = x.Id,
                FileName = x.FileName,
                OriginalFileName = x.OriginalFileName,
                FilePath = x.FilePath,
                Extension = x.Extension,
                ContentType = x.ContentType,
                Size = x.Size,
                UploadedByUserId = x.UploadedByUserId,
                UploadedByFullName = x.UploadedByUser.FullName,
                UploadedByEmail = x.UploadedByUser.Email,
                Module = x.Module,
                ReferenceId = x.ReferenceId,
                IsPublic = x.IsPublic,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<FileAttachmentDto> CreateAsync(
        CreateFileAttachmentRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await ValidateUserAsync(request.UploadedByUserId, cancellationToken);
        ValidateAttachment(request);

        var attachment = new FileAttachment
        {
            FileName = request.FileName.Trim(),
            OriginalFileName = request.OriginalFileName.Trim(),
            FilePath = request.FilePath.Trim(),
            Extension = NormalizeExtension(request.Extension),
            ContentType = request.ContentType.Trim(),
            Size = request.Size,
            UploadedByUserId = request.UploadedByUserId,
            Module = request.Module.Trim().ToLowerInvariant(),
            ReferenceId = request.ReferenceId,
            IsPublic = request.IsPublic
        };

        await _dbContext.FileAttachments.AddAsync(attachment, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var createdAttachment = await GetByIdAsync(
            attachment.Id,
            cancellationToken
        );

        return createdAttachment!;
    }

    public async Task<FileAttachmentDto?> UpdateAsync(
        Guid id,
        UpdateFileAttachmentRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await ValidateUserAsync(request.UploadedByUserId, cancellationToken);
        ValidateAttachment(request);

        var attachment = await _dbContext.FileAttachments
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (attachment is null)
        {
            return null;
        }

        attachment.FileName = request.FileName.Trim();
        attachment.OriginalFileName = request.OriginalFileName.Trim();
        attachment.FilePath = request.FilePath.Trim();
        attachment.Extension = NormalizeExtension(request.Extension);
        attachment.ContentType = request.ContentType.Trim();
        attachment.Size = request.Size;
        attachment.UploadedByUserId = request.UploadedByUserId;
        attachment.Module = request.Module.Trim().ToLowerInvariant();
        attachment.ReferenceId = request.ReferenceId;
        attachment.IsPublic = request.IsPublic;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(attachment.Id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var attachment = await _dbContext.FileAttachments
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (attachment is null)
        {
            return false;
        }

        _dbContext.FileAttachments.Remove(attachment);

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
            throw new InvalidOperationException("Active uploaded-by user was not found.");
        }
    }

    private static void ValidateAttachment(
        CreateFileAttachmentRequestDto request)
    {
        ValidateAttachmentCore(
            request.FileName,
            request.OriginalFileName,
            request.FilePath,
            request.Extension,
            request.ContentType,
            request.Size,
            request.Module,
            request.ReferenceId
        );
    }

    private static void ValidateAttachment(
        UpdateFileAttachmentRequestDto request)
    {
        ValidateAttachmentCore(
            request.FileName,
            request.OriginalFileName,
            request.FilePath,
            request.Extension,
            request.ContentType,
            request.Size,
            request.Module,
            request.ReferenceId
        );
    }

    private static void ValidateAttachmentCore(
        string fileName,
        string originalFileName,
        string filePath,
        string extension,
        string contentType,
        long size,
        string module,
        Guid referenceId)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new InvalidOperationException("File name is required.");
        }

        if (string.IsNullOrWhiteSpace(originalFileName))
        {
            throw new InvalidOperationException("Original file name is required.");
        }

        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new InvalidOperationException("File path is required.");
        }

        if (string.IsNullOrWhiteSpace(extension))
        {
            throw new InvalidOperationException("File extension is required.");
        }

        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new InvalidOperationException("Content type is required.");
        }

        if (size < 0)
        {
            throw new InvalidOperationException("File size cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(module))
        {
            throw new InvalidOperationException("Module is required.");
        }

        if (referenceId == Guid.Empty)
        {
            throw new InvalidOperationException("Reference id is required.");
        }
    }

    private static string NormalizeExtension(string extension)
    {
        var normalizedExtension = extension.Trim().ToLowerInvariant();

        return normalizedExtension.StartsWith('.')
            ? normalizedExtension
            : $".{normalizedExtension}";
    }
}