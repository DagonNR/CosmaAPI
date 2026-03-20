using System.ComponentModel.DataAnnotations;
using CosmaAPI.entities.enums;
namespace CosmaAPI.DTOs.expenses;
public class UpdateExpenseRequestDTO
{
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor que cero.")]
    public decimal Amount { get; set; }
    [Required]
    public DateOnly Date { get; set; }
    [Required]
    [StringLength(250, MinimumLength = 2, ErrorMessage = "La categoría no puede tener más de 250 caracteres.")]
    public string Description { get; set; } = string.Empty;
    [Required]
    public bool IsEssential { get; set; }
    [Required]
    public PaymentMethod PaymentMethod { get; set; }
    [Required]
    public Guid CategoryId { get; set; }
}
