using Application.DTOs.Projects;

namespace Application.Interfaces;

public interface IProjectReadService
{
    Task<List<ProjectListItemDto>> GetMyProjectsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<ProjectDetailDto?> GetMyProjectByIdAsync(
        Guid userId,
        Guid projectId,
        CancellationToken cancellationToken = default);

    Task<ProjectDetailDto?> UpdateMyCustomerCommentAsync(
        Guid userId,
        Guid projectId,
        UpdateProjectCustomerCommentRequestDto request,
        CancellationToken cancellationToken = default);
}