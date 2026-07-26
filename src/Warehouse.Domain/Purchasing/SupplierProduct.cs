using Warehouse.Domain.Common;
using Warehouse.Domain.Products;

namespace Warehouse.Domain.Purchasing;

public sealed class SupplierProduct : PersistentEntity
{
    private SupplierProduct(
        Guid id,
        Guid supplierId,
        Guid productId,
        string? supplierSku,
        string purchaseUnitOfMeasure,
        decimal minimumOrderQuantity,
        decimal unitPrice,
        string currencyCode,
        bool isActive,
        DateTime createdAtUtc,
        DateTime updatedAtUtc,
        Guid? createdByUserId,
        Guid? updatedByUserId)
        : base(id, createdAtUtc, updatedAtUtc, createdByUserId, updatedByUserId)
    {
        SupplierId = supplierId;
        ProductId = productId;
        SupplierSku = supplierSku;
        PurchaseUnitOfMeasure = purchaseUnitOfMeasure;
        MinimumOrderQuantity = minimumOrderQuantity;
        UnitPrice = unitPrice;
        CurrencyCode = currencyCode;
        IsActive = isActive;
    }

    public Guid SupplierId { get; private set; }
    public Guid ProductId { get; private set; }
    public string? SupplierSku { get; private set; }
    public string PurchaseUnitOfMeasure { get; private set; } = null!;
    public decimal MinimumOrderQuantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public string CurrencyCode { get; private set; } = null!;
    public bool IsActive { get; private set; }

    public static SupplierProduct Create(
        Guid supplierId,
        Guid productId,
        string? supplierSku,
        string? purchaseUnitOfMeasure,
        decimal minimumOrderQuantity,
        decimal unitPrice,
        string? currencyCode,
        DateTime createdAtUtc,
        Guid? actorUserId = null)
    {
        EnsureUtc(createdAtUtc);
        return new SupplierProduct(
            Guid.NewGuid(),
            RequireId(supplierId, nameof(supplierId)),
            RequireId(productId, nameof(productId)),
            NormalizeOptionalSupplierSku(supplierSku),
            ProductUnitOfMeasure.NormalizeUnitOfMeasure(purchaseUnitOfMeasure),
            NormalizePositiveQuantity(minimumOrderQuantity, nameof(minimumOrderQuantity)),
            NormalizeUnitPrice(unitPrice),
            NormalizeCurrencyCode(currencyCode),
            true,
            createdAtUtc,
            createdAtUtc,
            actorUserId,
            actorUserId);
    }

    public void Update(
        string? supplierSku,
        string? purchaseUnitOfMeasure,
        decimal minimumOrderQuantity,
        decimal unitPrice,
        string? currencyCode,
        DateTime updatedAtUtc,
        Guid? actorUserId = null)
    {
        EnsureUtc(updatedAtUtc);
        var normalizedSupplierSku = NormalizeOptionalSupplierSku(supplierSku);
        var normalizedUnit = ProductUnitOfMeasure.NormalizeUnitOfMeasure(purchaseUnitOfMeasure);
        var normalizedMinimum = NormalizePositiveQuantity(minimumOrderQuantity, nameof(minimumOrderQuantity));
        var normalizedPrice = NormalizeUnitPrice(unitPrice);
        var normalizedCurrency = NormalizeCurrencyCode(currencyCode);

        if (SupplierSku == normalizedSupplierSku && PurchaseUnitOfMeasure == normalizedUnit &&
            MinimumOrderQuantity == normalizedMinimum && UnitPrice == normalizedPrice && CurrencyCode == normalizedCurrency)
        {
            return;
        }

        SupplierSku = normalizedSupplierSku;
        PurchaseUnitOfMeasure = normalizedUnit;
        MinimumOrderQuantity = normalizedMinimum;
        UnitPrice = normalizedPrice;
        CurrencyCode = normalizedCurrency;
        UpdatedAtUtc = updatedAtUtc;
        SetUpdatedByUser(actorUserId);
    }

    public void SetStatus(bool isActive, DateTime updatedAtUtc, Guid? actorUserId = null)
    {
        EnsureUtc(updatedAtUtc);
        if (IsActive == isActive)
        {
            return;
        }

        IsActive = isActive;
        UpdatedAtUtc = updatedAtUtc;
        SetUpdatedByUser(actorUserId);
    }

    public static string NormalizeCurrencyCode(string? currencyCode)
    {
        var normalized = currencyCode?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalized) || normalized.Length != SupplierProductRules.CurrencyCodeLength || normalized.Any(character => !char.IsAsciiLetter(character)))
        {
            throw new ArgumentException("Currency code must be a three-letter ISO code.", nameof(currencyCode));
        }

        return normalized;
    }

    private static Guid RequireId(Guid id, string parameterName) => id != Guid.Empty
        ? id
        : throw new ArgumentException("An identifier is required.", parameterName);

    private static string? NormalizeOptionalSupplierSku(string? supplierSku)
    {
        var normalized = supplierSku?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        if (normalized.Length > SupplierProductRules.MaxSupplierSkuLength)
        {
            throw new ArgumentException($"Supplier SKU cannot exceed {SupplierProductRules.MaxSupplierSkuLength} characters.", nameof(supplierSku));
        }

        return normalized;
    }

    private static decimal NormalizePositiveQuantity(decimal quantity, string parameterName) => quantity > 0m
        ? quantity
        : throw new ArgumentOutOfRangeException(parameterName, "Quantity must be greater than zero.");

    private static decimal NormalizeUnitPrice(decimal unitPrice) => unitPrice >= 0m
        ? unitPrice
        : throw new ArgumentOutOfRangeException(nameof(unitPrice), "Unit price cannot be negative.");

    private static void EnsureUtc(DateTime timestamp)
    {
        if (timestamp.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Timestamps must be UTC.");
        }
    }
}
