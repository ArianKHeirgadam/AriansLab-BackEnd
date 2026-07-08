using Application.DTOs.HeroSections;

namespace Application.Interfaces;

public interface IHeroSectionReadService
{
    Task<List<HeroSectionDto>> GetActiveAsync(
        CancellationToken cancellationToken = default);

    Task<HeroSectionDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}