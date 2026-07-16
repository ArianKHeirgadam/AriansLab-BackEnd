using Application.Common.Models;
using Application.DTOs.Files;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/file-attachments")]
[Authorize]
[Produces("application/json")]
public class FileAttachmentsController : ControllerBase
{
    private readonly IFileAttachmentService _fileAttachmentService;

    public FileAttachmentsController(IFileAttachmentService fileAttachmentService)
    {
        _fileAttachmentService = fileAttachmentService;
    }

    /// <summary>
    /// Gets file attachments uploaded by the authenticated user.
    /// </summary>
    [HttpGet("my")]
    [ProducesResponseType(typeof(ApiResponse<List<FileAttachmentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<List<FileAttachmentDto>>>> GetMyAttachments(
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized(ApiResponse.Fail(
                "Authenticated user id was not found."
            ));
        }

        var attachments = await _fileAttachmentService.GetMyAttachmentsAsync(
            userId.Value,
            cancellationToken
        );

        return Ok(ApiResponse<List<FileAttachmentDto>>.Ok(
            attachments,
            "File attachments retrieved successfully."
        ));
    }

    /// <summary>
    /// Gets public file attachments by module and reference id.
    /// </summary>
    [HttpGet("public/{module}/{referenceId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<List<PublicFileAttachmentDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<PublicFileAttachmentDto>>>> GetPublicByReference(
        [FromRoute] string module,
        [FromRoute] Guid referenceId,
        CancellationToken cancellationToken)
    {
        var attachments = await _fileAttachmentService.GetPublicByReferenceAsync(
            module,
            referenceId,
            cancellationToken
        );

        return Ok(ApiResponse<List<PublicFileAttachmentDto>>.Ok(
            attachments,
            "Public file attachments retrieved successfully."
        ));
    }

    /// <summary>
    /// Gets a single file attachment uploaded by the authenticated user.
    /// </summary>
    [HttpGet("my/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<FileAttachmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<FileAttachmentDto>>> GetMyAttachmentById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized(ApiResponse.Fail(
                "Authenticated user id was not found."
            ));
        }

        var attachment = await _fileAttachmentService.GetMyAttachmentByIdAsync(
            userId.Value,
            id,
            cancellationToken
        );

        if (attachment is null)
        {
            return NotFound(ApiResponse.Fail(
                "File attachment was not found."
            ));
        }

        return Ok(ApiResponse<FileAttachmentDto>.Ok(
            attachment,
            "File attachment retrieved successfully."
        ));
    }

    /// <summary>
    /// Deletes one of the authenticated user's file attachment metadata records.
    /// </summary>
    [HttpDelete("my/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteMyAttachment(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized(ApiResponse.Fail(
                "Authenticated user id was not found."
            ));
        }

        var deleted = await _fileAttachmentService.DeleteMyAttachmentAsync(
            userId.Value,
            id,
            cancellationToken
        );

        if (!deleted)
        {
            return NotFound(ApiResponse.Fail(
                "File attachment was not found."
            ));
        }

        return Ok(ApiResponse.Ok(
            "File attachment deleted successfully."
        ));
    }

    private Guid? GetCurrentUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdValue))
        {
            return null;
        }

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return null;
        }

        return userId;
    }
}
