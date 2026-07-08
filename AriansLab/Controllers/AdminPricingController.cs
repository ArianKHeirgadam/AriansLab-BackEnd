using Application.Common.Models;
using Application.DTOs.Pricing;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/admin/pricing")]
[Authorize(Roles = "Admin")]
public class AdminPricingController : ControllerBase
{
    private readonly IPricingAdminService _pricingAdminService;

    public AdminPricingController(IPricingAdminService pricingAdminService)
    {
        _pricingAdminService = pricingAdminService;
    }

    [HttpGet("plans")]
    public async Task<ActionResult<ApiResponse<List<AdminPricingPlanDto>>>> GetAll(
        CancellationToken cancellationToken)
    {
        var plans = await _pricingAdminService.GetAllAsync(cancellationToken);

        return Ok(ApiResponse<List<AdminPricingPlanDto>>.Ok(
            plans,
            "Admin pricing plans retrieved successfully."
        ));
    }

    [HttpGet("plans/{id:guid}")]
    public async Task<ActionResult<ApiResponse<AdminPricingPlanDto>>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var plan = await _pricingAdminService.GetByIdAsync(
            id,
            cancellationToken
        );

        if (plan is null)
        {
            return NotFound(ApiResponse.Fail(
                "Pricing plan was not found."
            ));
        }

        return Ok(ApiResponse<AdminPricingPlanDto>.Ok(
            plan,
            "Admin pricing plan retrieved successfully."
        ));
    }

    [HttpPost("plans")]
    public async Task<ActionResult<ApiResponse<AdminPricingPlanDto>>> Create(
        [FromBody] CreatePricingPlanRequestDto request,
        CancellationToken cancellationToken)
    {
        var plan = await _pricingAdminService.CreateAsync(
            request,
            cancellationToken
        );

        return CreatedAtAction(
            nameof(GetById),
            new { id = plan.Id },
            ApiResponse<AdminPricingPlanDto>.Ok(
                plan,
                "Pricing plan created successfully."
            )
        );
    }

    [HttpPut("plans/{id:guid}")]
    public async Task<ActionResult<ApiResponse<AdminPricingPlanDto>>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdatePricingPlanRequestDto request,
        CancellationToken cancellationToken)
    {
        var plan = await _pricingAdminService.UpdateAsync(
            id,
            request,
            cancellationToken
        );

        if (plan is null)
        {
            return NotFound(ApiResponse.Fail(
                "Pricing plan was not found."
            ));
        }

        return Ok(ApiResponse<AdminPricingPlanDto>.Ok(
            plan,
            "Pricing plan updated successfully."
        ));
    }

    [HttpDelete("plans/{id:guid}")]
    public async Task<ActionResult<ApiResponse>> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await _pricingAdminService.DeleteAsync(
            id,
            cancellationToken
        );

        if (!deleted)
        {
            return NotFound(ApiResponse.Fail(
                "Pricing plan was not found."
            ));
        }

        return Ok(ApiResponse.Ok(
            "Pricing plan deleted successfully."
        ));
    }
}