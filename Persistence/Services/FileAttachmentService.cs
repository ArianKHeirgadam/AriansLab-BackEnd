using Application.DTOs.Files;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class FileAttachmentService : IFileAttachmentService
{
    private readonly ApplicationDbContext _dbContext;

    public FileAttachmentService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<FileAttachmentDto>> GetMyAttachmentsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.FileAttachments
            .AsNoTracking()
            .Where(x => x.UploadedByUserId == userId)
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

    public async Task<List<PublicFileAttachmentDto>> GetPublicByReferenceAsync(
        string module,
        Guid referenceId,
        CancellationToken cancellationToken = default)
    {
        var normalizedModule = module.Trim().ToLowerInvariant();

        return await _dbContext.FileAttachments
            .AsNoTracking()
            .Where(x =>
                x.Module.ToLower() == normalizedModule &&
                x.ReferenceId == referenceId &&
                x.IsPublic)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new PublicFileAttachmentDto
            {
                Id = x.Id,
                FileName = x.FileName,
                OriginalFileName = x.OriginalFileName,
                FilePath = x.FilePath,
                Extension = x.Extension,
                ContentType = x.ContentType,
                Size = x.Size,
                Module = x.Module,
                ReferenceId = x.ReferenceId,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<FileAttachmentDto?> GetMyAttachmentByIdAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.FileAttachments
            .AsNoTracking()
            .Where(x => x.Id == id && x.UploadedByUserId == userId)
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

    public async Task<bool> DeleteMyAttachmentAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var attachment = await _dbContext.FileAttachments
            .FirstOrDefaultAsync(
                x => x.Id == id && x.UploadedByUserId == userId,
                cancellationToken
            );

        if (attachment is null)
        {
            return false;
        }

        _dbContext.FileAttachments.Remove(attachment);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
