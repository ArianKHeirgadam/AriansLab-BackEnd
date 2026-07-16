using Domain.Entities;
using System.Security.Cryptography;
using System.Text;

namespace Application.Security;

public static class UserSecurityStamp
{
    public const string ClaimType = "arianslab:security_stamp";

    public static string Create(User user)
    {
        var changedAt = (user.UpdatedAt ?? user.CreatedAt).ToUniversalTime().Ticks;
        var source = string.Join(
            '|',
            user.Id.ToString("N"),
            user.PasswordHash,
            (int)user.Role,
            user.IsActive,
            changedAt);

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source)));
    }
}
