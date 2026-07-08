namespace Application.DTOs.Files;

public class CreateProjectFileRequestDto
{
    public Guid ProjectId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public string ContentType { get; set; } = string.Empty;
}