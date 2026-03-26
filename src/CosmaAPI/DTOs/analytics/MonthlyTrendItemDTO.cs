namespace CosmaAPI.DTOs.analytics;

public class MonthlyTrendItemDTO
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string Label { get; set; } = string.Empty;
    public decimal TotalSpent { get; set; }
    public int TransactionCount { get; set; }
    public decimal DifferenceFromPreviousMonth { get; set; }
    public decimal PercentageChangeFromPreviousMonth { get; set; }
}