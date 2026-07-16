using Application.Common.Models;
using Application.DTOs.Technologies;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/technologies")]
[AllowAnonymous]
[Produces("application/json")]
public class TechnologiesController : ControllerBase
{
    private readonly ITechnologyReadService _technologyReadService;

    public TechnologiesController(
        ITechnologyReadService technologyReadService)
    {
        _technologyReadService = technologyReadService;
    }

    /// <summary>
    /// Gets all public technologies.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<TechnologyDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<TechnologyDto>>>> GetAll(
        CancellationToken cancellationToken)
    {
        var technologies = await _technologyReadService.GetAllAsync(
            cancellationToken
        );

        return Ok(ApiResponse<List<TechnologyDto>>.Ok(
            technologies,
            "Technologies retrieved successfully."
        ));
    }

    /// <summary>
    /// Gets a single public technology by id.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TechnologyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TechnologyDto>>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var technology = await _technologyReadService.GetByIdAsync(
            id,
            cancellationToken
        );

        if (technology is null)
        {
            return NotFound(ApiResponse.Fail(
                "Technology was not found."
            ));
        }

        return Ok(ApiResponse<TechnologyDto>.Ok(
            technology,
            "Technology retrieved successfully."
        ));
    }
}
