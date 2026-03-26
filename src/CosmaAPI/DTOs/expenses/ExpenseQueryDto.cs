using CosmaAPI.entities.enums;
using System.ComponentModel.DataAnnotations;
namespace CosmaAPI.DTOs.expenses;

public class ExpenseQueryDTO
{
    public int? Year { get; set; }
    [Range(1, 12)]
    public int? Month { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public Guid? CategoryId { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public bool? IsEssential { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;
    [Range(1, 100)]
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "date";
    public string SortDirection { get; set; } = "desc";
}