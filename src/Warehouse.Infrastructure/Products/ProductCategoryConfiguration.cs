using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Products;

namespace Warehouse.Infrastructure.Products;

public sealed class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.ToTable("ProductCategories", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_ProductCategories_Code_NotBlank", "btrim(\"Code\") <> ''");
            tableBuilder.HasCheckConstraint("CK_ProductCategories_Name_NotBlank", "btrim(\"Name\") <> ''");
            tableBuilder.HasCheckConstraint("CK_ProductCategories_Code_Uppercase", "\"Code\" = upper(\"Code\")");
        });

        builder.HasKey(category => category.Id);
        builder.Property(category => category.Code).HasMaxLength(ProductCategoryRules.MaxCodeLength).IsRequired();
        builder.Property(category => category.Name).HasMaxLength(ProductCategoryRules.MaxNameLength).IsRequired();
        builder.Property(category => category.ParentCategoryId).HasColumnType("uuid");
        builder.Property(category => category.CreatedByUserId).HasColumnType("uuid");
        builder.Property(category => category.UpdatedByUserId).HasColumnType("uuid");
        builder.Property(category => category.CreatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(category => category.UpdatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.HasIndex(category => category.Code).IsUnique().HasDatabaseName("UX_ProductCategories_Code");
        builder.HasIndex(category => category.ParentCategoryId);
        builder.HasOne<ProductCategory>()
            .WithMany()
            .HasForeignKey(category => category.ParentCategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

