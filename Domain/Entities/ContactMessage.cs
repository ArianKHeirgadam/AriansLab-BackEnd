using Domain.Common;

namespace Domain.Entities;

public class ContactMessage : SoftDeleteEntity
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string? Company { get; set; }

    public bool IsRead { get; set; }

    public string? AdminReply { get; set; }

    public DateTime? RepliedAt { get; set; }
}