using CosmaAPI.entities.enums;
namespace CosmaAPI.DTOs.expenses;

public class ExpenseResponseDTO
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public DateOnly Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsEssential { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}