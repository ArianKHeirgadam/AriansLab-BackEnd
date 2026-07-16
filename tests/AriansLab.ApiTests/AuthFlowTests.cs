using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Context;

namespace AriansLab.ApiTests;

public class AuthFlowTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public AuthFlowTests(ApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_WithoutCsrfToken_IsRejected()
    {
        using var client = _factory.CreateSecureClient();
        var response = await client.PostAsJsonAsync("/api/Auth/register", CreateRegistration());
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CookieAuth_RegisterRefreshLogout_CompletesEndToEnd()
    {
        using var client = _factory.CreateSecureClient();
        await AddCsrfHeaderAsync(client);

        var registerResponse = await client.PostAsJsonAsync("/api/Auth/register", CreateRegistration());
        registerResponse.EnsureSuccessStatusCode();

        using (var document = JsonDocument.Parse(await registerResponse.Content.ReadAsStringAsync()))
        {
            var data = document.RootElement.GetProperty("data");
            Assert.False(data.TryGetProperty("accessToken", out _));
            Assert.False(data.TryGetProperty("refreshToken", out _));
        }

        var registrationCookies = GetSetCookieHeaders(registerResponse);
        Assert.Contains(registrationCookies, value =>
            value.Contains("__Host-AriansLab.Access=", StringComparison.Ordinal) &&
            value.Contains("httponly", StringComparison.OrdinalIgnoreCase) &&
            value.Contains("secure", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(registrationCookies, value =>
            value.Contains("__Secure-AriansLab.Refresh=", StringComparison.Ordinal) &&
            value.Contains("httponly", StringComparison.OrdinalIgnoreCase));

        (await client.GetAsync("/api/Auth/me")).EnsureSuccessStatusCode();

        await AddCsrfHeaderAsync(client);
        var refreshResponse = await client.PostAsync(
            "/api/Auth/refresh-token",
            JsonContent.Create(new { }));
        Assert.True(
            refreshResponse.IsSuccessStatusCode,
            $"Refresh failed with {(int)refreshResponse.StatusCode}: {await refreshResponse.Content.ReadAsStringAsync()}");
        Assert.Contains(GetSetCookieHeaders(refreshResponse), value =>
            value.Contains("__Secure-AriansLab.Refresh=", StringComparison.Ordinal));

        await AddCsrfHeaderAsync(client);
        var logoutResponse = await client.PostAsync("/api/Auth/logout", JsonContent.Create(new { }));
        logoutResponse.EnsureSuccessStatusCode();

        var meAfterLogoutResponse = await client.GetAsync("/api/Auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, meAfterLogoutResponse.StatusCode);
    }

    [Fact]
    public async Task CustomerCookie_CannotAccessAdminEndpoints()
    {
        using var client = _factory.CreateSecureClient();
        await AddCsrfHeaderAsync(client);
        (await client.PostAsJsonAsync("/api/Auth/register", CreateRegistration())).EnsureSuccessStatusCode();

        var response = await client.GetAsync("/api/admin/users");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task FallbackPolicy_RequiresAuthentication_AndPublicRoutesAreExplicit()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var policyProvider = scope.ServiceProvider.GetRequiredService<IAuthorizationPolicyProvider>();
            var fallbackPolicy = await policyProvider.GetFallbackPolicyAsync();
            Assert.NotNull(fallbackPolicy);
            Assert.Contains(
                fallbackPolicy.Requirements,
                requirement => requirement is DenyAnonymousAuthorizationRequirement);
        }

        using var client = _factory.CreateSecureClient();
        (await client.GetAsync("/health")).EnsureSuccessStatusCode();
        (await client.GetAsync("/api/technologies")).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/Auth/me")).StatusCode);
    }

    [Fact]
    public async Task DeactivatedUserCookie_IsImmediatelyRejected()
    {
        using var client = _factory.CreateSecureClient();
        var userId = await RegisterAndGetUserIdAsync(client);

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await dbContext.Users.SingleAsync(item => item.Id == userId);
            user.IsActive = false;
            await dbContext.SaveChangesAsync();
        }

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/Auth/me")).StatusCode);
    }

    [Fact]
    public async Task PasswordChangeInvalidatesExistingAccessCookie()
    {
        using var client = _factory.CreateSecureClient();
        var userId = await RegisterAndGetUserIdAsync(client);

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await dbContext.Users.SingleAsync(item => item.Id == userId);
            user.PasswordHash = new PasswordHasher().HashPassword("AnotherSafePassword456!");
            await dbContext.SaveChangesAsync();
        }

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/Auth/me")).StatusCode);
    }

    [Fact]
    public async Task ReplayedRefreshTokenRevokesTheRotatedSession()
    {
        using var client = _factory.CreateSecureClient();
        await AddCsrfHeaderAsync(client);
        var registerResponse = await client.PostAsJsonAsync("/api/Auth/register", CreateRegistration());
        registerResponse.EnsureSuccessStatusCode();

        using var registerDocument = JsonDocument.Parse(await registerResponse.Content.ReadAsStringAsync());
        var userId = registerDocument.RootElement.GetProperty("data").GetProperty("user").GetProperty("id").GetGuid();
        var originalRefreshCookie = GetCookiePair(registerResponse, "__Secure-AriansLab.Refresh");

        await AddCsrfHeaderAsync(client);
        (await client.PostAsync("/api/Auth/refresh-token", JsonContent.Create(new { }))).EnsureSuccessStatusCode();

        using var replayClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
            HandleCookies = false
        });
        var csrfResponse = await replayClient.GetAsync("/api/Auth/csrf-token");
        csrfResponse.EnsureSuccessStatusCode();
        var csrfCookie = GetCookiePair(csrfResponse, "__Host-AriansLab.Csrf");
        using var csrfDocument = JsonDocument.Parse(await csrfResponse.Content.ReadAsStringAsync());
        var csrfToken = csrfDocument.RootElement.GetProperty("data").GetProperty("token").GetString();
        Assert.False(string.IsNullOrWhiteSpace(csrfToken));

        replayClient.DefaultRequestHeaders.Add("Cookie", $"{csrfCookie}; {originalRefreshCookie}");
        replayClient.DefaultRequestHeaders.Add("X-CSRF-TOKEN", csrfToken!);
        var replayResponse = await replayClient.PostAsync(
            "/api/Auth/refresh-token",
            JsonContent.Create(new { }));
        Assert.Equal(HttpStatusCode.Unauthorized, replayResponse.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.False(await dbContext.RefreshTokens.AnyAsync(
            token => token.UserId == userId && !token.IsRevoked));
    }

    private async Task<Guid> RegisterAndGetUserIdAsync(HttpClient client)
    {
        await AddCsrfHeaderAsync(client);
        var response = await client.PostAsJsonAsync("/api/Auth/register", CreateRegistration());
        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("data").GetProperty("user").GetProperty("id").GetGuid();
    }

    private static object CreateRegistration()
    {
        var suffix = Guid.NewGuid().ToString("N")[..12];
        return new
        {
            fullName = "Test User",
            email = $"user-{suffix}@example.com",
            userName = $"user_{suffix}",
            password = "SafePassword123!"
        };
    }

    private static async Task AddCsrfHeaderAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/Auth/csrf-token");
        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var token = document.RootElement.GetProperty("data").GetProperty("token").GetString();
        Assert.False(string.IsNullOrWhiteSpace(token));
        client.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", token);
    }

    private static string[] GetSetCookieHeaders(HttpResponseMessage response) =>
        response.Headers.TryGetValues("Set-Cookie", out var values)
            ? values.ToArray()
            : Array.Empty<string>();

    private static string GetCookiePair(HttpResponseMessage response, string cookieName)
    {
        var prefix = $"{cookieName}=";
        var header = GetSetCookieHeaders(response).First(value =>
            value.StartsWith(prefix, StringComparison.Ordinal));
        return header.Split(';', 2)[0];
    }
}
