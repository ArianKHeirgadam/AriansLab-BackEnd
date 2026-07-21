using Domain.Common;

namespace Domain.Entities;

public sealed class PageView : AuditableEntity
{
    public string Path { get; set; } = string.Empty;

    public string VisitorIdHash { get; set; } = string.Empty;

    public string SessionIdHash { get; set; } = string.Empty;

    public string? ReferrerHost { get; set; }

    public string DeviceType { get; set; } = string.Empty;

    public string Browser { get; set; } = string.Empty;
}
