using Domain.Enums;

namespace Application.DTOs.Users;

public class UpdateUserRequestDto
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? UserName { get; set; }

    public string? PhoneNumber { get; set; }

    public UserRole Role { get; set; } = UserRole.Customer;

    public bool IsActive { get; set; } = true;

    public bool EmailConfirmed { get; set; } = true;

    public string? Avatar { get; set; }
}