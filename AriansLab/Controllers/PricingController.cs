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
    public async Task<ActionResult<ApiResponse<List<PricingPlanDto>>>> GetActivePlans(
        CancellationToken cancellationToken)
    {
        var plans = await _pricingReadService.GetActivePlansAsync(cancellationToken);

        return Ok(ApiResponse<List<PricingPlanDto>>.Ok(
            plans,
            "Pricing plans retrieved successfully."
        ));
    }

    [HttpGet("plans/{id:guid}")]
    public async Task<ActionResult<ApiResponse<PricingPlanDto>>> GetPlanById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var plan = await _pricingReadService.GetPlanByIdAsync(id, cancellationToken);

        if (plan is null)
        {
            return NotFound(ApiResponse.Fail(
                "Pricing plan was not found."
            ));
        }

        return Ok(ApiResponse<PricingPlanDto>.Ok(
            plan,
            "Pricing plan retrieved successfully."
        ));
    }
}