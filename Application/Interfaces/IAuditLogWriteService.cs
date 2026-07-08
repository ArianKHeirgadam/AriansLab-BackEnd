using Application.DTOs.Logs;

namespace Application.Interfaces;

public interface IAuditLogWriteService
{
    Task CreateAsync(
        CreateAuditLogRequestDto request,
        CancellationToken cancellationToken = default);
}