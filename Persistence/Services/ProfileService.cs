using Application.DTOs.Profile;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Persistence.Services;

public partial class ProfileService : IProfileService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    public ProfileService(ApplicationDbContext dbContext, IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public Task<ProfileDto?> GetMeAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users
            .AsNoTracking()
            .Where(user => user.Id == userId && user.IsActive)
            .Select(user => new ProfileDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                Avatar = user.Avatar,
                LastLoginAt = user.LastLoginAt,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ProfileDto?> UpdateMeAsync(
        Guid userId,
        UpdateProfileRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateUpdateRequest(request);
        var user = await _dbContext.Users.FirstOrDefaultAsync(
            item => item.Id == userId,
            cancellationToken);

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
        var normalizedEmail = email.ToUpperInvariant();
        var normalizedUserName = userName.ToUpperInvariant();

        if (await _dbContext.Users.AnyAsync(
                item => item.Id != userId && item.NormalizedEmail == normalizedEmail,
                cancellationToken))
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        if (await _dbContext.Users.AnyAsync(
                item => item.Id != userId && item.NormalizedUserName == normalizedUserName,
                cancellationToken))
        {
            throw new InvalidOperationException("A user with this username already exists.");
        }

        user.FullName = request.FullName.Trim();
        user.Email = email;
        user.NormalizedEmail = normalizedEmail;
        user.UserName = userName;
        user.NormalizedUserName = normalizedUserName;
        user.PhoneNumber = NormalizeNullable(request.PhoneNumber);
        user.Avatar = NormalizeNullable(request.Avatar);

        await RevokeActiveRefreshTokensAsync(userId, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetMeAsync(user.Id, cancellationToken);
    }

    public async Task<bool> ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateChangePasswordRequest(request);
        var user = await _dbContext.Users.FirstOrDefaultAsync(
            item => item.Id == userId,
            cancellationToken);

        if (user is null)
        {
            return false;
        }

        if (!user.IsActive)
        {
            throw new InvalidOperationException("User account is not active.");
        }

        if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            throw new InvalidOperationException("Current password is incorrect.");
        }

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        await RevokeActiveRefreshTokensAsync(userId, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task RevokeActiveRefreshTokensAsync(Guid userId, CancellationToken cancellationToken)
    {
        var tokens = await _dbContext.RefreshTokens
            .Where(token => token.UserId == userId && !token.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
        }
    }

    private static void ValidateUpdateRequest(UpdateProfileRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) || request.FullName.Trim().Length > 150)
        {
            throw new InvalidOperationException("Full name is required and cannot exceed 150 characters.");
        }

        ValidateEmail(request.Email);
        if (!string.IsNullOrWhiteSpace(request.UserName) &&
            (request.UserName.Trim().Length is < 3 or > 100 || !UserNamePattern().IsMatch(request.UserName.Trim())))
        {
            throw new InvalidOperationException("Username format is invalid.");
        }

        if (request.PhoneNumber?.Trim().Length > 30 || request.Avatar?.Trim().Length > 2048)
        {
            throw new InvalidOperationException("Profile field length is invalid.");
        }
    }

    private static void ValidateChangePasswordRequest(ChangePasswordRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword) || request.CurrentPassword.Length > 128)
        {
            throw new InvalidOperationException("Current password is required.");
        }

        ValidateStrongPassword(request.NewPassword);
        if (request.CurrentPassword == request.NewPassword)
        {
            throw new InvalidOperationException("New password must be different from current password.");
        }
    }

    private static void ValidateStrongPassword(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length is < 12 or > 128 ||
            !value.Any(char.IsUpper) || !value.Any(char.IsLower) || !value.Any(char.IsDigit))
        {
            throw new InvalidOperationException(
                "Password must be 12 to 128 characters and include uppercase, lowercase and a number.");
        }
    }

    private static void ValidateEmail(string value)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(value) || value.Trim().Length > 256 ||
                !new MailAddress(value.Trim()).Address.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                throw new FormatException();
            }
        }
        catch (FormatException)
        {
            throw new InvalidOperationException("Email format is invalid.");
        }
    }

    private static string NormalizeUserName(string? userName, string email) =>
        string.IsNullOrWhiteSpace(userName)
            ? email.Split('@')[0].Trim().ToLowerInvariant()
            : userName.Trim().ToLowerInvariant();

    private static string? NormalizeNullable(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    [GeneratedRegex("^[a-zA-Z0-9._-]+$", RegexOptions.CultureInvariant)]
    private static partial Regex UserNamePattern();
}
