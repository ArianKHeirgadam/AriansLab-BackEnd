using Application.Security;
using Domain.Entities;
using Domain.Enums;

namespace AriansLab.ApiTests;

public class UserSecurityStampTests
{
    [Fact]
    public void Create_IsStableWhenSqlServerReturnsUnspecifiedDateTimeKind()
    {
        var changedAtUtc = new DateTime(2026, 7, 16, 10, 12, 37, DateTimeKind.Utc);
        var userId = Guid.NewGuid();

        var issuedUser = CreateUser(userId, changedAtUtc);
        var reloadedUser = CreateUser(
            userId,
            DateTime.SpecifyKind(changedAtUtc, DateTimeKind.Unspecified));

        Assert.Equal(
            UserSecurityStamp.Create(issuedUser),
            UserSecurityStamp.Create(reloadedUser));
    }

    private static User CreateUser(Guid id, DateTime updatedAt)
    {
        return new User
        {
            Id = id,
            FullName = "Security Test",
            Email = "security@example.com",
            NormalizedEmail = "SECURITY@EXAMPLE.COM",
            UserName = "security-test",
            NormalizedUserName = "SECURITY-TEST",
            PasswordHash = "stable-password-hash",
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = updatedAt.AddDays(-1),
            UpdatedAt = updatedAt
        };
    }
}
