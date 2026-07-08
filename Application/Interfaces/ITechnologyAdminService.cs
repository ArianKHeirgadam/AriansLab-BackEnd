using Application.DTOs.Technologies;

namespace Application.Interfaces;

public interface ITechnologyAdminService
{
    Task<List<TechnologyDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<TechnologyDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<TechnologyDto> CreateAsync(
        CreateTechnologyRequestDto request,
        CancellationToken cancellationToken = default);

    Task<TechnologyDto?> UpdateAsync(
        Guid id,
        UpdateTechnologyRequestDto request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}