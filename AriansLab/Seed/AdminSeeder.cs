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

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("AdminSeeder");

        var adminEmail = configuration["AdminSeed:Email"];
        var adminPassword = configuration["AdminSeed:Password"];
        var adminFullName = configuration["AdminSeed:FullName"];

        if (string.IsNullOrWhiteSpace(adminEmail) ||
            string.IsNullOrWhiteSpace(adminPassword) ||
            string.IsNullOrWhiteSpace(adminFullName))
        {
            logger.LogWarning("Admin seed skipped because AdminSeed configuration is incomplete.");
            return;
        }

        adminEmail = adminEmail.Trim().ToLowerInvariant();
        adminFullName = adminFullName.Trim();

        var adminUserName = adminEmail
            .Split('@')[0]
            .Trim()
            .ToLowerInvariant();

        var normalizedEmail = adminEmail.ToUpperInvariant();
        var normalizedUserName = adminUserName.ToUpperInvariant();

        var existingAdmin = await dbContext.Users
            .FirstOrDefaultAsync(x =>
                x.NormalizedEmail == normalizedEmail ||
                x.NormalizedUserName == normalizedUserName ||
                x.Role == UserRole.Admin);

        if (existingAdmin is not null)
        {
            existingAdmin.FullName = adminFullName;
            existingAdmin.Email = adminEmail;
            existingAdmin.NormalizedEmail = normalizedEmail;
            existingAdmin.UserName = adminUserName;
            existingAdmin.NormalizedUserName = normalizedUserName;

            existingAdmin.PasswordHash = passwordHasher.HashPassword(adminPassword);

            existingAdmin.Role = UserRole.Admin;
            existingAdmin.IsActive = true;
            existingAdmin.EmailConfirmed = true;
            existingAdmin.IsDeleted = false;

            await dbContext.SaveChangesAsync();

            logger.LogInformation(
                "Admin user already exists and was updated. Email: {Email}, UserName: {UserName}",
                existingAdmin.Email,
                existingAdmin.UserName
            );

            return;
        }

        var admin = new User
        {
            FullName = adminFullName,
            Email = adminEmail,
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

        logger.LogInformation(
            "Admin user seeded successfully. Email: {Email}, UserName: {UserName}",
            admin.Email,
            admin.UserName
        );
    }
}