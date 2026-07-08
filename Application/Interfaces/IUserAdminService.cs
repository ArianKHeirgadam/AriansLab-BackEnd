using Application.DTOs.Users;

namespace Application.Interfaces;

public interface IUserAdminService
{
    Task<List<AdminUserDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<AdminUserDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<AdminUserDto> CreateAsync(
        CreateUserRequestDto request,
        CancellationToken cancellationToken = default);

    Task<AdminUserDto?> UpdateAsync(
        Guid id,
        UpdateUserRequestDto request,
        CancellationToken cancellationToken = default);

    Task<AdminUserDto?> ResetPasswordAsync(
        Guid id,
        ResetUserPasswordRequestDto request,
        CancellationToken cancellationToken = default);

    Task<AdminUserDto?> ActivateAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<AdminUserDto?> DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}