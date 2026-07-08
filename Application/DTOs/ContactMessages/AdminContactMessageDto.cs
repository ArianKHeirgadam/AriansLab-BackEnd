namespace Application.DTOs.ContactMessages;

public class AdminContactMessageDto
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string? Company { get; set; }

    public bool IsRead { get; set; }

    public string? AdminReply { get; set; }

    public DateTime? RepliedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}