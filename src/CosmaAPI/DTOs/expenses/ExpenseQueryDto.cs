using CosmaAPI.entities.enums;
namespace CosmaAPI.DTOs.expenses;

public class ExpenseQueryDTO
{
    public int? Year { get; set; }
    public int? Month { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public Guid? CategoryId { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public bool? IsEssential { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
}