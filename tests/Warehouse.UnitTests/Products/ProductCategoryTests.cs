using Warehouse.Domain.Products;

namespace Warehouse.UnitTests.Products;

public sealed class ProductCategoryTests
{
    [Fact]
    public void Create_normalizes_code_and_preserves_parent_assignment()
    {
        var parentId = Guid.NewGuid();
        var createdAtUtc = new DateTime(2026, 7, 22, 12, 0, 0, DateTimeKind.Utc);

        var category = ProductCategory.Create(" consumables ", "Consumables", parentId, createdAtUtc);

        Assert.Equal("CONSUMABLES", category.Code);
        Assert.Equal("Consumables", category.Name);
        Assert.Equal(parentId, category.ParentCategoryId);
    }

    [Fact]
    public void Create_rejects_blank_or_oversized_code()
    {
        var createdAtUtc = new DateTime(2026, 7, 22, 12, 0, 0, DateTimeKind.Utc);

        Assert.Throws<ArgumentException>(() => ProductCategory.Create(" ", "Category", null, createdAtUtc));
        Assert.Throws<ArgumentException>(() => ProductCategory.Create(
            new string('C', ProductCategoryRules.MaxCodeLength + 1),
            "Category",
            null,
            createdAtUtc));
    }
}
