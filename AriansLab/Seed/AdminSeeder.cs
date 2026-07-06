using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace AriansLab.Api.Seed;

public static class AdminSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("AdminSeeder");

        var adminEmail = configuration["AdminSeed:Email"];
        var adminPassword = configuration["AdminSeed:Password"];
        var adminFullName = configuration["AdminSeed:FullName"];

        if (string.IsNullOrWhiteSpace(adminEmail) ||
            string.IsNullOrWhiteSpace(adminPassword) ||
            string.IsNullOrWhiteSpace(adminFullName))
        {
            logger.LogWarning("Admin seed settings are incomplete. Admin user was not seeded.");
            return;
        }

        var normalizedEmail = adminEmail.Trim().ToUpperInvariant();

        var adminExists = await dbContext.Users
            .AnyAsync(x => x.NormalizedEmail == normalizedEmail);

        if (adminExists)
        {
            return;
        }

        var adminUserName = adminEmail.Split('@')[0];
        var normalizedUserName = adminUserName.Trim().ToUpperInvariant();

        var admin = new User
        {
            Id = Guid.NewGuid(),
            FullName = adminFullName.Trim(),
            Email = adminEmail.Trim(),
            NormalizedEmail = normalizedEmail,
            UserName = adminUserName,
            NormalizedUserName = normalizedUserName,
            PasswordHash = passwordHasher.HashPassword(adminPassword),
            Role = UserRole.Admin,
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await dbContext.Users.AddAsync(admin);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Admin user seeded successfully. Email: {Email}", admin.Email);
    }
}