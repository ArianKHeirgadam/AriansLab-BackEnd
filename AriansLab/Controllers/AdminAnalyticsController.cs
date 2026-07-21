using Application.Common.Models;
using Application.DTOs.Analytics;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/admin/analytics")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public sealed class AdminAnalyticsController : ControllerBase
{
    private readonly IAnalyticsReadService _analyticsReadService;

    public AdminAnalyticsController(IAnalyticsReadService analyticsReadService)
    {
        _analyticsReadService = analyticsReadService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<AnalyticsDashboardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<AnalyticsDashboardDto>>> GetDashboard(
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        var dashboard = await _analyticsReadService.GetDashboardAsync(days, cancellationToken);
        return Ok(ApiResponse<AnalyticsDashboardDto>.Ok(
            dashboard,
            "Analytics dashboard retrieved successfully."));
    }
}
