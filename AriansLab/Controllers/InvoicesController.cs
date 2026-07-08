using Application.Common.Models;
using Application.DTOs.Invoices;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/invoices")]
[Authorize]
[Produces("application/json")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceReadService _invoiceReadService;

    public InvoicesController(IInvoiceReadService invoiceReadService)
    {
        _invoiceReadService = invoiceReadService;
    }

    /// <summary>
    /// Gets invoices that belong to the authenticated user.
    /// </summary>
    [HttpGet("my")]
    [ProducesResponseType(typeof(ApiResponse<List<InvoiceListItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<List<InvoiceListItemDto>>>> GetMyInvoices(
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized(ApiResponse.Fail(
                "Authenticated user id was not found."
            ));
        }

        var invoices = await _invoiceReadService.GetMyInvoicesAsync(
            userId.Value,
            cancellationToken
        );

        return Ok(ApiResponse<List<InvoiceListItemDto>>.Ok(
            invoices,
            "Invoices retrieved successfully."
        ));
    }

    /// <summary>
    /// Gets a single invoice that belongs to the authenticated user.
    /// </summary>
    [HttpGet("my/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<InvoiceDetailDto>>> GetMyInvoiceById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized(ApiResponse.Fail(
                "Authenticated user id was not found."
            ));
        }

        var invoice = await _invoiceReadService.GetMyInvoiceByIdAsync(
            userId.Value,
            id,
            cancellationToken
        );

        if (invoice is null)
        {
            return NotFound(ApiResponse.Fail(
                "Invoice was not found."
            ));
        }

        return Ok(ApiResponse<InvoiceDetailDto>.Ok(
            invoice,
            "Invoice retrieved successfully."
        ));
    }

    private Guid? GetCurrentUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdValue))
        {
            return null;
        }

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return null;
        }

        return userId;
    }
}