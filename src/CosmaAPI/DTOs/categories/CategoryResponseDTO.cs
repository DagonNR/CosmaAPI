namespace CosmaAPI.DTOs.categories;

public class CategoryResponseDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ColorHex { get; set; }
    public string? Icon { get; set; }
}