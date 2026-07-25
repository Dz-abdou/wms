using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Currencies;

namespace Warehouse.Infrastructure.Currencies;

public sealed class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
    private static readonly DateTime SeededAtUtc = new(2026, 7, 25, 0, 0, 0, DateTimeKind.Utc);

    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.ToTable("Currencies", table =>
        {
            table.HasCheckConstraint("CK_Currencies_Code_Uppercase", "\"Code\" = upper(\"Code\")");
            table.HasCheckConstraint("CK_Currencies_Code_NotBlank", "btrim(\"Code\") <> ''");
            table.HasCheckConstraint("CK_Currencies_Name_NotBlank", "btrim(\"Name\") <> ''");
            table.HasCheckConstraint("CK_Currencies_DecimalPlaces_Valid", "\"DecimalPlaces\" BETWEEN 0 AND 4");
            table.HasCheckConstraint("CK_Currencies_Default_Active", "NOT \"IsDefault\" OR \"IsActive\"");
        });
        builder.HasKey(currency => currency.Id);
        builder.Property(currency => currency.Code).HasMaxLength(CurrencyRules.CodeLength).IsRequired();
        builder.Property(currency => currency.Name).HasMaxLength(CurrencyRules.MaxNameLength).IsRequired();
        builder.Property(currency => currency.Symbol).HasMaxLength(CurrencyRules.MaxSymbolLength);
        builder.Property(currency => currency.DecimalPlaces).IsRequired();
        builder.Property(currency => currency.IsActive).HasDefaultValue(true).IsRequired();
        builder.Property(currency => currency.IsDefault).HasDefaultValue(false).IsRequired();
        builder.Property(currency => currency.CreatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(currency => currency.UpdatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(currency => currency.CreatedByUserId).HasColumnType("uuid");
        builder.Property(currency => currency.UpdatedByUserId).HasColumnType("uuid");
        builder.HasIndex(currency => currency.Code).IsUnique().HasDatabaseName("UX_Currencies_Code");
        builder.HasIndex(currency => currency.IsDefault).IsUnique().HasFilter("\"IsDefault\" = true").HasDatabaseName("UX_Currencies_OneDefault");
        builder.HasData(
            Seed("49f755f8-c6cd-4b22-8615-083b0d5536f2", "DZD", "Algerian dinar", "DA", true),
            Seed("d3fa10b2-a7fc-4a75-a5f6-1d2a8efc1d96", "EUR", "Euro", "€", false),
            Seed("6b5c1ad6-f3a3-48e5-8222-4ca8b16a44ce", "USD", "US dollar", "$", false));
    }

    private static object Seed(string id, string code, string name, string symbol, bool isDefault) => new { Id = Guid.Parse(id), Code = code, Name = name, Symbol = symbol, DecimalPlaces = 2, IsActive = true, IsDefault = isDefault, CreatedAtUtc = SeededAtUtc, UpdatedAtUtc = SeededAtUtc, CreatedByUserId = (Guid?)null, UpdatedByUserId = (Guid?)null };
}
