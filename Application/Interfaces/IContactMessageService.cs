using Application.DTOs.ContactMessages;

namespace Application.Interfaces;

public interface IContactMessageService
{
    Task<ContactMessageSubmissionResultDto> CreateAsync(
        CreateContactMessageRequestDto request,
        CancellationToken cancellationToken = default);
}