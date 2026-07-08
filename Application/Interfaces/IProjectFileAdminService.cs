using Application.DTOs.Files;

namespace Application.Interfaces;

public interface IProjectFileAdminService
{
    Task<List<ProjectFileDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<List<ProjectFileDto>> GetByProjectIdAsync(
        Guid projectId,
        CancellationToken cancellationToken = default);

    Task<ProjectFileDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ProjectFileDto> CreateAsync(
        CreateProjectFileRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ProjectFileDto?> UpdateAsync(
        Guid id,
        UpdateProjectFileRequestDto request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}