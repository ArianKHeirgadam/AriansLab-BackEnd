using Application.DTOs.Profile;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class ProfileService : IProfileService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    public ProfileService(
        ApplicationDbContext dbContext,
        IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<ProfileDto?> GetMeAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => new ProfileDto
            {
                Id = x.Id,
                FullName = x.FullName,
                Email = x.Email,
                UserName = x.UserName,
                PhoneNumber = x.PhoneNumber,
                Role = x.Role,
                IsActive = x.IsActive,
                EmailConfirmed = x.EmailConfirmed,
                Avatar = x.Avatar,
                LastLoginAt = x.LastLoginAt,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ProfileDto?> UpdateMeAsync(
        Guid userId,
        UpdateProfileRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateUpdateRequest(request);

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        if (!user.IsActive)
        {
            throw new InvalidOperationException("User account is not active.");
        }

        var email = request.Email.Trim().ToLowerInvariant();
        var userName = NormalizeUserName(request.UserName, email);

        var emailExists = await _dbContext.Users
            .AnyAsync(
                x => x.Id != userId &&
                     x.NormalizedEmail == email.ToUpperInvariant(),
                cancellationToken
            );

        if (emailExists)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var userNameExists = await _dbContext.Users
            .AnyAsync(
                x => x.Id != userId &&
                     x.NormalizedUserName == userName.ToUpperInvariant(),
                cancellationToken
            );

        if (userNameExists)
        {
            throw new InvalidOperationException("A user with this username already exists.");
        }

        user.FullName = request.FullName.Trim();
        user.Email = email;
        user.NormalizedEmail = email.ToUpperInvariant();
        user.UserName = userName;
        user.NormalizedUserName = userName.ToUpperInvariant();
        user.PhoneNumber = request.PhoneNumber?.Trim();
        user.Avatar = request.Avatar?.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetMeAsync(user.Id, cancellationToken);
    }

    public async Task<bool> ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateChangePasswordRequest(request);

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user is null)
        {
            return false;
        }

        if (!user.IsActive)
        {
            throw new InvalidOperationException("User account is not active.");
        }

        var currentPasswordIsValid = _passwordHasher.VerifyPassword(
            request.CurrentPassword,
            user.PasswordHash
        );

        if (!currentPasswordIsValid)
        {
            throw new InvalidOperationException("Current password is incorrect.");
        }

        user.PasswordHash = _passwordHasher.HashPassword(
            request.NewPassword
        );

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static void ValidateUpdateRequest(UpdateProfileRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            throw new InvalidOperationException("Full name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new InvalidOperationException("Email is required.");
        }

        if (!request.Email.Contains('@'))
        {
            throw new InvalidOperationException("Email format is invalid.");
        }
    }

    private static void ValidateChangePasswordRequest(
        ChangePasswordRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword))
        {
            throw new InvalidOperationException("Current password is required.");
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword))
        {
            throw new InvalidOperationException("New password is required.");
        }

        if (request.NewPassword.Length < 8)
        {
            throw new InvalidOperationException("New password must be at least 8 characters.");
        }

        if (request.CurrentPassword == request.NewPassword)
        {
            throw new InvalidOperationException("New password must be different from current password.");
        }
    }

    private static string NormalizeUserName(string? userName, string email)
    {
        if (!string.IsNullOrWhiteSpace(userName))
        {
            return userName.Trim().ToLowerInvariant();
        }

        return email.Split('@')[0].Trim().ToLowerInvariant();
    }
}