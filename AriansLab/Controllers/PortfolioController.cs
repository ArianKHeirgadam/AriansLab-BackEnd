using Application.Common.Models;
using Application.DTOs.Common;
using Application.DTOs.Portfolio;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/portfolio")]
[AllowAnonymous]
public class PortfolioController : ControllerBase
{
    private readonly IPortfolioReadService _portfolioReadService;

    public PortfolioController(IPortfolioReadService portfolioReadService)
    {
        _portfolioReadService = portfolioReadService;
    }

    [HttpGet("items")]
    public async Task<ActionResult<ApiResponse<PagedResultDto<PortfolioListItemDto>>>> GetPortfolios(
        [FromQuery] PortfolioQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _portfolioReadService.GetPortfoliosAsync(
            parameters,
            cancellationToken);

        return Ok(ApiResponse<PagedResultDto<PortfolioListItemDto>>.Ok(
            result,
            "Portfolio items retrieved successfully."));
    }

    [HttpGet("items/{slug}")]
    public async Task<ActionResult<ApiResponse<PortfolioDetailDto>>> GetPortfolioBySlug(
        [FromRoute] string slug,
        CancellationToken cancellationToken)
    {
        var result = await _portfolioReadService.GetPortfolioBySlugAsync(
            slug,
            cancellationToken);

        if (result is null)
        {
            return NotFound(ApiResponse<PortfolioDetailDto>.Fail(
                "Portfolio item was not found."));
        }

        return Ok(ApiResponse<PortfolioDetailDto>.Ok(
            result,
            "Portfolio item retrieved successfully."));
    }

    [HttpGet("categories")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PortfolioCategoryDto>>>> GetCategories(
        CancellationToken cancellationToken)
    {
        var result = await _portfolioReadService.GetCategoriesAsync(cancellationToken);

        return Ok(ApiResponse<IReadOnlyList<PortfolioCategoryDto>>.Ok(
            result,
            "Portfolio categories retrieved successfully."));
    }
}