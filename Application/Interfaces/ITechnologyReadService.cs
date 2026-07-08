using Application.DTOs.Technologies;

namespace Application.Interfaces;

public interface ITechnologyReadService
{
    Task<List<TechnologyDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<TechnologyDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}