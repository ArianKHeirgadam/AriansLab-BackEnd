using Application.DTOs.Users;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Persistence.Services;

public partial class UserAdminService : IUserAdminService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    public UserAdminService(ApplicationDbContext dbContext, IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public Task<List<AdminUserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.Users
            .AsNoTracking()
            .OrderByDescending(user => user.CreatedAt)
            .Select(user => new AdminUserDto
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
            .ToListAsync(cancellationToken);
    }

    public Task<AdminUserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users
            .AsNoTracking()
            .Where(user => user.Id == id)
            .Select(user => new AdminUserDto
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

    public async Task<AdminUserDto> CreateAsync(
        CreateUserRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateSharedFields(request.FullName, request.Email, request.UserName, request.PhoneNumber, request.Avatar, request.Role);
        ValidateStrongPassword(request.Password);

        var email = request.Email.Trim().ToLowerInvariant();
        var userName = NormalizeUserName(request.UserName, email);
        await EnsureUniqueAsync(null, email, userName, cancellationToken);

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            UserName = userName,
            NormalizedUserName = userName.ToUpperInvariant(),
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            PhoneNumber = NormalizeNullable(request.PhoneNumber),
            Role = request.Role,
            IsActive = request.IsActive,
            EmailConfirmed = request.EmailConfirmed,
            Avatar = NormalizeNullable(request.Avatar)
        };

        await _dbContext.Users.AddAsync(user, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return (await GetByIdAsync(user.Id, cancellationToken))!;
    }

    public async Task<AdminUserDto?> UpdateAsync(
        Guid id,
        UpdateUserRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateSharedFields(request.FullName, request.Email, request.UserName, request.PhoneNumber, request.Avatar, request.Role);
        var user = await _dbContext.Users.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (user is null)
        {
            return null;
        }

        if (user.Role == UserRole.Admin && user.IsActive &&
            (request.Role != UserRole.Admin || !request.IsActive))
        {
            await EnsureAnotherActiveAdminExistsAsync(user.Id, cancellationToken);
        }

        var email = request.Email.Trim().ToLowerInvariant();
        var userName = NormalizeUserName(request.UserName, email);
        await EnsureUniqueAsync(id, email, userName, cancellationToken);

        user.FullName = request.FullName.Trim();
        user.Email = email;
        user.NormalizedEmail = email.ToUpperInvariant();
        user.UserName = userName;
        user.NormalizedUserName = userName.ToUpperInvariant();
        user.PhoneNumber = NormalizeNullable(request.PhoneNumber);
        user.Role = request.Role;
        user.IsActive = request.IsActive;
        user.EmailConfirmed = request.EmailConfirmed;
        user.Avatar = NormalizeNullable(request.Avatar);

        await RevokeActiveRefreshTokensAsync(user.Id, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(user.Id, cancellationToken);
    }

    public async Task<AdminUserDto?> ResetPasswordAsync(
        Guid id,
        ResetUserPasswordRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateStrongPassword(request.NewPassword);
        var user = await _dbContext.Users.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (user is null)
        {
            return null;
        }

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        await RevokeActiveRefreshTokensAsync(user.Id, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(user.Id, cancellationToken);
    }

    public async Task<AdminUserDto?> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (user is null)
        {
            return null;
        }

        user.IsActive = true;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(user.Id, cancellationToken);
    }

    public async Task<AdminUserDto?> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (user is null)
        {
            return null;
        }

        if (user.Role == UserRole.Admin && user.IsActive)
        {
            await EnsureAnotherActiveAdminExistsAsync(user.Id, cancellationToken);
        }

        user.IsActive = false;
        await RevokeActiveRefreshTokensAsync(user.Id, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(user.Id, cancellationToken);
    }

    private async Task EnsureUniqueAsync(
        Guid? currentId,
        string email,
        string userName,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = email.ToUpperInvariant();
        var normalizedUserName = userName.ToUpperInvariant();

        if (await _dbContext.Users.AnyAsync(
                user => user.Id != currentId && user.NormalizedEmail == normalizedEmail,
                cancellationToken))
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        if (await _dbContext.Users.AnyAsync(
                user => user.Id != currentId && user.NormalizedUserName == normalizedUserName,
                cancellationToken))
        {
            throw new InvalidOperationException("A user with this username already exists.");
        }
    }

    private async Task EnsureAnotherActiveAdminExistsAsync(Guid excludedId, CancellationToken cancellationToken)
    {
        if (!await _dbContext.Users.AnyAsync(
                user => user.Id != excludedId && user.Role == UserRole.Admin && user.IsActive,
                cancellationToken))
        {
            throw new InvalidOperationException("The last active administrator cannot be deactivated or demoted.");
        }
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

    private static void ValidateSharedFields(
        string fullName,
        string email,
        string? userName,
        string? phoneNumber,
        string? avatar,
        UserRole role)
    {
        if (string.IsNullOrWhiteSpace(fullName) || fullName.Trim().Length > 150)
        {
            throw new InvalidOperationException("Full name is required and cannot exceed 150 characters.");
        }

        ValidateEmail(email);
        if (!string.IsNullOrWhiteSpace(userName) &&
            (userName.Trim().Length is < 3 or > 100 || !UserNamePattern().IsMatch(userName.Trim())))
        {
            throw new InvalidOperationException("Username format is invalid.");
        }

        if (phoneNumber?.Trim().Length > 30 || avatar?.Trim().Length > 2048 || !Enum.IsDefined(role))
        {
            throw new InvalidOperationException("User field value is invalid.");
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
