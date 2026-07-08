using Application.Common.Models;
using Application.DTOs.Invoices;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/admin/invoices")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminInvoicesController : ControllerBase
{
    private readonly IInvoiceAdminService _invoiceAdminService;

    public AdminInvoicesController(IInvoiceAdminService invoiceAdminService)
    {
        _invoiceAdminService = invoiceAdminService;
    }

    /// <summary>
    /// Gets all invoices for admin panel.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<InvoiceDetailDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<InvoiceDetailDto>>>> GetAll(
        CancellationToken cancellationToken)
    {
        var invoices = await _invoiceAdminService.GetAllAsync(cancellationToken);

        return Ok(ApiResponse<List<InvoiceDetailDto>>.Ok(
            invoices,
            "Invoices retrieved successfully."
        ));
    }

    /// <summary>
    /// Gets a single invoice by id for admin panel.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<InvoiceDetailDto>>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var invoice = await _invoiceAdminService.GetByIdAsync(
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

    /// <summary>
    /// Creates a new invoice for a customer project.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDetailDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<InvoiceDetailDto>>> Create(
        [FromBody] CreateInvoiceRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var invoice = await _invoiceAdminService.CreateAsync(
                request,
                cancellationToken
            );

            return CreatedAtAction(
                nameof(GetById),
                new { id = invoice.Id },
                ApiResponse<InvoiceDetailDto>.Ok(
                    invoice,
                    "Invoice created successfully."
                )
            );
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Updates an existing invoice.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<InvoiceDetailDto>>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateInvoiceRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var invoice = await _invoiceAdminService.UpdateAsync(
                id,
                request,
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
                "Invoice updated successfully."
            ));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Updates invoice payment status.
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<InvoiceDetailDto>>> UpdateStatus(
        [FromRoute] Guid id,
        [FromBody] UpdateInvoiceStatusRequestDto request,
        CancellationToken cancellationToken)
    {
        var invoice = await _invoiceAdminService.UpdateStatusAsync(
            id,
            request,
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
            "Invoice status updated successfully."
        ));
    }

    /// <summary>
    /// Soft deletes an existing invoice.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await _invoiceAdminService.DeleteAsync(
            id,
            cancellationToken
        );

        if (!deleted)
        {
            return NotFound(ApiResponse.Fail(
                "Invoice was not found."
            ));
        }

        return Ok(ApiResponse.Ok(
            "Invoice deleted successfully."
        ));
    }
}