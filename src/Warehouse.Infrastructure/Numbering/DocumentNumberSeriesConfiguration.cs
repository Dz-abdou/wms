using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Numbering;

namespace Warehouse.Infrastructure.Numbering;

public sealed class DocumentNumberSeriesConfiguration : IEntityTypeConfiguration<DocumentNumberSeries>
{
    public void Configure(EntityTypeBuilder<DocumentNumberSeries> builder)
    {
        builder.ToTable("DocumentNumberSeries", table =>
        {
            table.HasCheckConstraint("CK_DocumentNumberSeries_Year_Valid", "\"Year\" BETWEEN 2000 AND 9999");
            table.HasCheckConstraint("CK_DocumentNumberSeries_NextValue_Positive", "\"NextValue\" > 0");
        });
        builder.HasKey(series => series.Id);
        builder.Property(series => series.Id).ValueGeneratedNever();
        builder.Property(series => series.DefinitionCode).HasMaxLength(DocumentNumberRules.MaxCodeLength).IsRequired();
        builder.Property(series => series.Year).IsRequired();
        builder.Property(series => series.NextValue).IsRequired();
        builder.HasIndex(series => new { series.DefinitionCode, series.Year }).IsUnique();
        builder.HasOne<DocumentNumberDefinition>().WithMany().HasForeignKey(series => series.DefinitionCode).OnDelete(DeleteBehavior.Restrict);
    }
}
