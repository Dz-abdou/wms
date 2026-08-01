using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Numbering;

namespace Warehouse.Infrastructure.Numbering;

public sealed class DocumentNumberDefinitionConfiguration : IEntityTypeConfiguration<DocumentNumberDefinition>
{
    public void Configure(EntityTypeBuilder<DocumentNumberDefinition> builder)
    {
        builder.ToTable("DocumentNumberDefinitions", table =>
        {
            table.HasCheckConstraint("CK_DocumentNumberDefinitions_Code_NotBlank", "btrim(\"Code\") <> ''");
            table.HasCheckConstraint("CK_DocumentNumberDefinitions_Prefix_NotBlank", "btrim(\"Prefix\") <> ''");
            table.HasCheckConstraint("CK_DocumentNumberDefinitions_DigitCount_Valid", $"\"DigitCount\" BETWEEN {DocumentNumberRules.MinimumDigitCount} AND {DocumentNumberRules.MaximumDigitCount}");
        });
        builder.HasKey(definition => definition.Code);
        builder.Property(definition => definition.Code).HasMaxLength(DocumentNumberRules.MaxCodeLength).IsRequired();
        builder.Property(definition => definition.Description).HasMaxLength(DocumentNumberRules.MaxDescriptionLength).IsRequired();
        builder.Property(definition => definition.Prefix).HasMaxLength(DocumentNumberRules.MaxPrefixLength).IsRequired();
        builder.Property(definition => definition.DigitCount).IsRequired();
        builder.Property(definition => definition.ResetPeriod).HasConversion<int>().IsRequired();
        builder.Property(definition => definition.IsActive).IsRequired();
        builder.Property(definition => definition.AllowsManualEntry).IsRequired();
        builder.HasData(DocumentNumberDefinition.InitialDefinitions);
    }
}
