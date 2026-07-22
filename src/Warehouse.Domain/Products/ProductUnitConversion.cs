namespace Warehouse.Domain.Products;

public sealed class ProductUnitConversion
{
    private ProductUnitConversion(string unitOfMeasure, decimal quantityInBaseUnit)
    {
        UnitOfMeasure = unitOfMeasure;
        QuantityInBaseUnit = quantityInBaseUnit;
    }

    public string UnitOfMeasure { get; private set; } = null!;

    public decimal QuantityInBaseUnit { get; private set; }

    public static ProductUnitConversion Create(string? unitOfMeasure, decimal quantityInBaseUnit)
    {
        if (quantityInBaseUnit <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(quantityInBaseUnit), "Conversion quantity must be greater than zero.");
        }

        return new ProductUnitConversion(
            ProductUnitOfMeasure.NormalizeUnitOfMeasure(unitOfMeasure),
            quantityInBaseUnit);
    }
}

public sealed record ProductUnitConversionDefinition(string? UnitOfMeasure, decimal QuantityInBaseUnit);
