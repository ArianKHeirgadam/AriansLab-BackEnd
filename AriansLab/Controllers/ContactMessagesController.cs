using Application.Common.Models;
using Application.DTOs.ContactMessages;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/contact-messages")]
[AllowAnonymous]
[Produces("application/json")]
public class ContactMessagesController : ControllerBase
{
    private readonly IContactMessageService _contactMessageService;

    public ContactMessagesController(IContactMessageService contactMessageService)
    {
        _contactMessageService = contactMessageService;
    }

    /// <summary>
    /// Submits a new contact message from the public website.
    /// </summary>
    [HttpPost]
    [EnableRateLimiting("public-write")]
    [ProducesResponseType(typeof(ApiResponse<ContactMessageSubmissionResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ContactMessageSubmissionResultDto>>> Create(
        [FromBody] CreateContactMessageRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _contactMessageService.CreateAsync(
                request,
                cancellationToken
            );

            return Ok(ApiResponse<ContactMessageSubmissionResultDto>.Ok(
                result,
                "Contact message submitted successfully."
            ));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }
}
