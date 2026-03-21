using CosmaAPI.data;
using CosmaAPI.DTOs.analytics;
using CosmaAPI.services.interfaces;
using Microsoft.EntityFrameworkCore;

namespace CosmaAPI.Services.Implementations;

public class AnalyticsService : IAnalyticsService
{
    private readonly ApplicationDbContext _dbContext;

    public AnalyticsService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MonthlySummaryDTO> GetMonthlySummaryAsync(
        Guid userId,
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        if (year < 2000 || year > 2100)
        {
            throw new InvalidOperationException("El año está fuera del rango permitido.");
        }

        if (month < 1 || month > 12)
        {
            throw new InvalidOperationException("El mes debe estar entre 1 y 12.");
        }

        var startDate = new DateOnly(year, month, 1);
        var endDate = startDate.AddMonths(1);

        var baseQuery = _dbContext.Expenses
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Date >= startDate && x.Date < endDate);

        var totalSpent = await baseQuery
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        var transactionCount = await baseQuery
            .CountAsync(cancellationToken);

        var essentialTotal = await baseQuery
            .Where(x => x.IsEssential)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        var nonEssentialTotal = await baseQuery
            .Where(x => !x.IsEssential)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        var topCategory = await baseQuery
            .GroupBy(x => new { x.CategoryId, CategoryName = x.Category.Name })
            .Select(g => new
            {
                g.Key.CategoryId,
                g.Key.CategoryName,
                Total = g.Sum(x => x.Amount)
            })
            .OrderByDescending(x => x.Total)
            .FirstOrDefaultAsync(cancellationToken);

        var daysInMonth = DateTime.DaysInMonth(year, month);

        var averagePerDay = daysInMonth == 0
            ? 0m
            : Math.Round(totalSpent / daysInMonth, 2);

        var essentialPercentage = totalSpent == 0
            ? 0m
            : Math.Round((essentialTotal / totalSpent) * 100m, 2);

        var nonEssentialPercentage = totalSpent == 0
            ? 0m
            : Math.Round((nonEssentialTotal / totalSpent) * 100m, 2);

        return new MonthlySummaryDTO
        {
            Year = year,
            Month = month,
            TotalSpent = totalSpent,
            TransactionCount = transactionCount,
            TopCategoryId = topCategory?.CategoryId,
            TopCategoryName = topCategory?.CategoryName,
            TopCategoryAmount = topCategory?.Total ?? 0m,
            AveragePerDay = averagePerDay,
            EssentialTotal = essentialTotal,
            NonEssentialTotal = nonEssentialTotal,
            EssentialPercentage = essentialPercentage,
            NonEssentialPercentage = nonEssentialPercentage
        };
    }
}