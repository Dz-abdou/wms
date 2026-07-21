using Warehouse.Domain.Products;

namespace Warehouse.UnitTests.Products;

public sealed class ProductTests
{
    private static readonly DateTime CreatedAtUtc = new(2026, 7, 20, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_normalizes_sku_and_trims_supported_text_fields()
    {
        var product = Product.Create(" sku-001 ", " Sample product ", " Description ", CreatedAtUtc);

        Assert.Equal("SKU-001", product.Sku);
        Assert.Equal("Sample product", product.Name);
        Assert.Equal("Description", product.Description);
        Assert.True(product.IsActive);
        Assert.Equal(CreatedAtUtc, product.CreatedAtUtc);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_blank_sku(string? sku)
    {
        Assert.Throws<ArgumentException>(() => Product.Create(sku, "Product", null, CreatedAtUtc));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_blank_name(string? name)
    {
        Assert.Throws<ArgumentException>(() => Product.Create("SKU-001", name, null, CreatedAtUtc));
    }

    [Fact]
    public void Create_rejects_values_that_exceed_supported_lengths()
    {
        Assert.Throws<ArgumentException>(() => Product.Create(new string('S', ProductRules.MaxSkuLength + 1), "Product", null, CreatedAtUtc));
        Assert.Throws<ArgumentException>(() => Product.Create("SKU-001", new string('N', ProductRules.MaxNameLength + 1), null, CreatedAtUtc));
        Assert.Throws<ArgumentException>(() => Product.Create("SKU-001", "Product", new string('D', ProductRules.MaxDescriptionLength + 1), CreatedAtUtc));
    }

    [Fact]
    public void Update_preserves_identity_and_updates_utc_timestamp()
    {
        var product = Product.Create("SKU-001", "Product", null, CreatedAtUtc);
        var productId = product.Id;
        var updatedAtUtc = CreatedAtUtc.AddMinutes(5);

        product.Update(" sku-002 ", " Updated product ", " Updated description ", updatedAtUtc);

        Assert.Equal(productId, product.Id);
        Assert.Equal("SKU-002", product.Sku);
        Assert.Equal("Updated product", product.Name);
        Assert.Equal(updatedAtUtc, product.UpdatedAtUtc);
        Assert.Equal(DateTimeKind.Utc, product.UpdatedAtUtc.Kind);
    }

    [Fact]
    public void Set_status_is_idempotent()
    {
        var product = Product.Create("SKU-001", "Product", null, CreatedAtUtc);
        var changedAtUtc = CreatedAtUtc.AddMinutes(5);
        product.SetStatus(false, changedAtUtc);

        product.SetStatus(false, changedAtUtc.AddMinutes(5));

        Assert.False(product.IsActive);
        Assert.Equal(changedAtUtc, product.UpdatedAtUtc);
    }
}