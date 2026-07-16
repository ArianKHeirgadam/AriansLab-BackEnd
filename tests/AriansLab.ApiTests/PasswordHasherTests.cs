using Infrastructure.Identity;

namespace AriansLab.ApiTests;

public class PasswordHasherTests
{
    [Fact]
    public void HashPassword_UsesRandomSaltAndVerifiesCorrectly()
    {
        var hasher = new PasswordHasher();
        const string password = "SafePassword123!";

        var first = hasher.HashPassword(password);
        var second = hasher.HashPassword(password);

        Assert.NotEqual(first, second);
        Assert.True(hasher.VerifyPassword(password, first));
        Assert.False(hasher.VerifyPassword("WrongPassword123!", first));
        Assert.False(hasher.NeedsRehash(first));
    }

    [Fact]
    public void LegacyHash_IsAcceptedAndMarkedForUpgrade()
    {
        var hasher = new PasswordHasher();
        const string legacyHash =
            "100000.QXJpYW5zTGFiRHVtbXkxIQ==.YmSEOh0YBvsPBokR58bZyLRfwj2us1mtU3GFLIhP44A=";

        Assert.True(hasher.VerifyPassword("DummyLoginPassword123!", legacyHash));
        Assert.True(hasher.NeedsRehash(legacyHash));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-password-hash")]
    [InlineData("999999999.AA==.AA==")]
    public void VerifyPassword_RejectsMalformedOrUnsafeHashes(string passwordHash)
    {
        var hasher = new PasswordHasher();
        Assert.False(hasher.VerifyPassword("SafePassword123!", passwordHash));
    }
}
