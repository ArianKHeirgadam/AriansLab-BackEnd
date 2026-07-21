using Application.DTOs.Analytics;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/analytics")]
[Produces("application/json")]
public sealed class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsWriteService _analyticsWriteService;

    public AnalyticsController(IAnalyticsWriteService analyticsWriteService)
    {
        _analyticsWriteService = analyticsWriteService;
    }

    [HttpPost("page-view")]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    [EnableRateLimiting("analytics")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> TrackPageView(
        [FromBody] TrackPageViewRequestDto request,
        CancellationToken cancellationToken)
    {
        await _analyticsWriteService.TrackPageViewAsync(
            request,
            Request.Headers.UserAgent.FirstOrDefault(),
            cancellationToken);

        return Accepted();
    }
}
