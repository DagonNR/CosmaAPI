namespace CosmaAPI.entities;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public DateTime CreatedAtUtc { get; set; }

    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}