using Application.DTOs.Projects;

namespace Application.Interfaces;

public interface IProjectAdminService
{
    Task<List<ProjectDetailDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<ProjectDetailDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ProjectDetailDto> CreateAsync(
        CreateProjectRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ProjectDetailDto?> UpdateAsync(
        Guid id,
        UpdateProjectRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ProjectDetailDto?> UpdateStatusAsync(
        Guid id,
        UpdateProjectStatusRequestDto request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}