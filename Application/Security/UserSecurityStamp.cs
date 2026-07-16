using Domain.Entities;
using System.Security.Cryptography;
using System.Text;

namespace Application.Security;

public static class UserSecurityStamp
{
    public const string ClaimType = "arianslab:security_stamp";

    public static string Create(User user)
    {
        // SQL Server returns DateTime columns with Kind=Unspecified. The values in
        // this application are stored as UTC, so normalize the kind without
        // applying a local-time conversion that would change the stamp.
        var changedAt = user.UpdatedAt ?? user.CreatedAt;
        var changedAtUtcTicks = DateTime.SpecifyKind(changedAt, DateTimeKind.Utc).Ticks;
        var source = string.Join(
            '|',
            user.Id.ToString("N"),
            user.PasswordHash,
            (int)user.Role,
            user.IsActive,
            changedAtUtcTicks);

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source)));
    }
}
