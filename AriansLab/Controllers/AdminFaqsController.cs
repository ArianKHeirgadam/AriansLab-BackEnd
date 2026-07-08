using Application.Common.Models;
using Application.DTOs.FAQs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/admin/faqs")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminFaqsController : ControllerBase
{
    private readonly IFaqAdminService _faqAdminService;

    public AdminFaqsController(IFaqAdminService faqAdminService)
    {
        _faqAdminService = faqAdminService;
    }

    /// <summary>
    /// Gets all FAQs for admin panel.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<AdminFaqDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<AdminFaqDto>>>> GetAll(
        CancellationToken cancellationToken)
    {
        var faqs = await _faqAdminService.GetAllAsync(cancellationToken);

        return Ok(ApiResponse<List<AdminFaqDto>>.Ok(
            faqs,
            "Admin FAQs retrieved successfully."
        ));
    }

    /// <summary>
    /// Gets a single FAQ by id for admin panel.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AdminFaqDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AdminFaqDto>>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var faq = await _faqAdminService.GetByIdAsync(
            id,
            cancellationToken
        );

        if (faq is null)
        {
            return NotFound(ApiResponse.Fail(
                "FAQ was not found."
            ));
        }

        return Ok(ApiResponse<AdminFaqDto>.Ok(
            faq,
            "Admin FAQ retrieved successfully."
        ));
    }

    /// <summary>
    /// Creates a new FAQ.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AdminFaqDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<AdminFaqDto>>> Create(
        [FromBody] CreateFaqRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var faq = await _faqAdminService.CreateAsync(
                request,
                cancellationToken
            );

            return CreatedAtAction(
                nameof(GetById),
                new { id = faq.Id },
                ApiResponse<AdminFaqDto>.Ok(
                    faq,
                    "FAQ created successfully."
                )
            );
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Updates an existing FAQ.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AdminFaqDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AdminFaqDto>>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateFaqRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var faq = await _faqAdminService.UpdateAsync(
                id,
                request,
                cancellationToken
            );

            if (faq is null)
            {
                return NotFound(ApiResponse.Fail(
                    "FAQ was not found."
                ));
            }

            return Ok(ApiResponse<AdminFaqDto>.Ok(
                faq,
                "FAQ updated successfully."
            ));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Soft deletes an existing FAQ.
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
        var deleted = await _faqAdminService.DeleteAsync(
            id,
            cancellationToken
        );

        if (!deleted)
        {
            return NotFound(ApiResponse.Fail(
                "FAQ was not found."
            ));
        }

        return Ok(ApiResponse.Ok(
            "FAQ deleted successfully."
        ));
    }
}