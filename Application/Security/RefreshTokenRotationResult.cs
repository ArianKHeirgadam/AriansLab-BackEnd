using Domain.Entities;

namespace Application.Security;

public sealed record RefreshTokenRotationResult(bool Succeeded, User? User)
{
    public static RefreshTokenRotationResult Invalid { get; } = new(false, null);

    public static RefreshTokenRotationResult Success(User user) => new(true, user);
}
