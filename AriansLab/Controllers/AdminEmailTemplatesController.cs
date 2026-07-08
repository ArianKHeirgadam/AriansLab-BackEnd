using Application.Common.Models;
using Application.DTOs.EmailTemplates;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/admin/email-templates")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminEmailTemplatesController : ControllerBase
{
    private readonly IEmailTemplateAdminService _emailTemplateAdminService;

    public AdminEmailTemplatesController(
        IEmailTemplateAdminService emailTemplateAdminService)
    {
        _emailTemplateAdminService = emailTemplateAdminService;
    }

    /// <summary>
    /// Gets all email templates for admin panel.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<EmailTemplateDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<EmailTemplateDto>>>> GetAll(
        CancellationToken cancellationToken)
    {
        var templates = await _emailTemplateAdminService.GetAllAsync(
            cancellationToken
        );

        return Ok(ApiResponse<List<EmailTemplateDto>>.Ok(
            templates,
            "Email templates retrieved successfully."
        ));
    }

    /// <summary>
    /// Gets active email templates for admin panel.
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<List<EmailTemplateDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<EmailTemplateDto>>>> GetActive(
        CancellationToken cancellationToken)
    {
        var templates = await _emailTemplateAdminService.GetActiveAsync(
            cancellationToken
        );

        return Ok(ApiResponse<List<EmailTemplateDto>>.Ok(
            templates,
            "Active email templates retrieved successfully."
        ));
    }

    /// <summary>
    /// Gets a single email template by id for admin panel.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<EmailTemplateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<EmailTemplateDto>>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var template = await _emailTemplateAdminService.GetByIdAsync(
            id,
            cancellationToken
        );

        if (template is null)
        {
            return NotFound(ApiResponse.Fail(
                "Email template was not found."
            ));
        }

        return Ok(ApiResponse<EmailTemplateDto>.Ok(
            template,
            "Email template retrieved successfully."
        ));
    }

    /// <summary>
    /// Gets a single email template by name for admin panel.
    /// </summary>
    [HttpGet("by-name/{name}")]
    [ProducesResponseType(typeof(ApiResponse<EmailTemplateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<EmailTemplateDto>>> GetByName(
        [FromRoute] string name,
        CancellationToken cancellationToken)
    {
        var template = await _emailTemplateAdminService.GetByNameAsync(
            name,
            cancellationToken
        );

        if (template is null)
        {
            return NotFound(ApiResponse.Fail(
                "Email template was not found."
            ));
        }

        return Ok(ApiResponse<EmailTemplateDto>.Ok(
            template,
            "Email template retrieved successfully."
        ));
    }

    /// <summary>
    /// Creates a new email template.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<EmailTemplateDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<EmailTemplateDto>>> Create(
        [FromBody] CreateEmailTemplateRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var template = await _emailTemplateAdminService.CreateAsync(
                request,
                cancellationToken
            );

            return CreatedAtAction(
                nameof(GetById),
                new { id = template.Id },
                ApiResponse<EmailTemplateDto>.Ok(
                    template,
                    "Email template created successfully."
                )
            );
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Updates an existing email template.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<EmailTemplateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<EmailTemplateDto>>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateEmailTemplateRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var template = await _emailTemplateAdminService.UpdateAsync(
                id,
                request,
                cancellationToken
            );

            if (template is null)
            {
                return NotFound(ApiResponse.Fail(
                    "Email template was not found."
                ));
            }

            return Ok(ApiResponse<EmailTemplateDto>.Ok(
                template,
                "Email template updated successfully."
            ));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Updates email template active status.
    /// </summary>
    [HttpPatch("{id:guid}/active-status")]
    [ProducesResponseType(typeof(ApiResponse<EmailTemplateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<EmailTemplateDto>>> UpdateActiveStatus(
        [FromRoute] Guid id,
        [FromBody] UpdateEmailTemplateActiveStatusRequestDto request,
        CancellationToken cancellationToken)
    {
        var template = await _emailTemplateAdminService.UpdateActiveStatusAsync(
            id,
            request,
            cancellationToken
        );

        if (template is null)
        {
            return NotFound(ApiResponse.Fail(
                "Email template was not found."
            ));
        }

        return Ok(ApiResponse<EmailTemplateDto>.Ok(
            template,
            "Email template active status updated successfully."
        ));
    }

    /// <summary>
    /// Soft deletes an email template.
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
        var deleted = await _emailTemplateAdminService.DeleteAsync(
            id,
            cancellationToken
        );

        if (!deleted)
        {
            return NotFound(ApiResponse.Fail(
                "Email template was not found."
            ));
        }

        return Ok(ApiResponse.Ok(
            "Email template deleted successfully."
        ));
    }
}