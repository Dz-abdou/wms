using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Purchasing;

namespace Warehouse.Infrastructure.Purchasing;

public sealed class PurchaseOrderNumberSequenceConfiguration
    : IEntityTypeConfiguration<PurchaseOrderNumberSequence>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderNumberSequence> builder)
    {
        builder.ToTable("PurchaseOrderNumberSequences");
        builder.HasKey(sequence => sequence.Value);
        builder.Property(sequence => sequence.Value)
            .ValueGeneratedOnAdd();
        builder.Property(sequence => sequence.Year).IsRequired();
    }
}
