using Application.Common.Models;
using Application.DTOs.Pricing;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/pricing")]
[AllowAnonymous]
public class PricingController : ControllerBase
{
    private readonly IPricingReadService _pricingReadService;

    public PricingController(IPricingReadService pricingReadService)
    {
        _pricingReadService = pricingReadService;
    }

    [HttpGet("plans")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PricingPlanDto>>>> GetPlans(
        CancellationToken cancellationToken)
    {
        var result = await _pricingReadService.GetActivePlansAsync(cancellationToken);

        return Ok(ApiResponse<IReadOnlyList<PricingPlanDto>>.Ok(
            result,
            "Pricing plans retrieved successfully."));
    }

    [HttpGet("plans/{id:guid}")]
    public async Task<ActionResult<ApiResponse<PricingPlanDto>>> GetPlanById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _pricingReadService.GetPlanByIdAsync(id, cancellationToken);

        if (result is null)
        {
            return NotFound(ApiResponse<PricingPlanDto>.Fail(
                "Pricing plan was not found."));
        }

        return Ok(ApiResponse<PricingPlanDto>.Ok(
            result,
            "Pricing plan retrieved successfully."));
    }
}