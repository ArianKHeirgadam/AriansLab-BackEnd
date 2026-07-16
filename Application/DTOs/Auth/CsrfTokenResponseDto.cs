namespace Application.DTOs.Auth;

public sealed class CsrfTokenResponseDto
{
    public string Token { get; init; } = string.Empty;
}
