using Application.DTOs.HeroSections;

namespace Application.Interfaces;

public interface IHeroSectionAdminService
{
    Task<List<HeroSectionDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<HeroSectionDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<HeroSectionDto> CreateAsync(
        CreateHeroSectionRequestDto request,
        CancellationToken cancellationToken = default);

    Task<HeroSectionDto?> UpdateAsync(
        Guid id,
        UpdateHeroSectionRequestDto request,
        CancellationToken cancellationToken = default);

    Task<HeroSectionDto?> UpdateActiveStatusAsync(
        Guid id,
        UpdateHeroSectionActiveStatusRequestDto request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}