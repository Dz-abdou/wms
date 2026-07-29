using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Receiving;

namespace Warehouse.Infrastructure.Receiving;

public sealed class GoodsReceiptNumberSequenceConfiguration : IEntityTypeConfiguration<GoodsReceiptNumberSequence>
{
    public void Configure(EntityTypeBuilder<GoodsReceiptNumberSequence> builder)
    {
        builder.ToTable("GoodsReceiptNumberSequences"); builder.HasKey(sequence => sequence.Value); builder.Property(sequence => sequence.Value).ValueGeneratedOnAdd(); builder.Property(sequence => sequence.Year).IsRequired();
    }
}
