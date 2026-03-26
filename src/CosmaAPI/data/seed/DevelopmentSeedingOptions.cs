namespace CosmaAPI.data.seed;

public class DevelopmentSeedingOptions
{
    public const string SectionName = "DevelopmentSeeding";
    public bool Enabled { get; set; }
    public int ExpensesCount { get; set; }
    public bool ResetExisting { get; set; }
}