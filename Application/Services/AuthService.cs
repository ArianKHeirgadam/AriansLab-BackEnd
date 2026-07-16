using Application.Common.Exceptions;
using Application.DTOs.Auth;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services;

public class AuthService : IAuthService
{
    private const string InvalidCredentialsMessage = "Invalid email/username or password.";
    private const string InvalidRefreshTokenMessage = "Refresh token is invalid or expired.";
    private const string DummyPasswordHash =
        "600000.QXJpYW5zTGFiRHVtbXkxIQ==.5b79A3Ol88+SZ9KUwUzXMJiNe1JCPkgJvTkvHEPGiDY=";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenRotationService _refreshTokenRotationService;
    private readonly IValidator<RegisterRequestDto> _registerValidator;
    private readonly IValidator<LoginRequestDto> _loginValidator;
    private readonly IValidator<RefreshTokenRequestDto> _refreshTokenValidator;

    public AuthService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IRefreshTokenRotationService refreshTokenRotationService,
        IValidator<RegisterRequestDto> registerValidator,
        IValidator<LoginRequestDto> loginValidator,
        IValidator<RefreshTokenRequestDto> refreshTokenValidator)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _refreshTokenRotationService = refreshTokenRotationService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _refreshTokenValidator = refreshTokenValidator;
    }

    public async Task<AuthResponseDto> RegisterAsync(
        RegisterRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _registerValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            ThrowValidationException(validationResult);
        }

        var normalizedEmail = Normalize(request.Email);
        var normalizedUserName = Normalize(request.UserName);
        var existingUsers = await _unitOfWork.Repository<User>().FindAsync(
            x => x.NormalizedEmail == normalizedEmail || x.NormalizedUserName == normalizedUserName,
            cancellationToken);

        if (existingUsers.Any(x => x.NormalizedEmail == normalizedEmail))
        {
            throw new ApiException("Email is already registered.", 409);
        }

        if (existingUsers.Any(x => x.NormalizedUserName == normalizedUserName))
        {
            throw new ApiException("UserName is already registered.", 409);
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            NormalizedEmail = normalizedEmail,
            UserName = request.UserName.Trim(),
            NormalizedUserName = normalizedUserName,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
            Role = UserRole.Customer,
            IsActive = true,
            EmailConfirmed = false
        };

        var issuedRefreshToken = CreateRefreshToken(user.Id);
        user.RefreshTokens.Add(issuedRefreshToken.Entity);

        await _unitOfWork.Repository<User>().AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateAuthResponse(user, issuedRefreshToken);
    }

    public async Task<AuthResponseDto> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _loginValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            ThrowValidationException(validationResult);
        }

        var normalizedIdentifier = Normalize(request.EmailOrUserName);
        var users = await _unitOfWork.Repository<User>().FindAsync(
            x => x.NormalizedEmail == normalizedIdentifier || x.NormalizedUserName == normalizedIdentifier,
            cancellationToken);
        var user = users.FirstOrDefault();

        if (user is null)
        {
            _passwordHasher.VerifyPassword(request.Password, DummyPasswordHash);
            throw new ApiException(InvalidCredentialsMessage, 401);
        }

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new ApiException(InvalidCredentialsMessage, 401);
        }

        if (!user.IsActive)
        {
            throw new ApiException(InvalidCredentialsMessage, 401);
        }

        if (_passwordHasher.NeedsRehash(user.PasswordHash))
        {
            user.PasswordHash = _passwordHasher.HashPassword(request.Password);
        }

        user.LastLoginAt = DateTime.UtcNow;
        var issuedRefreshToken = CreateRefreshToken(user.Id);

        await _unitOfWork.Repository<RefreshToken>().AddAsync(issuedRefreshToken.Entity, cancellationToken);
        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateAuthResponse(user, issuedRefreshToken);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(
        RefreshTokenRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _refreshTokenValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            ThrowValidationException(validationResult);
        }

        var replacementRawToken = _jwtTokenService.GenerateRefreshToken();
        var replacementExpiresAt = _jwtTokenService.GetRefreshTokenExpiration();
        var rotation = await _refreshTokenRotationService.RotateAsync(
            HashRefreshToken(request.RefreshToken),
            HashRefreshToken(replacementRawToken),
            replacementExpiresAt,
            cancellationToken);

        if (!rotation.Succeeded || rotation.User is null)
        {
            throw new ApiException(InvalidRefreshTokenMessage, 401);
        }

        return CreateAuthResponse(rotation.User, replacementRawToken, replacementExpiresAt);
    }

    public async Task RevokeRefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken) || refreshToken.Length > 500)
        {
            return;
        }

        var tokenHash = HashRefreshToken(refreshToken);
        var refreshTokens = await _unitOfWork.Repository<RefreshToken>().FindAsync(
            x => x.Token == tokenHash,
            cancellationToken);
        var token = refreshTokens.FirstOrDefault();

        if (token is null || token.IsRevoked)
        {
            return;
        }

        token.IsRevoked = true;
        _unitOfWork.Repository<RefreshToken>().Update(token);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserDto?> GetUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId, cancellationToken);
        return user is null || !user.IsActive ? null : MapUser(user);
    }

    private IssuedRefreshToken CreateRefreshToken(Guid userId)
    {
        var rawToken = _jwtTokenService.GenerateRefreshToken();
        var expiresAt = _jwtTokenService.GetRefreshTokenExpiration();

        return new IssuedRefreshToken(
            rawToken,
            new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Token = HashRefreshToken(rawToken),
                ExpireDate = expiresAt,
                IsRevoked = false
            });
    }

    private AuthResponseDto CreateAuthResponse(User user, IssuedRefreshToken refreshToken)
    {
        return CreateAuthResponse(user, refreshToken.RawToken, refreshToken.Entity.ExpireDate);
    }

    private AuthResponseDto CreateAuthResponse(User user, string refreshToken, DateTime refreshExpiresAt)
    {
        return new AuthResponseDto
        {
            AccessToken = _jwtTokenService.GenerateAccessToken(user),
            RefreshToken = refreshToken,
            AccessTokenExpiresAt = _jwtTokenService.GetAccessTokenExpiration(),
            RefreshTokenExpiresAt = refreshExpiresAt,
            User = MapUser(user)
        };
    }

    private static UserDto MapUser(User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email,
        UserName = user.UserName,
        Role = user.Role,
        Avatar = user.Avatar,
        IsActive = user.IsActive,
        EmailConfirmed = user.EmailConfirmed
    };

    private static string HashRefreshToken(string token)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }

    private static string Normalize(string value) => value.Trim().ToUpperInvariant();

    private static void ThrowValidationException(ValidationResult validationResult)
    {
        var errors = validationResult.Errors
            .GroupBy(x => x.PropertyName)
            .ToDictionary(x => x.Key, x => x.Select(error => error.ErrorMessage).ToArray());

        throw new Application.Common.Exceptions.ValidationException(errors);
    }

    private sealed record IssuedRefreshToken(string RawToken, RefreshToken Entity);
}
