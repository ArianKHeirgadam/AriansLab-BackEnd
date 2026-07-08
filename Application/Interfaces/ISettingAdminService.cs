using Application.DTOs.Settings;

namespace Application.Interfaces;

public interface ISettingAdminService
{
    Task<List<SettingDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<SettingDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<SettingDto> CreateAsync(
        CreateSettingRequestDto request,
        CancellationToken cancellationToken = default);

    Task<SettingDto?> UpdateAsync(
        Guid id,
        UpdateSettingRequestDto request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}