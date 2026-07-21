using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Persistence.Context;

namespace AriansLab.ApiTests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"AriansLabTests-{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseContentRoot(Path.Combine(FindRepositoryRoot(), "AriansLab"));
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:DefaultConnection", "Server=(localdb);Database=AriansLabTests");
        builder.UseSetting("Jwt:Issuer", "AriansLab.Tests");
        builder.UseSetting("Jwt:Audience", "AriansLab.Tests.Client");
        builder.UseSetting(
            "Jwt:SecretKey",
            "TEST_ONLY_RANDOM_SECRET_0123456789_ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz");
        builder.UseSetting("Jwt:AccessTokenExpirationMinutes", "15");
        builder.UseSetting("Jwt:RefreshTokenExpirationDays", "7");
        builder.UseSetting("AuthCookies:Secure", "true");
        builder.UseSetting("AuthCookies:SameSite", "None");
        builder.UseSetting("AuthCookies:Partitioned", "true");
        builder.UseSetting("Cors:AllowedOrigins:0", "https://localhost");
        builder.UseSetting("AllowedHosts", "*");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=(localdb);Database=AriansLabTests",
                ["Jwt:Issuer"] = "AriansLab.Tests",
                ["Jwt:Audience"] = "AriansLab.Tests.Client",
                ["Jwt:SecretKey"] = "TEST_ONLY_RANDOM_SECRET_0123456789_ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz",
                ["Jwt:AccessTokenExpirationMinutes"] = "15",
                ["Jwt:RefreshTokenExpirationDays"] = "7",
                ["AuthCookies:Secure"] = "true",
                ["AuthCookies:SameSite"] = "None",
                ["AuthCookies:Partitioned"] = "true",
                ["Cors:AllowedOrigins:0"] = "https://localhost",
                ["AllowedHosts"] = "*",
                ["AdminSeed:Enabled"] = "false"
            });
        });
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<ApplicationDbContext>();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }

    public HttpClient CreateSecureClient()
    {
        return CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
            HandleCookies = true
        });
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AriansLab.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate AriansLab.sln.");
    }
}
