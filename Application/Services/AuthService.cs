using Application.Common.Exceptions;
using Application.DTOs.Auth;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using FluentValidation;
using FluentValidation.Results;

namespace Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IValidator<RegisterRequestDto> _registerValidator;
    private readonly IValidator<LoginRequestDto> _loginValidator;
    private readonly IValidator<RefreshTokenRequestDto> _refreshTokenValidator;

    public AuthService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IValidator<RegisterRequestDto> registerValidator,
        IValidator<LoginRequestDto> loginValidator,
        IValidator<RefreshTokenRequestDto> refreshTokenValidator)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
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
            Email = request.Email.Trim(),
            NormalizedEmail = normalizedEmail,
            UserName = request.UserName.Trim(),
            NormalizedUserName = normalizedUserName,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber)
                ? null
                : request.PhoneNumber.Trim(),
            Role = UserRole.Customer,
            IsActive = true,
            EmailConfirmed = false
        };

        var refreshToken = CreateRefreshToken(user.Id);

        user.RefreshTokens.Add(refreshToken);

        await _unitOfWork.Repository<User>().AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateAuthResponse(user, refreshToken);
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
            throw new ApiException("Invalid email/username or password.", 401);
        }

        if (!user.IsActive)
        {
            throw new ApiException("User account is inactive.", 403);
        }

        var isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);

        if (!isPasswordValid)
        {
            throw new ApiException("Invalid email/username or password.", 401);
        }

        user.LastLoginAt = DateTime.UtcNow;

        var refreshToken = CreateRefreshToken(user.Id);

        await _unitOfWork.Repository<RefreshToken>().AddAsync(refreshToken, cancellationToken);
        _unitOfWork.Repository<User>().Update(user);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateAuthResponse(user, refreshToken);
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

        var refreshTokens = await _unitOfWork.Repository<RefreshToken>().FindAsync(
            x => x.Token == request.RefreshToken,
            cancellationToken);

        var oldRefreshToken = refreshTokens.FirstOrDefault();

        if (oldRefreshToken is null)
        {
            throw new ApiException("Refresh token is invalid.", 401);
        }

        if (oldRefreshToken.IsRevoked)
        {
            throw new ApiException("Refresh token has been revoked.", 401);
        }

        if (oldRefreshToken.ExpireDate <= DateTime.UtcNow)
        {
            throw new ApiException("Refresh token has expired.", 401);
        }

        var user = await _unitOfWork.Repository<User>().GetByIdAsync(
            oldRefreshToken.UserId,
            cancellationToken);

        if (user is null)
        {
            throw new ApiException("User was not found.", 404);
        }

        if (!user.IsActive)
        {
            throw new ApiException("User account is inactive.", 403);
        }

        oldRefreshToken.IsRevoked = true;

        var newRefreshToken = CreateRefreshToken(user.Id);

        _unitOfWork.Repository<RefreshToken>().Update(oldRefreshToken);
        await _unitOfWork.Repository<RefreshToken>().AddAsync(newRefreshToken, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateAuthResponse(user, newRefreshToken);
    }

    private RefreshToken CreateRefreshToken(Guid userId)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = _jwtTokenService.GenerateRefreshToken(),
            ExpireDate = _jwtTokenService.GetRefreshTokenExpiration(),
            IsRevoked = false
        };
    }

    private AuthResponseDto CreateAuthResponse(User user, RefreshToken refreshToken)
    {
        return new AuthResponseDto
        {
            AccessToken = _jwtTokenService.GenerateAccessToken(user),
            RefreshToken = refreshToken.Token,
            AccessTokenExpiresAt = _jwtTokenService.GetAccessTokenExpiration(),
            RefreshTokenExpiresAt = refreshToken.ExpireDate,
            User = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                UserName = user.UserName,
                Role = user.Role,
                Avatar = user.Avatar,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed
            }
        };
    }

    private static string Normalize(string value)
    {
        return value.Trim().ToUpperInvariant();
    }

    private static void ThrowValidationException(ValidationResult validationResult)
    {
        var errors = validationResult.Errors
            .GroupBy(x => x.PropertyName)
            .ToDictionary(
                x => x.Key,
                x => x.Select(e => e.ErrorMessage).ToArray());

        throw new Application.Common.Exceptions.ValidationException(errors);
    }
}