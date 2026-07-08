using Application.DTOs.SocialMedias;

namespace Application.Interfaces;

public interface ISocialMediaReadService
{
    Task<List<SocialMediaDto>> GetActiveAsync(
        CancellationToken cancellationToken = default);

    Task<SocialMediaDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}