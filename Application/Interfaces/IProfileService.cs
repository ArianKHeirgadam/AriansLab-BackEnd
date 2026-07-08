using Application.DTOs.Profile;

namespace Application.Interfaces;

public interface IProfileService
{
    Task<ProfileDto?> GetMeAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<ProfileDto?> UpdateMeAsync(
        Guid userId,
        UpdateProfileRequestDto request,
        CancellationToken cancellationToken = default);

    Task<bool> ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequestDto request,
        CancellationToken cancellationToken = default);
}