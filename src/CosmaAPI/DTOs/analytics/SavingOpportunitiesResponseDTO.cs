namespace CosmaAPI.DTOs.analytics;

public class SavingOpportunitiesResponseDTO
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalSpent { get; set; }
    public List<SavingOpportunityItemDTO> Opportunities { get; set; } = new();
}