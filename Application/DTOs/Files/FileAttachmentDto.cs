namespace Application.DTOs.Files;

public class FileAttachmentDto
{
    public Guid Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string OriginalFileName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public string Extension { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long Size { get; set; }

    public Guid UploadedByUserId { get; set; }

    public string UploadedByFullName { get; set; } = string.Empty;

    public string UploadedByEmail { get; set; } = string.Empty;

    public string Module { get; set; } = string.Empty;

    public Guid ReferenceId { get; set; }

    public bool IsPublic { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}