namespace CosmaAPI.DTOs.analytics;

public class MonthlySummaryDTO
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalSpent { get; set; }
    public int TransactionCount { get; set; }
    public Guid? TopCategoryId { get; set; }
    public string? TopCategoryName { get; set; }
    public decimal? TopCategoryAmount { get; set; }
    public decimal AveragePerDay { get; set; }
    public decimal EssentialTotal { get; set; }
    public decimal NonEssentialTotal { get; set; }
    public decimal EssentialPercentage { get; set; }
    public decimal NonEssentialPercentage { get; set; }
}