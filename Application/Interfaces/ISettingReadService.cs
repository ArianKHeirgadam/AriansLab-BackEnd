using Application.DTOs.Settings;

namespace Application.Interfaces;

public interface ISettingReadService
{
    Task<SettingDto?> GetCurrentAsync(
        CancellationToken cancellationToken = default);

    Task<SettingDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}