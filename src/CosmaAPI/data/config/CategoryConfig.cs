using CosmaAPI.data.seed;
using CosmaAPI.entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CosmaAPI.data.config;

public class CategoryConfig : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.Name).IsUnique();
        builder.Property(x => x.ColorHex).HasMaxLength(20);
        builder.Property(x => x.Icon).HasMaxLength(50);
        builder.Property(x => x.IsActive).IsRequired();

        builder.HasData(CategorySeedData.GetSeedData());
    }
}