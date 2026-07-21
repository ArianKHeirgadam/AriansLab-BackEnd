using Application.Common.Models;
using Application.DTOs.Payments;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/admin/payments")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminPaymentsController : ControllerBase
{
    private readonly IPaymentAdminService _paymentAdminService;

    public AdminPaymentsController(IPaymentAdminService paymentAdminService)
    {
        _paymentAdminService = paymentAdminService;
    }

    /// <summary>
    /// Gets all payments for admin panel.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<PaymentDetailDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<PaymentDetailDto>>>> GetAll(
        CancellationToken cancellationToken)
    {
        var payments = await _paymentAdminService.GetAllAsync(cancellationToken);

        return Ok(ApiResponse<List<PaymentDetailDto>>.Ok(
            payments,
            "Payments retrieved successfully."
        ));
    }

    /// <summary>
    /// Gets a single payment by id for admin panel.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PaymentDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PaymentDetailDto>>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var payment = await _paymentAdminService.GetByIdAsync(
            id,
            cancellationToken
        );

        if (payment is null)
        {
            return NotFound(ApiResponse.Fail(
                "Payment was not found."
            ));
        }

        return Ok(ApiResponse<PaymentDetailDto>.Ok(
            payment,
            "Payment retrieved successfully."
        ));
    }

    /// <summary>
    /// Creates a new payment record for an invoice.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PaymentDetailDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<PaymentDetailDto>>> Create(
        [FromBody] CreatePaymentRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var payment = await _paymentAdminService.CreateAsync(
                request,
                cancellationToken
            );

            return CreatedAtAction(
                nameof(GetById),
                new { id = payment.Id },
                ApiResponse<PaymentDetailDto>.Ok(
                    payment,
                    "Payment created successfully."
                )
            );
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Updates an existing payment record.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PaymentDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PaymentDetailDto>>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdatePaymentRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var payment = await _paymentAdminService.UpdateAsync(
                id,
                request,
                cancellationToken
            );

            if (payment is null)
            {
                return NotFound(ApiResponse.Fail(
                    "Payment was not found."
                ));
            }

            return Ok(ApiResponse<PaymentDetailDto>.Ok(
                payment,
                "Payment updated successfully."
            ));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Updates payment status.
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(ApiResponse<PaymentDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PaymentDetailDto>>> UpdateStatus(
        [FromRoute] Guid id,
        [FromBody] UpdatePaymentStatusRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var payment = await _paymentAdminService.UpdateStatusAsync(
                id,
                request,
                cancellationToken
            );

            if (payment is null)
            {
                return NotFound(ApiResponse.Fail(
                    "Payment was not found."
                ));
            }

            return Ok(ApiResponse<PaymentDetailDto>.Ok(
                payment,
                "Payment status updated successfully."
            ));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(ApiResponse.Fail(exception.Message));
        }
    }

    /// <summary>
    /// Soft deletes an existing payment.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _paymentAdminService.DeleteAsync(
                id,
                cancellationToken
            );

            if (!deleted)
            {
                return NotFound(ApiResponse.Fail(
                    "Payment was not found."
                ));
            }

            return Ok(ApiResponse.Ok(
                "Payment deleted successfully."
            ));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(ApiResponse.Fail(exception.Message));
        }
    }
}
