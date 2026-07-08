using Application.DTOs.Settings;

namespace Application.Interfaces;

public interface ISiteSettingAdminService
{
    Task<List<SiteSettingDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<SiteSettingDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<SiteSettingDto> CreateAsync(
        CreateSiteSettingRequestDto request,
        CancellationToken cancellationToken = default);

    Task<SiteSettingDto?> UpdateAsync(
        Guid id,
        UpdateSiteSettingRequestDto request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}