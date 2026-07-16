namespace Application.DTOs.Auth;

public class LoginRequestDto
{
    public string EmailOrUserName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}
