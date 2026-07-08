using Application.DTOs.Files;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class ProjectFileService : IProjectFileService
{
    private readonly ApplicationDbContext _dbContext;

    public ProjectFileService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ProjectFileDto>> GetMyProjectFilesAsync(
        Guid userId,
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var projectBelongsToUser = await _dbContext.Projects
            .AnyAsync(
                x => x.Id == projectId && x.UserId == userId,
                cancellationToken
            );

        if (!projectBelongsToUser)
        {
            return new List<ProjectFileDto>();
        }

        return await _dbContext.ProjectFiles
            .AsNoTracking()
            .Where(x => x.ProjectId == projectId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ProjectFileDto
            {
                Id = x.Id,
                ProjectId = x.ProjectId,
                ProjectTitle = x.Project.Title,
                ProjectCode = x.Project.ProjectCode,
                FileName = x.FileName,
                FilePath = x.FilePath,
                FileSize = x.FileSize,
                ContentType = x.ContentType,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ProjectFileDto?> GetMyProjectFileByIdAsync(
        Guid userId,
        Guid fileId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProjectFiles
            .AsNoTracking()
            .Where(x => x.Id == fileId && x.Project.UserId == userId)
            .Select(x => new ProjectFileDto
            {
                Id = x.Id,
                ProjectId = x.ProjectId,
                ProjectTitle = x.Project.Title,
                ProjectCode = x.Project.ProjectCode,
                FileName = x.FileName,
                FilePath = x.FilePath,
                FileSize = x.FileSize,
                ContentType = x.ContentType,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}