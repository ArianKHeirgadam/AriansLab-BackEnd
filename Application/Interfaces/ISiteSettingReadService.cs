using Application.DTOs.Settings;

namespace Application.Interfaces;

public interface ISiteSettingReadService
{
    Task<SiteSettingDto?> GetCurrentAsync(
        CancellationToken cancellationToken = default);

    Task<SiteSettingDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}