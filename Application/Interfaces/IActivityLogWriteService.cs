using Application.DTOs.Logs;

namespace Application.Interfaces;

public interface IActivityLogWriteService
{
    Task CreateAsync(
        CreateActivityLogRequestDto request,
        CancellationToken cancellationToken = default);
}