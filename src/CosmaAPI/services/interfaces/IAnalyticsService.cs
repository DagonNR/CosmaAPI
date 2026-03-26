using CosmaAPI.DTOs.analytics;
namespace CosmaAPI.services.interfaces;

public interface IAnalyticsService
{
    Task<MonthlySummaryDTO> GetMonthlySummaryAsync(
        Guid userId, 
        int year, 
        int month,
        CancellationToken cancellationToken = default
        );
    
    Task<CategoryBreakdownResponseDTO> GetCategoryBreakdownAsync(
        Guid userId,
        int year,
        int month,
        CancellationToken cancellationToken = default
        );

    Task<List<TopExpenseItemDTO>> GetTopExpensesAsync(
        Guid userId,
        int year,
        int month,
        int take,
        CancellationToken cancellationToken = default
        );
    
    Task<List<MonthlyTrendItemDTO>> GetMonthlyTrendAsync(
        Guid userId,
        int months,
        CancellationToken cancellationToken = default
        );

    Task<SavingOpportunitiesResponseDTO> GetSavingOpportunitiesAsync(
        Guid userId,
        int year,
        int month,
        CancellationToken cancellationToken = default
        );
}