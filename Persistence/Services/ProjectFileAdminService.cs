using Application.DTOs.Files;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class ProjectFileAdminService : IProjectFileAdminService
{
    private readonly ApplicationDbContext _dbContext;

    public ProjectFileAdminService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ProjectFileDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProjectFiles
            .AsNoTracking()
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

    public async Task<List<ProjectFileDto>> GetByProjectIdAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
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

    public async Task<ProjectFileDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProjectFiles
            .AsNoTracking()
            .Where(x => x.Id == id)
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

    public async Task<ProjectFileDto> CreateAsync(
        CreateProjectFileRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await ValidateProjectAsync(request.ProjectId, cancellationToken);
        ValidateProjectFile(request.FileName, request.FilePath, request.FileSize, request.ContentType);

        var projectFile = new ProjectFile
        {
            ProjectId = request.ProjectId,
            FileName = request.FileName.Trim(),
            FilePath = request.FilePath.Trim(),
            FileSize = request.FileSize,
            ContentType = request.ContentType.Trim()
        };

        await _dbContext.ProjectFiles.AddAsync(projectFile, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var createdFile = await GetByIdAsync(projectFile.Id, cancellationToken);

        return createdFile!;
    }

    public async Task<ProjectFileDto?> UpdateAsync(
        Guid id,
        UpdateProjectFileRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await ValidateProjectAsync(request.ProjectId, cancellationToken);
        ValidateProjectFile(request.FileName, request.FilePath, request.FileSize, request.ContentType);

        var projectFile = await _dbContext.ProjectFiles
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (projectFile is null)
        {
            return null;
        }

        projectFile.ProjectId = request.ProjectId;
        projectFile.FileName = request.FileName.Trim();
        projectFile.FilePath = request.FilePath.Trim();
        projectFile.FileSize = request.FileSize;
        projectFile.ContentType = request.ContentType.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(projectFile.Id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var projectFile = await _dbContext.ProjectFiles
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (projectFile is null)
        {
            return false;
        }

        _dbContext.ProjectFiles.Remove(projectFile);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task ValidateProjectAsync(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var projectExists = await _dbContext.Projects
            .AnyAsync(x => x.Id == projectId, cancellationToken);

        if (!projectExists)
        {
            throw new InvalidOperationException("Project was not found.");
        }
    }

    private static void ValidateProjectFile(
        string fileName,
        string filePath,
        long fileSize,
        string contentType)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new InvalidOperationException("File name is required.");
        }

        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new InvalidOperationException("File path is required.");
        }

        if (fileSize < 0)
        {
            throw new InvalidOperationException("File size cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new InvalidOperationException("Content type is required.");
        }
    }
}