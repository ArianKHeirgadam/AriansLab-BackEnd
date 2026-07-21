using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Infrastructure.Identity;
using Domain.Entities;
using Domain.Enums;
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
            value.Contains("secure", StringComparison.OrdinalIgnoreCase) &&
            value.Contains("samesite=none", StringComparison.OrdinalIgnoreCase) &&
            value.Contains("partitioned", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(registrationCookies, value =>
            value.Contains("__Secure-AriansLab.Refresh=", StringComparison.Ordinal) &&
            value.Contains("httponly", StringComparison.OrdinalIgnoreCase) &&
            value.Contains("partitioned", StringComparison.OrdinalIgnoreCase));

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
    public async Task AnonymousAnalyticsPageView_DoesNotRequireCsrfOrAuthCookie()
    {
        using var client = _factory.CreateSecureClient();
        var visitorId = Guid.NewGuid();
        var response = await client.PostAsJsonAsync("/api/analytics/page-view", new
        {
            path = "/products?campaign=integration-test",
            visitorId,
            sessionId = Guid.NewGuid(),
            referrerHost = "example.com"
        });

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Contains(
            await dbContext.PageViews.ToListAsync(),
            item => item.Path == "/products" && item.VisitorIdHash.Length == 64);
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

    [Fact]
    public async Task CommentSubmission_WithCsrf_IsStoredPendingApproval()
    {
        using var client = _factory.CreateSecureClient();
        var userId = await RegisterAndGetUserIdAsync(client);
        var blogPostId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var category = new BlogCategory
            {
                Id = Guid.NewGuid(),
                Name = "Security",
                Slug = $"security-{Guid.NewGuid():N}"
            };
            dbContext.BlogCategories.Add(category);
            dbContext.BlogPosts.Add(new BlogPost
            {
                Id = blogPostId,
                AuthorId = userId,
                CategoryId = category.Id,
                Title = "Comment integration test",
                Slug = $"comment-integration-{Guid.NewGuid():N}",
                Excerpt = "Integration test",
                Content = "Integration test",
                CoverImage = "/test.jpg",
                ReadTime = 1,
                IsPublished = true,
                PublishedAt = DateTime.UtcNow
            });
            await dbContext.SaveChangesAsync();
        }

        await AddCsrfHeaderAsync(client);
        var response = await client.PostAsJsonAsync("/api/comments", new
        {
            blogPostId,
            fullName = "Comment Tester",
            email = "commenter@example.com",
            message = "This comment should wait for approval."
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Guid createdCommentId;
        using (var responseDocument = JsonDocument.Parse(await response.Content.ReadAsStringAsync()))
        {
            var publicComment = responseDocument.RootElement.GetProperty("data");
            createdCommentId = publicComment.GetProperty("id").GetGuid();
            Assert.Equal(blogPostId, publicComment.GetProperty("blogPostId").GetGuid());
            Assert.False(publicComment.TryGetProperty("email", out _));
            Assert.False(publicComment.TryGetProperty("userEmail", out _));
            Assert.False(publicComment.TryGetProperty("userId", out _));
        }

        using var verificationScope = _factory.Services.CreateScope();
        var verificationDb = verificationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var storedComment = await verificationDb.Comments.SingleAsync(item =>
            item.BlogPostId == blogPostId && item.Email == "commenter@example.com");
        Assert.False(storedComment.IsApproved);
        Assert.Equal(userId, storedComment.UserId);

        using var adminClient = await CreateAdminClientAsync();
        var adminListResponse = await adminClient.GetAsync(
            "/api/admin/comments?isApproved=false&skip=0&take=500");
        adminListResponse.EnsureSuccessStatusCode();
        Assert.Equal("no-store", adminListResponse.Headers.CacheControl?.ToString());

        using var adminListDocument = JsonDocument.Parse(
            await adminListResponse.Content.ReadAsStringAsync());
        var pendingComments = adminListDocument.RootElement.GetProperty("data");
        Assert.Contains(
            pendingComments.EnumerateArray(),
            item => item.GetProperty("id").GetGuid() == createdCommentId &&
                    !item.GetProperty("isApproved").GetBoolean());
    }

    private async Task<HttpClient> CreateAdminClientAsync()
    {
        const string password = "AdminTestPassword123!";
        var suffix = Guid.NewGuid().ToString("N")[..12];
        var email = $"admin-{suffix}@example.com";

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Users.Add(new User
            {
                Id = Guid.NewGuid(),
                FullName = "Admin Test User",
                Email = email,
                NormalizedEmail = email.ToUpperInvariant(),
                UserName = $"admin_{suffix}",
                NormalizedUserName = $"ADMIN_{suffix.ToUpperInvariant()}",
                PasswordHash = new PasswordHasher().HashPassword(password),
                Role = UserRole.Admin,
                IsActive = true,
                EmailConfirmed = true
            });
            await dbContext.SaveChangesAsync();
        }

        var adminClient = _factory.CreateSecureClient();
        await AddCsrfHeaderAsync(adminClient);
        var loginResponse = await adminClient.PostAsJsonAsync("/api/Auth/login", new
        {
            emailOrUserName = email,
            password,
            rememberMe = false
        });
        loginResponse.EnsureSuccessStatusCode();
        return adminClient;
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
        Assert.Contains(GetSetCookieHeaders(response), value =>
            value.Contains("__Host-AriansLab.Csrf=", StringComparison.Ordinal) &&
            value.Contains("samesite=none", StringComparison.OrdinalIgnoreCase) &&
            value.Contains("partitioned", StringComparison.OrdinalIgnoreCase));
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
