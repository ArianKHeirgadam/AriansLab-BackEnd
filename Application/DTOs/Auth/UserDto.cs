using Domain.Enums;

namespace Application.DTOs.Auth;

public class UserDto
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public string? Avatar { get; set; }

    public bool IsActive { get; set; }

    public bool EmailConfirmed { get; set; }
}