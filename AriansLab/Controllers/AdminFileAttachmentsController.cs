using Application.Common.Models;
using Application.DTOs.Files;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/admin/file-attachments")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminFileAttachmentsController : ControllerBase
{
    private readonly IFileAttachmentAdminService _fileAttachmentAdminService;

    public AdminFileAttachmentsController(
        IFileAttachmentAdminService fileAttachmentAdminService)
    {
        _fileAttachmentAdminService = fileAttachmentAdminService;
    }

    /// <summary>
    /// Gets all file attachments for admin panel.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<FileAttachmentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<FileAttachmentDto>>>> GetAll(
        CancellationToken cancellationToken)
    {
        var attachments = await _fileAttachmentAdminService.GetAllAsync(
            cancellationToken
        );

        return Ok(ApiResponse<List<FileAttachmentDto>>.Ok(
            attachments,
            "File attachments retrieved successfully."
        ));
    }

    /// <summary>
    /// Gets file attachments by module and reference id.
    /// </summary>
    [HttpGet("reference/{module}/{referenceId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<FileAttachmentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<FileAttachmentDto>>>> GetByReference(
        [FromRoute] string module,
        [FromRoute] Guid referenceId,
        CancellationToken cancellationToken)
    {
        var attachments = await _fileAttachmentAdminService.GetByReferenceAsync(
            module,
            referenceId,
            cancellationToken
        );

        return Ok(ApiResponse<List<FileAttachmentDto>>.Ok(
            attachments,
            "File attachments retrieved successfully."
        ));
    }

    /// <summary>
    /// Gets a single file attachment by id for admin panel.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<FileAttachmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<FileAttachmentDto>>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var attachment = await _fileAttachmentAdminService.GetByIdAsync(
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
    /// Creates a file attachment metadata record.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<FileAttachmentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<FileAttachmentDto>>> Create(
        [FromBody] CreateFileAttachmentRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var attachment = await _fileAttachmentAdminService.CreateAsync(
                request,
                cancellationToken
            );

            return CreatedAtAction(
                nameof(GetById),
                new { id = attachment.Id },
                ApiResponse<FileAttachmentDto>.Ok(
                    attachment,
                    "File attachment created successfully."
                )
            );
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Updates a file attachment metadata record.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<FileAttachmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<FileAttachmentDto>>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateFileAttachmentRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var attachment = await _fileAttachmentAdminService.UpdateAsync(
                id,
                request,
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
                "File attachment updated successfully."
            ));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Soft deletes a file attachment metadata record.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await _fileAttachmentAdminService.DeleteAsync(
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
}