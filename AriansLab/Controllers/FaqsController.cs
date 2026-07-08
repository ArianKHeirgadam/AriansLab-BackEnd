using Application.Common.Models;
using Application.DTOs.FAQs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/faqs")]
[AllowAnonymous]
[Produces("application/json")]
public class FaqsController : ControllerBase
{
    private readonly IFaqReadService _faqReadService;

    public FaqsController(IFaqReadService faqReadService)
    {
        _faqReadService = faqReadService;
    }

    /// <summary>
    /// Gets active FAQs for public website pages.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<FaqDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<FaqDto>>>> GetActiveFaqs(
        CancellationToken cancellationToken)
    {
        var faqs = await _faqReadService.GetActiveFaqsAsync(cancellationToken);

        return Ok(ApiResponse<List<FaqDto>>.Ok(
            faqs,
            "FAQs retrieved successfully."
        ));
    }
}