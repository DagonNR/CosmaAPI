using System.Security.Claims;
using CosmaAPI.DTOs.analytics;
using CosmaAPI.services.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CosmaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("monthly-summary")]
    public async Task<ActionResult<MonthlySummaryDTO>> GetMonthlySummary(
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        var summary = await _analyticsService.GetMonthlySummaryAsync(
            userId,
            year,
            month,
            cancellationToken);

        return Ok(summary);
    }

    [HttpGet("category-breakdown")]
    public async Task<ActionResult<CategoryBreakdownResponseDTO>> GetCategoryBreakdown(
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        var breakdown = await _analyticsService.GetCategoryBreakdownAsync(
            userId,
            year,
            month,
            cancellationToken);

        return Ok(breakdown);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Token inválido.");
        }

        return userId;
    }
}