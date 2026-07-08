using Application.DTOs.SocialMedias;

namespace Application.Interfaces;

public interface ISocialMediaAdminService
{
    Task<List<SocialMediaDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<SocialMediaDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<SocialMediaDto> CreateAsync(
        CreateSocialMediaRequestDto request,
        CancellationToken cancellationToken = default);

    Task<SocialMediaDto?> UpdateAsync(
        Guid id,
        UpdateSocialMediaRequestDto request,
        CancellationToken cancellationToken = default);

    Task<SocialMediaDto?> UpdateActiveStatusAsync(
        Guid id,
        UpdateSocialMediaActiveStatusRequestDto request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}