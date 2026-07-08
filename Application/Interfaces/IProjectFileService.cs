using Application.DTOs.Files;

namespace Application.Interfaces;

public interface IProjectFileService
{
    Task<List<ProjectFileDto>> GetMyProjectFilesAsync(
        Guid userId,
        Guid projectId,
        CancellationToken cancellationToken = default);

    Task<ProjectFileDto?> GetMyProjectFileByIdAsync(
        Guid userId,
        Guid fileId,
        CancellationToken cancellationToken = default);
}