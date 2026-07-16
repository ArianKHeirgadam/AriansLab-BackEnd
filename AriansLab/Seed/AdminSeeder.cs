using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace AriansLab.Api.Seed;

public static partial class AdminSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("AdminSeeder");

        if (!configuration.GetValue<bool>("AdminSeed:Enabled"))
        {
            logger.LogInformation("Admin seed is disabled.");
            return;
        }

        var fullName = configuration["AdminSeed:FullName"]?.Trim();
        var email = configuration["AdminSeed:Email"]?.Trim().ToLowerInvariant();
        var password = configuration["AdminSeed:Password"];
        var configuredUserName = configuration["AdminSeed:UserName"]?.Trim();

        if (string.IsNullOrWhiteSpace(fullName) || fullName.Length > 150 ||
            string.IsNullOrWhiteSpace(email) || email.Length > 256 || !IsValidEmail(email) ||
            !IsStrongPassword(password))
        {
            throw new InvalidOperationException("Enabled AdminSeed configuration is invalid or insecure.");
        }

        var userName = string.IsNullOrWhiteSpace(configuredUserName)
            ? email.Split('@')[0]
            : configuredUserName;

        if (userName.Length is < 3 or > 100 || !UserNamePattern().IsMatch(userName))
        {
            throw new InvalidOperationException("AdminSeed UserName is invalid.");
        }

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (await dbContext.Users.AnyAsync(user => user.Role == UserRole.Admin))
        {
            logger.LogInformation("Admin seed skipped because an administrator already exists.");
            return;
        }

        var normalizedEmail = email.ToUpperInvariant();
        var normalizedUserName = userName.ToUpperInvariant();
        if (await dbContext.Users.AnyAsync(user =>
                user.NormalizedEmail == normalizedEmail || user.NormalizedUserName == normalizedUserName))
        {
            throw new InvalidOperationException("AdminSeed email or username already belongs to another account.");
        }

        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await dbContext.Users.AddAsync(new User
        {
            FullName = fullName,
            Email = email,
            NormalizedEmail = normalizedEmail,
            UserName = userName,
            NormalizedUserName = normalizedUserName,
            PasswordHash = passwordHasher.HashPassword(password!),
            Role = UserRole.Admin,
            IsActive = true,
            EmailConfirmed = true
        });
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Initial administrator was created successfully.");
    }

    private static bool IsValidEmail(string value)
    {
        try
        {
            return new MailAddress(value).Address.Equals(value, StringComparison.OrdinalIgnoreCase);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static bool IsStrongPassword(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) &&
               value.Length is >= 12 and <= 128 &&
               value.Any(char.IsUpper) &&
               value.Any(char.IsLower) &&
               value.Any(char.IsDigit);
    }

    [GeneratedRegex("^[a-zA-Z0-9._-]+$", RegexOptions.CultureInvariant)]
    private static partial Regex UserNamePattern();
}
