using CosmaAPI.entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CosmaAPI.data.config;

public class ExpenseConfig : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("expenses", table =>
        {
            table.HasCheckConstraint("CK_expenses_amount_gt_zero", "\"Amount\" > 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.Description).HasMaxLength(150).IsRequired();
        builder.Property(x => x.IsEssential).IsRequired();
        builder.Property(x => x.PaymentMethod).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.Date).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc);
        builder.HasOne(x => x.User).WithMany(x => x.Expenses).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Category).WithMany(x => x.Expenses).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.UserId, x.Date });
        builder.HasIndex(x => new { x.UserId, x.CategoryId });
        builder.HasIndex(x => new { x.UserId, x.PaymentMethod });
        builder.HasIndex(x => new { x.UserId, x.IsEssential });
    }
}