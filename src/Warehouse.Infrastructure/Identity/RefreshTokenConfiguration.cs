using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Warehouse.Infrastructure.Identity;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(token => token.Id);
        builder.Property(token => token.TokenHash).HasMaxLength(128).IsRequired();
        builder.Property(token => token.CreatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(token => token.ExpiresAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(token => token.RevokedAtUtc).HasColumnType("timestamp with time zone");
        builder.HasIndex(token => token.TokenHash).IsUnique();
        builder.HasIndex(token => new { token.UserId, token.ExpiresAtUtc });
        builder.HasOne(token => token.User).WithMany(user => user.RefreshTokens).HasForeignKey(token => token.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}