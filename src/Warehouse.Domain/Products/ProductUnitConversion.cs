namespace Warehouse.Domain.Products;

public sealed class ProductUnitConversion
{
    private ProductUnitConversion(string unitOfMeasure, decimal quantityInBaseUnit, bool allowsFractionalQuantity)
    {
        UnitOfMeasure = unitOfMeasure;
        QuantityInBaseUnit = quantityInBaseUnit;
        AllowsFractionalQuantity = allowsFractionalQuantity;
    }

    public string UnitOfMeasure { get; private set; } = null!;

    public decimal QuantityInBaseUnit { get; private set; }

    public bool AllowsFractionalQuantity { get; private set; }

    public static ProductUnitConversion Create(string? unitOfMeasure, decimal quantityInBaseUnit, bool allowsFractionalQuantity = false)
    {
        if (quantityInBaseUnit <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(quantityInBaseUnit), "Conversion quantity must be greater than zero.");
        }

        return new ProductUnitConversion(
            ProductUnitOfMeasure.NormalizeUnitOfMeasure(unitOfMeasure),
            quantityInBaseUnit,
            allowsFractionalQuantity);
    }
}

public sealed record ProductUnitConversionDefinition(
    string? UnitOfMeasure,
    decimal QuantityInBaseUnit,
    bool AllowsFractionalQuantity = false);
