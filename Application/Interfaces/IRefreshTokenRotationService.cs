using Application.Security;

namespace Application.Interfaces;

public interface IRefreshTokenRotationService
{
    Task<RefreshTokenRotationResult> RotateAsync(
        string currentTokenHash,
        string replacementTokenHash,
        DateTime replacementExpiresAt,
        CancellationToken cancellationToken = default);
}
