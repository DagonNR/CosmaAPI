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
}