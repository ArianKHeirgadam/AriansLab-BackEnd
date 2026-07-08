using Application.Common.Models;
using Application.DTOs.Services;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/admin/services")]
[Authorize(Roles = "Admin")]
public class AdminServicesController : ControllerBase
{
    private readonly IServiceAdminService _serviceAdminService;

    public AdminServicesController(IServiceAdminService serviceAdminService)
    {
        _serviceAdminService = serviceAdminService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AdminServiceDto>>>> GetAll(
        CancellationToken cancellationToken)
    {
        var services = await _serviceAdminService.GetAllAsync(cancellationToken);

        return Ok(ApiResponse<List<AdminServiceDto>>.Ok(
            services,
            "Admin services retrieved successfully."
        ));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AdminServiceDto>>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var service = await _serviceAdminService.GetByIdAsync(
            id,
            cancellationToken
        );

        if (service is null)
        {
            return NotFound(ApiResponse.Fail(
                "Service was not found."
            ));
        }

        return Ok(ApiResponse<AdminServiceDto>.Ok(
            service,
            "Admin service retrieved successfully."
        ));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AdminServiceDto>>> Create(
        [FromBody] CreateServiceRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var service = await _serviceAdminService.CreateAsync(
                request,
                cancellationToken
            );

            return CreatedAtAction(
                nameof(GetById),
                new { id = service.Id },
                ApiResponse<AdminServiceDto>.Ok(
                    service,
                    "Service created successfully."
                )
            );
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AdminServiceDto>>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateServiceRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var service = await _serviceAdminService.UpdateAsync(
                id,
                request,
                cancellationToken
            );

            if (service is null)
            {
                return NotFound(ApiResponse.Fail(
                    "Service was not found."
                ));
            }

            return Ok(ApiResponse<AdminServiceDto>.Ok(
                service,
                "Service updated successfully."
            ));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse>> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await _serviceAdminService.DeleteAsync(
            id,
            cancellationToken
        );

        if (!deleted)
        {
            return NotFound(ApiResponse.Fail(
                "Service was not found."
            ));
        }

        return Ok(ApiResponse.Ok(
            "Service deleted successfully."
        ));
    }
}