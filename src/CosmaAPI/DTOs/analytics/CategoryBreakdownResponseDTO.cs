using System.ComponentModel;

namespace CosmaAPI.DTOs.analytics;

public class CategoryBreakdownResponseDTO
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalSpent { get; set; }
    public List<CategoryBreakdownItemDTO> Categories { get; set; } = new();
}