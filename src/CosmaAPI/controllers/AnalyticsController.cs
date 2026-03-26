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

    [HttpGet("top-expenses")]
    public async Task<ActionResult<List<TopExpenseItemDTO>>> GetTopExpenses(
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] int take = 5,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();

        var topExpenses = await _analyticsService.GetTopExpensesAsync(
            userId,
            year,
            month,
            take,
            cancellationToken);

        return Ok(topExpenses);
    }

    [HttpGet("monthly-trend")]
    public async Task<ActionResult<List<MonthlyTrendItemDTO>>> GetMonthlyTrend(
        [FromQuery] int months = 6,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();

        var trend = await _analyticsService.GetMonthlyTrendAsync(
            userId,
            months,
            cancellationToken);

        return Ok(trend);
    }

    [HttpGet("saving-opportunities")]
    public async Task<ActionResult<SavingOpportunitiesResponseDTO>> GetSavingOpportunities(
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();

        var opportunities = await _analyticsService.GetSavingOpportunitiesAsync(
            userId,
            year,
            month,
            cancellationToken);

        return Ok(opportunities);
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