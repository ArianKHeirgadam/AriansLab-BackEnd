using Domain.Enums;

namespace Application.DTOs.Profile;

public class ProfileDto
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public UserRole Role { get; set; }

    public bool IsActive { get; set; }

    public bool EmailConfirmed { get; set; }

    public string? Avatar { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}