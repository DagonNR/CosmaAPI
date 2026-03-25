namespace CosmaAPI.DTOs.analytics;

public class CategoryBreakdownItemDTO
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? ColorHex { get; set; }
    public string? Icon { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PercentageOfTotal { get; set; }
    public int TransactionCount { get; set; }
}