namespace Application.DTOs.Profile;

public class UpdateProfileRequestDto
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? UserName { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Avatar { get; set; }
}