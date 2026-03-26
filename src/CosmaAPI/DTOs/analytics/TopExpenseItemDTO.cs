using Bogus.DataSets;

namespace CosmaAPI.DTOs.analytics;

public class TopExpenseItemDTO
{
    public Guid ExpenseId { get; set; }
    public decimal Amount { get; set; }
    public DateOnly Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public bool IsEssential { get; set; }
}