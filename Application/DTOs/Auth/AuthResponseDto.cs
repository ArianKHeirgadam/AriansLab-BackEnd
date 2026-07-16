using System.Text.Json.Serialization;

namespace Application.DTOs.Auth;

public class AuthResponseDto
{
    [JsonIgnore]
    public string AccessToken { get; set; } = string.Empty;

    [JsonIgnore]
    public string RefreshToken { get; set; } = string.Empty;

    public DateTime AccessTokenExpiresAt { get; set; }

    public DateTime RefreshTokenExpiresAt { get; set; }

    public UserDto User { get; set; } = new();
}
