namespace AriansLab.ApiTests;

public static class DirectTestRunner
{
    public static async Task<int> Main()
    {
        var failures = new List<string>();
        var passwordTests = new PasswordHasherTests();

        await RunAsync(
            nameof(PasswordHasherTests.HashPassword_UsesRandomSaltAndVerifiesCorrectly),
            () => RunSync(passwordTests.HashPassword_UsesRandomSaltAndVerifiesCorrectly),
            failures);
        await RunAsync(
            nameof(PasswordHasherTests.LegacyHash_IsAcceptedAndMarkedForUpgrade),
            () => RunSync(passwordTests.LegacyHash_IsAcceptedAndMarkedForUpgrade),
            failures);

        foreach (var malformedHash in new[] { "", "not-a-password-hash", "999999999.AA==.AA==" })
        {
            await RunAsync(
                $"{nameof(PasswordHasherTests.VerifyPassword_RejectsMalformedOrUnsafeHashes)}({malformedHash})",
                () => RunSync(() => passwordTests.VerifyPassword_RejectsMalformedOrUnsafeHashes(malformedHash)),
                failures);
        }

        var securityStampTests = new UserSecurityStampTests();
        await RunAsync(
            nameof(UserSecurityStampTests.Create_IsStableWhenSqlServerReturnsUnspecifiedDateTimeKind),
            () => RunSync(securityStampTests.Create_IsStableWhenSqlServerReturnsUnspecifiedDateTimeKind),
            failures);

        using var factory = new ApiFactory();
        var authTests = new AuthFlowTests(factory);
        await RunAsync(nameof(AuthFlowTests.Register_WithoutCsrfToken_IsRejected), authTests.Register_WithoutCsrfToken_IsRejected, failures);
        await RunAsync(nameof(AuthFlowTests.CookieAuth_RegisterRefreshLogout_CompletesEndToEnd), authTests.CookieAuth_RegisterRefreshLogout_CompletesEndToEnd, failures);
        await RunAsync(nameof(AuthFlowTests.CustomerCookie_CannotAccessAdminEndpoints), authTests.CustomerCookie_CannotAccessAdminEndpoints, failures);
        await RunAsync(nameof(AuthFlowTests.FallbackPolicy_RequiresAuthentication_AndPublicRoutesAreExplicit), authTests.FallbackPolicy_RequiresAuthentication_AndPublicRoutesAreExplicit, failures);
        await RunAsync(nameof(AuthFlowTests.DeactivatedUserCookie_IsImmediatelyRejected), authTests.DeactivatedUserCookie_IsImmediatelyRejected, failures);
        await RunAsync(nameof(AuthFlowTests.PasswordChangeInvalidatesExistingAccessCookie), authTests.PasswordChangeInvalidatesExistingAccessCookie, failures);
        await RunAsync(nameof(AuthFlowTests.ReplayedRefreshTokenRevokesTheRotatedSession), authTests.ReplayedRefreshTokenRevokesTheRotatedSession, failures);

        Console.WriteLine();
        Console.WriteLine(failures.Count == 0
            ? "All security and authentication tests passed."
            : $"{failures.Count} test(s) failed.");
        return failures.Count == 0 ? 0 : 1;
    }

    private static Task RunSync(Action action)
    {
        action();
        return Task.CompletedTask;
    }

    private static async Task RunAsync(
        string name,
        Func<Task> test,
        ICollection<string> failures)
    {
        try
        {
            await test();
            Console.WriteLine($"PASS {name}");
        }
        catch (Exception exception)
        {
            failures.Add(name);
            Console.Error.WriteLine($"FAIL {name}: {exception}");
        }
    }
}
