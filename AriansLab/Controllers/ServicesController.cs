using Application.Common.Models;
using Application.DTOs.Services;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/services")]
[AllowAnonymous]
public class ServicesController : ControllerBase
{
    private readonly IServiceReadService _serviceReadService;

    public ServicesController(IServiceReadService serviceReadService)
    {
        _serviceReadService = serviceReadService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ServiceListItemDto>>>> GetServices(
        CancellationToken cancellationToken)
    {
        var services = await _serviceReadService.GetServicesAsync(cancellationToken);

        return Ok(ApiResponse<List<ServiceListItemDto>>.Ok(
            services,
            "Services retrieved successfully."
        ));
    }

    [HttpGet("featured")]
    public async Task<ActionResult<ApiResponse<List<ServiceListItemDto>>>> GetFeaturedServices(
        CancellationToken cancellationToken)
    {
        var services = await _serviceReadService.GetFeaturedServicesAsync(cancellationToken);

        return Ok(ApiResponse<List<ServiceListItemDto>>.Ok(
            services,
            "Featured services retrieved successfully."
        ));
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<ApiResponse<ServiceDetailDto>>> GetServiceBySlug(
        [FromRoute] string slug,
        CancellationToken cancellationToken)
    {
        var service = await _serviceReadService.GetServiceBySlugAsync(
            slug,
            cancellationToken
        );

        if (service is null)
        {
            return NotFound(ApiResponse.Fail(
                "Service was not found."
            ));
        }

        return Ok(ApiResponse<ServiceDetailDto>.Ok(
            service,
            "Service retrieved successfully."
        ));
    }
}