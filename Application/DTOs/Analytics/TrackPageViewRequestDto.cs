using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Analytics;

public sealed class TrackPageViewRequestDto
{
    [Required]
    [StringLength(300)]
    public string Path { get; set; } = string.Empty;

    public Guid VisitorId { get; set; }

    public Guid SessionId { get; set; }

    [StringLength(253)]
    public string? ReferrerHost { get; set; }
}
