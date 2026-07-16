using Application.Interfaces;
using Application.Security;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using System.Data;

namespace Persistence.Services;

public sealed class RefreshTokenRotationService : IRefreshTokenRotationService
{
    private readonly ApplicationDbContext _dbContext;

    public RefreshTokenRotationService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<RefreshTokenRotationResult> RotateAsync(
        string currentTokenHash,
        string replacementTokenHash,
        DateTime replacementExpiresAt,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Database.IsRelational()
            ? RotateRelationalAsync(currentTokenHash, replacementTokenHash, replacementExpiresAt, cancellationToken)
            : RotateNonRelationalAsync(currentTokenHash, replacementTokenHash, replacementExpiresAt, cancellationToken);
    }

    private async Task<RefreshTokenRotationResult> RotateRelationalAsync(
        string currentTokenHash,
        string replacementTokenHash,
        DateTime replacementExpiresAt,
        CancellationToken cancellationToken)
    {
        var executionStrategy = _dbContext.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.ReadCommitted,
                cancellationToken);

            var currentToken = await _dbContext.RefreshTokens
                .AsNoTracking()
                .Where(token => token.Token == currentTokenHash)
                .Select(token => new RefreshTokenState(
                    token.Id,
                    token.UserId,
                    token.ExpireDate,
                    token.IsRevoked))
                .FirstOrDefaultAsync(cancellationToken);

            if (currentToken is null)
            {
                await transaction.CommitAsync(cancellationToken);
                return RefreshTokenRotationResult.Invalid;
            }

            if (currentToken.IsRevoked)
            {
                await RevokeAllActiveRelationalAsync(currentToken.UserId, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return RefreshTokenRotationResult.Invalid;
            }

            var consumedRows = await _dbContext.RefreshTokens
                .Where(token => token.Id == currentToken.Id && !token.IsRevoked)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(token => token.IsRevoked, true),
                    cancellationToken);

            if (consumedRows != 1)
            {
                await RevokeAllActiveRelationalAsync(currentToken.UserId, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return RefreshTokenRotationResult.Invalid;
            }

            if (currentToken.ExpireDate <= DateTime.UtcNow)
            {
                await transaction.CommitAsync(cancellationToken);
                return RefreshTokenRotationResult.Invalid;
            }

            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    item => item.Id == currentToken.UserId && item.IsActive,
                    cancellationToken);

            if (user is null)
            {
                await RevokeAllActiveRelationalAsync(currentToken.UserId, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return RefreshTokenRotationResult.Invalid;
            }

            await _dbContext.RefreshTokens.AddAsync(
                CreateReplacement(user.Id, replacementTokenHash, replacementExpiresAt),
                cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return RefreshTokenRotationResult.Success(user);
        });
    }

    private async Task<RefreshTokenRotationResult> RotateNonRelationalAsync(
        string currentTokenHash,
        string replacementTokenHash,
        DateTime replacementExpiresAt,
        CancellationToken cancellationToken)
    {
        var currentToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(
            token => token.Token == currentTokenHash,
            cancellationToken);

        if (currentToken is null)
        {
            return RefreshTokenRotationResult.Invalid;
        }

        if (currentToken.IsRevoked)
        {
            await RevokeAllActiveNonRelationalAsync(currentToken.UserId, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return RefreshTokenRotationResult.Invalid;
        }

        currentToken.IsRevoked = true;

        if (currentToken.ExpireDate <= DateTime.UtcNow)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return RefreshTokenRotationResult.Invalid;
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(
            item => item.Id == currentToken.UserId && item.IsActive,
            cancellationToken);

        if (user is null)
        {
            await RevokeAllActiveNonRelationalAsync(currentToken.UserId, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return RefreshTokenRotationResult.Invalid;
        }

        await _dbContext.RefreshTokens.AddAsync(
            CreateReplacement(user.Id, replacementTokenHash, replacementExpiresAt),
            cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return RefreshTokenRotationResult.Success(user);
    }

    private Task<int> RevokeAllActiveRelationalAsync(Guid userId, CancellationToken cancellationToken)
    {
        return _dbContext.RefreshTokens
            .Where(token => token.UserId == userId && !token.IsRevoked)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(token => token.IsRevoked, true),
                cancellationToken);
    }

    private async Task RevokeAllActiveNonRelationalAsync(Guid userId, CancellationToken cancellationToken)
    {
        var activeTokens = await _dbContext.RefreshTokens
            .Where(token => token.UserId == userId && !token.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
        }
    }

    private static RefreshToken CreateReplacement(Guid userId, string tokenHash, DateTime expiresAt) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Token = tokenHash,
        ExpireDate = expiresAt,
        IsRevoked = false
    };

    private sealed record RefreshTokenState(Guid Id, Guid UserId, DateTime ExpireDate, bool IsRevoked);
}
