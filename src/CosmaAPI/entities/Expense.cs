using CosmaAPI.entities.enums;
namespace CosmaAPI.entities;

public class Expense
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public DateOnly Date { get; set; }
    public string Description { get; set; } = default!;
    public bool IsEssential { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = default!;
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}