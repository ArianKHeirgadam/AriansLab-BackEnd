using Application.DTOs.Services;

namespace Application.Interfaces;

public interface IServiceReadService
{
    Task<List<ServiceListItemDto>> GetServicesAsync(
        CancellationToken cancellationToken = default);

    Task<List<ServiceListItemDto>> GetFeaturedServicesAsync(
        CancellationToken cancellationToken = default);

    Task<ServiceDetailDto?> GetServiceBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);
}