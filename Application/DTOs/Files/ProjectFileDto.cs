namespace Application.DTOs.Files;

public class ProjectFileDto
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }

    public string ProjectTitle { get; set; } = string.Empty;

    public string ProjectCode { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public string ContentType { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}