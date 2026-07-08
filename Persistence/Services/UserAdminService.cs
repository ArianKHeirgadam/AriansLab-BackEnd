using Application.DTOs.Users;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class UserAdminService : IUserAdminService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    public UserAdminService(
        ApplicationDbContext dbContext,
        IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<List<AdminUserDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new AdminUserDto
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
            .ToListAsync(cancellationToken);
    }

    public async Task<AdminUserDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new AdminUserDto
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

    public async Task<AdminUserDto> CreateAsync(
        CreateUserRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateCreateRequest(request);

        var email = request.Email.Trim().ToLowerInvariant();
        var userName = NormalizeUserName(request.UserName, email);

        var emailExists = await _dbContext.Users
            .AnyAsync(x => x.NormalizedEmail == email.ToUpperInvariant(), cancellationToken);

        if (emailExists)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var userNameExists = await _dbContext.Users
            .AnyAsync(x => x.NormalizedUserName == userName.ToUpperInvariant(), cancellationToken);

        if (userNameExists)
        {
            throw new InvalidOperationException("A user with this username already exists.");
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            UserName = userName,
            NormalizedUserName = userName.ToUpperInvariant(),
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            PhoneNumber = request.PhoneNumber?.Trim(),
            Role = request.Role,
            IsActive = request.IsActive,
            EmailConfirmed = request.EmailConfirmed,
            Avatar = request.Avatar?.Trim()
        };

        await _dbContext.Users.AddAsync(user, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var createdUser = await GetByIdAsync(user.Id, cancellationToken);

        return createdUser!;
    }

    public async Task<AdminUserDto?> UpdateAsync(
        Guid id,
        UpdateUserRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateUpdateRequest(request);

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var email = request.Email.Trim().ToLowerInvariant();
        var userName = NormalizeUserName(request.UserName, email);

        var emailExists = await _dbContext.Users
            .AnyAsync(
                x => x.Id != id && x.NormalizedEmail == email.ToUpperInvariant(),
                cancellationToken
            );

        if (emailExists)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var userNameExists = await _dbContext.Users
            .AnyAsync(
                x => x.Id != id && x.NormalizedUserName == userName.ToUpperInvariant(),
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
        user.Role = request.Role;
        user.IsActive = request.IsActive;
        user.EmailConfirmed = request.EmailConfirmed;
        user.Avatar = request.Avatar?.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(user.Id, cancellationToken);
    }

    public async Task<AdminUserDto?> ResetPasswordAsync(
        Guid id,
        ResetUserPasswordRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.NewPassword))
        {
            throw new InvalidOperationException("New password is required.");
        }

        if (request.NewPassword.Length < 8)
        {
            throw new InvalidOperationException("Password must be at least 8 characters.");
        }

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (user is null)
        {
            return null;
        }

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(user.Id, cancellationToken);
    }

    public async Task<AdminUserDto?> ActivateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (user is null)
        {
            return null;
        }

        user.IsActive = true;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(user.Id, cancellationToken);
    }

    public async Task<AdminUserDto?> DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (user is null)
        {
            return null;
        }

        user.IsActive = false;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(user.Id, cancellationToken);
    }

    private static void ValidateCreateRequest(CreateUserRequestDto request)
    {
        ValidateSharedFields(
            request.FullName,
            request.Email
        );

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new InvalidOperationException("Password is required.");
        }

        if (request.Password.Length < 8)
        {
            throw new InvalidOperationException("Password must be at least 8 characters.");
        }
    }

    private static void ValidateUpdateRequest(UpdateUserRequestDto request)
    {
        ValidateSharedFields(
            request.FullName,
            request.Email
        );
    }

    private static void ValidateSharedFields(string fullName, string email)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new InvalidOperationException("Full name is required.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException("Email is required.");
        }

        if (!email.Contains('@'))
        {
            throw new InvalidOperationException("Email format is invalid.");
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