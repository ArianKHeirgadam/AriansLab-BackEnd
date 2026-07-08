using Application.Common.Models;
using Application.DTOs.Payments;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
[Produces("application/json")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentReadService _paymentReadService;

    public PaymentsController(IPaymentReadService paymentReadService)
    {
        _paymentReadService = paymentReadService;
    }

    /// <summary>
    /// Gets payments that belong to the authenticated user.
    /// </summary>
    [HttpGet("my")]
    [ProducesResponseType(typeof(ApiResponse<List<PaymentListItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<List<PaymentListItemDto>>>> GetMyPayments(
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized(ApiResponse.Fail(
                "Authenticated user id was not found."
            ));
        }

        var payments = await _paymentReadService.GetMyPaymentsAsync(
            userId.Value,
            cancellationToken
        );

        return Ok(ApiResponse<List<PaymentListItemDto>>.Ok(
            payments,
            "Payments retrieved successfully."
        ));
    }

    /// <summary>
    /// Gets a single payment that belongs to the authenticated user.
    /// </summary>
    [HttpGet("my/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PaymentDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PaymentDetailDto>>> GetMyPaymentById(
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

        var payment = await _paymentReadService.GetMyPaymentByIdAsync(
            userId.Value,
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