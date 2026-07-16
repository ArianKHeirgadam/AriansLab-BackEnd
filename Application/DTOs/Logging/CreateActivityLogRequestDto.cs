namespace Application.DTOs.Logs;

public sealed class CreateActivityLogRequestDto
{
    public Guid UserId { get; set; }
    public string Activity { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
