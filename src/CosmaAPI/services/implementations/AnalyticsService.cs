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

    public async Task<CategoryBreakdownResponseDTO> GetCategoryBreakdownAsync(
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

        var categoryData = await baseQuery
            .GroupBy(x => new 
            { 
                x.CategoryId, 
                CategoryName = x.Category.Name, 
                x.Category.ColorHex, 
                x.Category.Icon })
            .Select(g => new
            {
                g.Key.CategoryId,
                g.Key.CategoryName,
                g.Key.ColorHex,
                g.Key.Icon,
                TotalAmount = g.Sum(x => x.Amount),
                TransactionCount = g.Count()
            })
            .OrderByDescending(x => x.TotalAmount)
            .ToListAsync(cancellationToken);

        var categories = categoryData
            .Select(x => new CategoryBreakdownItemDTO
            {
                CategoryId = x.CategoryId,
                CategoryName = x.CategoryName,
                ColorHex = x.ColorHex,
                Icon = x.Icon,
                TotalAmount = x.TotalAmount,
                PercentageOfTotal = totalSpent == 0 ? 0 : Math.Round((x.TotalAmount / totalSpent) * 100m, 2),
                TransactionCount = x.TransactionCount
            })
            .ToList();
        
        return new CategoryBreakdownResponseDTO
        {
            Year = year,
            Month = month,
            TotalSpent = totalSpent,
            Categories = categories
        };
    }

    public async Task<List<TopExpenseItemDTO>> GetTopExpensesAsync(
        Guid userId,
        int year,
        int month,
        int take,
        CancellationToken cancellationToken = default)
    {
        if (year < 2000 || year > 2100)
        {
            throw new InvalidOperationException("Año fuera del rango permitido.");
        }

        if (month < 1 || month > 12)
        {
            throw new InvalidOperationException("El mes debe estar entre 1 y 12.");
        }

        if (take < 1 || take > 50)
        {
            throw new InvalidOperationException("El número de gastos a mostrar debe estar entre 1 y 50.");
        }

        var startDate = new DateOnly(year, month, 1);
        var endDate = startDate.AddMonths(1);

        return await _dbContext.Expenses
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Date >= startDate && x.Date < endDate)
            .OrderByDescending(x => x.Amount)
            .ThenByDescending(x => x.Date)
            .Select(x => new TopExpenseItemDTO
            {
                ExpenseId = x.Id,
                Amount = x.Amount,
                Date = x.Date,
                Description = x.Description,
                CategoryId = x.CategoryId,
                CategoryName = x.Category.Name,
                IsEssential = x.IsEssential
            })
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<MonthlyTrendItemDTO>> GetMonthlyTrendAsync(
        Guid userId,
        int months,
        CancellationToken cancellationToken = default)
    {
        if (months < 1 || months > 24)
        {
            throw new InvalidOperationException("Los meses deben estar entre 1 y 24.");
        }

        var currentMonthStart = new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var rangeStart = currentMonthStart.AddMonths(-(months - 1));
        var rangeEnd = currentMonthStart.AddMonths(1);

        var groupedData = await _dbContext.Expenses
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Date >= rangeStart && x.Date < rangeEnd)
            .GroupBy(x => new { x.Date.Year, x.Date.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                TotalSpent = g.Sum(x => x.Amount),
                TransactionCount = g.Count()
            })
            .ToListAsync(cancellationToken);

        var groupedLookup = groupedData.ToDictionary(
            x => (x.Year, x.Month),
            x => new
            {
                x.TotalSpent,
                x.TransactionCount
            });

        var result = new List<MonthlyTrendItemDTO>();

        for (var i = 0; i < months; i++)
        {
            var monthDate = rangeStart.AddMonths(i);
            var key = (monthDate.Year, monthDate.Month);

            var current = groupedLookup.TryGetValue(key, out var currentData)
                ? currentData
                : null;

            var totalSpent = current?.TotalSpent ?? 0m;
            var transactionCount = current?.TransactionCount ?? 0;

            decimal differenceFromPreviousMonth = 0m;
            decimal percentageChangeFromPreviousMonth = 0m;

            if (result.Count > 0)
            {
                var previous = result[^1];
                differenceFromPreviousMonth = totalSpent - previous.TotalSpent;

                percentageChangeFromPreviousMonth = previous.TotalSpent == 0m
                    ? 0m
                    : Math.Round((differenceFromPreviousMonth / previous.TotalSpent) * 100m, 2);
            }

            result.Add(new MonthlyTrendItemDTO
            {
                Year = monthDate.Year,
                Month = monthDate.Month,
                Label = $"{monthDate.Year}-{monthDate.Month:D2}",
                TotalSpent = totalSpent,
                TransactionCount = transactionCount,
                DifferenceFromPreviousMonth = differenceFromPreviousMonth,
                PercentageChangeFromPreviousMonth = percentageChangeFromPreviousMonth
            });
        }

        return result;
    }

    public async Task<SavingOpportunitiesResponseDTO> GetSavingOpportunitiesAsync(
        Guid userId,
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        if (year < 2000 || year > 2100)
        {
            throw new InvalidOperationException("El año esta fuera del rango permitido.");
        }

        if (month < 1 || month > 12)
        {
            throw new InvalidOperationException("El mes debe estar entre 1 y 12.");
        }

        var currentMonthStart = new DateOnly(year, month, 1);
        var currentMonthEnd = currentMonthStart.AddMonths(1);

        var previousMonthStart = currentMonthStart.AddMonths(-1);
        var previousMonthEnd = currentMonthStart;

        var currentMonthQuery = _dbContext.Expenses
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Date >= currentMonthStart && x.Date < currentMonthEnd);

        var previousMonthQuery = _dbContext.Expenses
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Date >= previousMonthStart && x.Date < previousMonthEnd);

        var totalSpent = await currentMonthQuery
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        var opportunities = new List<SavingOpportunityItemDTO>();

        if (totalSpent == 0m)
        {
            return new SavingOpportunitiesResponseDTO
            {
                Year = year,
                Month = month,
                TotalSpent = 0m,
                Opportunities = opportunities
            };
        }

        // Rule 1: Top category too dominant
        var topCategory = await currentMonthQuery
            .GroupBy(x => new { x.CategoryId, CategoryName = x.Category.Name })
            .Select(g => new
            {
                g.Key.CategoryId,
                g.Key.CategoryName,
                TotalAmount = g.Sum(x => x.Amount)
            })
            .OrderByDescending(x => x.TotalAmount)
            .FirstOrDefaultAsync(cancellationToken);

        if (topCategory is not null)
        {
            var topCategoryPercentage = Math.Round((topCategory.TotalAmount / totalSpent) * 100m, 2);

            if (topCategoryPercentage >= 30m)
            {
                opportunities.Add(new SavingOpportunityItemDTO
                {
                    Type = "category_share",
                    Severity = topCategoryPercentage >= 40m ? "high" : "medium",
                    Title = "Alta concentración en una categoría",
                    Message = $"La categoría '{topCategory.CategoryName}' representa {topCategoryPercentage}% de su gasto este mes.",
                    Amount = topCategory.TotalAmount,
                    Percentage = topCategoryPercentage
                });
            }
        }

        // Rule 2: Non-essential spending too high
        var nonEssentialTotal = await currentMonthQuery
            .Where(x => !x.IsEssential)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        var nonEssentialPercentage = Math.Round((nonEssentialTotal / totalSpent) * 100m, 2);

        if (nonEssentialPercentage >= 40m)
        {
            opportunities.Add(new SavingOpportunityItemDTO
            {
                Type = "non_essential_share",
                Severity = nonEssentialPercentage >= 50m ? "high" : "medium",
                Title = "Alto gasto en gastos no esenciales",
                Message = $"Sus gastos no esenciales representan {nonEssentialPercentage}% de su gasto total este mes.",
                Amount = nonEssentialTotal,
                Percentage = nonEssentialPercentage
            });
        }

        // Rule 3: Category increase vs previous month
        var currentCategoryTotals = await currentMonthQuery
            .GroupBy(x => new { x.CategoryId, CategoryName = x.Category.Name })
            .Select(g => new
            {
                g.Key.CategoryId,
                g.Key.CategoryName,
                TotalAmount = g.Sum(x => x.Amount)
            })
            .ToListAsync(cancellationToken);

        var previousCategoryTotals = await previousMonthQuery
            .GroupBy(x => x.CategoryId)
            .Select(g => new
            {
                CategoryId = g.Key,
                TotalAmount = g.Sum(x => x.Amount)
            })
            .ToDictionaryAsync(x => x.CategoryId, x => x.TotalAmount, cancellationToken);

        foreach (var category in currentCategoryTotals)
        {
            if (!previousCategoryTotals.TryGetValue(category.CategoryId, out var previousTotal))
            {
                continue;
            }

            if (previousTotal <= 0m)
            {
                continue;
            }

            var difference = category.TotalAmount - previousTotal;
            var percentageIncrease = Math.Round((difference / previousTotal) * 100m, 2);

            if (difference >= 100m && percentageIncrease >= 20m)
            {
                opportunities.Add(new SavingOpportunityItemDTO
                {
                    Type = "category_increase",
                    Severity = percentageIncrease >= 40m ? "high" : "medium",
                    Title = "Categoría con aumento significativo",
                    Message = $"Su gasto en '{category.CategoryName}' aumentó en {percentageIncrease}% comparado con el mes anterior.",
                    Amount = difference,
                    Percentage = percentageIncrease
                });
            }
        }

        // Rule 4: Repeated small non-essential expenses
        var repeatedSmallExpenses = await currentMonthQuery
            .Where(x => !x.IsEssential && x.Amount <= 300m)
            .GroupBy(x => new { x.Description, x.CategoryId, CategoryName = x.Category.Name })
            .Select(g => new
            {
                g.Key.Description,
                g.Key.CategoryId,
                g.Key.CategoryName,
                TransactionCount = g.Count(),
                TotalAmount = g.Sum(x => x.Amount)
            })
            .Where(x => x.TransactionCount >= 3 && x.TotalAmount >= 500m)
            .OrderByDescending(x => x.TotalAmount)
            .Take(3)
            .ToListAsync(cancellationToken);

        foreach (var repeated in repeatedSmallExpenses)
        {
            opportunities.Add(new SavingOpportunityItemDTO
            {
                Type = "repeated_small_expenses",
                Severity = "medium",
                Title = "Gastos pequeños repetidos detectados",
                Message = $"Tuviste {repeated.TransactionCount} gastos pequeños repetidos para '{repeated.Description}' en '{repeated.CategoryName}', sumando un total de {repeated.TotalAmount}.",
                Amount = repeated.TotalAmount
            });
        }

        return new SavingOpportunitiesResponseDTO
        {
            Year = year,
            Month = month,
            TotalSpent = totalSpent,
            Opportunities = opportunities
        };
    }
}