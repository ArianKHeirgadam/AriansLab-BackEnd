using Application.DTOs.Files;

namespace Application.Interfaces;

public interface IFileAttachmentService
{
    Task<List<FileAttachmentDto>> GetMyAttachmentsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<List<PublicFileAttachmentDto>> GetPublicByReferenceAsync(
        string module,
        Guid referenceId,
        CancellationToken cancellationToken = default);

    Task<FileAttachmentDto?> GetMyAttachmentByIdAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteMyAttachmentAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken = default);
}
