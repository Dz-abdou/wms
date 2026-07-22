namespace Warehouse.Domain.Products;

public static class ProductUnitOfMeasure
{
    public const int MaxCodeLength = 16;

    private static readonly HashSet<string> SupportedBaseUnits =
    [
        "EA",
        "KG",
        "G",
        "L",
        "ML",
        "M",
        "CM",
        "MM"
    ];

    public static string NormalizeBaseUnitOfMeasure(string? unitOfMeasure)
    {
        var normalized = Normalize(unitOfMeasure);
        if (!SupportedBaseUnits.Contains(normalized))
        {
            throw new ArgumentException($"'{normalized}' is not a supported base unit of measure.", nameof(unitOfMeasure));
        }

        return normalized;
    }

    public static string NormalizeUnitOfMeasure(string? unitOfMeasure) => Normalize(unitOfMeasure);

    public static bool AllowsFractionalQuantity(string unitOfMeasure) => unitOfMeasure switch
    {
        "KG" or "G" or "L" or "ML" or "M" or "CM" or "MM" => true,
        _ => false
    };

    private static string Normalize(string? unitOfMeasure)
    {
        var normalized = unitOfMeasure?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("Unit of measure is required.", nameof(unitOfMeasure));
        }

        if (normalized.Length > MaxCodeLength)
        {
            throw new ArgumentException($"Unit of measure cannot exceed {MaxCodeLength} characters.", nameof(unitOfMeasure));
        }

        return normalized;
    }
}
