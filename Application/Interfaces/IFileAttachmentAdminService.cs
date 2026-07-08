using Application.DTOs.Files;

namespace Application.Interfaces;

public interface IFileAttachmentAdminService
{
    Task<List<FileAttachmentDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<List<FileAttachmentDto>> GetByReferenceAsync(
        string module,
        Guid referenceId,
        CancellationToken cancellationToken = default);

    Task<FileAttachmentDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<FileAttachmentDto> CreateAsync(
        CreateFileAttachmentRequestDto request,
        CancellationToken cancellationToken = default);

    Task<FileAttachmentDto?> UpdateAsync(
        Guid id,
        UpdateFileAttachmentRequestDto request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}