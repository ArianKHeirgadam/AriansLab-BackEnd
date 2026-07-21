using Application.Common.Models;
using Application.DTOs.Portfolio;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/admin/portfolio")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminPortfolioController : ControllerBase
{
    private readonly IPortfolioAdminService _portfolioAdminService;

    public AdminPortfolioController(IPortfolioAdminService portfolioAdminService)
    {
        _portfolioAdminService = portfolioAdminService;
    }

    /// <summary>
    /// Gets all portfolio items for the admin panel.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<AdminPortfolioDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<AdminPortfolioDto>>>> GetAll(
        CancellationToken cancellationToken)
    {
        var items = await _portfolioAdminService.GetAllAsync(cancellationToken);

        return Ok(ApiResponse<List<AdminPortfolioDto>>.Ok(
            items,
            "Admin portfolio items retrieved successfully."));
    }

    /// <summary>
    /// Gets a portfolio item by id for the admin panel.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AdminPortfolioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AdminPortfolioDto>>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var item = await _portfolioAdminService.GetByIdAsync(id, cancellationToken);

        if (item is null)
        {
            return NotFound(ApiResponse.Fail(
                "Portfolio item was not found."));
        }

        return Ok(ApiResponse<AdminPortfolioDto>.Ok(
            item,
            "Admin portfolio item retrieved successfully."));
    }

    /// <summary>
    /// Creates a portfolio item.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AdminPortfolioDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<AdminPortfolioDto>>> Create(
        [FromBody] CreatePortfolioRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var item = await _portfolioAdminService.CreateAsync(
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = item.Id },
                ApiResponse<AdminPortfolioDto>.Ok(
                    item,
                    "Portfolio item created successfully."));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(ApiResponse.Fail(exception.Message));
        }
    }

    /// <summary>
    /// Updates a portfolio item.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AdminPortfolioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AdminPortfolioDto>>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdatePortfolioRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var item = await _portfolioAdminService.UpdateAsync(
                id,
                request,
                cancellationToken);

            if (item is null)
            {
                return NotFound(ApiResponse.Fail(
                    "Portfolio item was not found."));
            }

            return Ok(ApiResponse<AdminPortfolioDto>.Ok(
                item,
                "Portfolio item updated successfully."));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(ApiResponse.Fail(exception.Message));
        }
    }

    /// <summary>
    /// Soft deletes a portfolio item and its related records.
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
        var deleted = await _portfolioAdminService.DeleteAsync(
            id,
            cancellationToken);

        if (!deleted)
        {
            return NotFound(ApiResponse.Fail(
                "Portfolio item was not found."));
        }

        return Ok(ApiResponse.Ok(
            "Portfolio item deleted successfully."));
    }
}
