using Warehouse.Domain.Common;

namespace Warehouse.Domain.Purchasing;

public sealed class PurchaseOrder : PersistentEntity
{
    private readonly List<PurchaseOrderLine> lines = [];

    private PurchaseOrder(
        Guid id,
        Guid supplierId,
        DateTime createdAtUtc,
        Guid? actorUserId)
        : base(id, createdAtUtc, createdAtUtc, actorUserId, actorUserId)
    {
        SupplierId = RequireId(supplierId, nameof(supplierId));
        Status = PurchaseOrderStatus.Draft;
    }

    public Guid SupplierId { get; private set; }
    public PurchaseOrderStatus Status { get; private set; }
    public IReadOnlyCollection<PurchaseOrderLine> Lines => lines;

    public static PurchaseOrder Create(Guid supplierId, DateTime createdAtUtc, Guid? actorUserId = null)
    {
        EnsureUtc(createdAtUtc);
        return new PurchaseOrder(Guid.NewGuid(), supplierId, createdAtUtc, actorUserId);
    }

    public void ReplaceLines(IEnumerable<PurchaseOrderLine> replacementLines, DateTime updatedAtUtc, Guid? actorUserId = null)
    {
        EnsureDraft();
        EnsureUtc(updatedAtUtc);
        ArgumentNullException.ThrowIfNull(replacementLines);
        var normalizedLines = replacementLines.ToList();
        if (normalizedLines.Select(line => line.SupplierProductId).Distinct().Count() != normalizedLines.Count)
        {
            throw new ArgumentException("A purchase order can contain each supplier catalogue item only once.", nameof(replacementLines));
        }

        lines.Clear();
        lines.AddRange(normalizedLines);
        UpdatedAtUtc = updatedAtUtc;
        SetUpdatedByUser(actorUserId);
    }

    public void UpdateSupplier(Guid supplierId, DateTime updatedAtUtc, Guid? actorUserId = null)
    {
        EnsureDraft();
        EnsureUtc(updatedAtUtc);
        var normalizedSupplierId = RequireId(supplierId, nameof(supplierId));
        if (SupplierId == normalizedSupplierId)
        {
            return;
        }

        SupplierId = normalizedSupplierId;
        UpdatedAtUtc = updatedAtUtc;
        SetUpdatedByUser(actorUserId);
    }

    public void Submit(DateTime updatedAtUtc, Guid? actorUserId = null)
    {
        EnsureDraft();
        EnsureUtc(updatedAtUtc);
        if (lines.Count == 0)
        {
            throw new InvalidOperationException("A purchase order must contain at least one line before submission.");
        }

        Status = PurchaseOrderStatus.Submitted;
        UpdatedAtUtc = updatedAtUtc;
        SetUpdatedByUser(actorUserId);
    }

    private void EnsureDraft()
    {
        if (Status != PurchaseOrderStatus.Draft)
        {
            throw new InvalidOperationException("Only draft purchase orders can be changed.");
        }
    }

    private static Guid RequireId(Guid id, string parameterName) => id != Guid.Empty
        ? id
        : throw new ArgumentException("An identifier is required.", parameterName);

    private static void EnsureUtc(DateTime timestamp)
    {
        if (timestamp.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Timestamps must be UTC.");
        }
    }
}

public sealed class PurchaseOrderLine
{
    private PurchaseOrderLine(
        Guid id,
        Guid supplierProductId,
        Guid productId,
        string productSku,
        string productName,
        string? supplierSku,
        string purchaseUnitOfMeasure,
        decimal quantity,
        decimal unitPrice,
        string currencyCode)
    {
        Id = id;
        SupplierProductId = supplierProductId;
        ProductId = productId;
        ProductSku = productSku;
        ProductName = productName;
        SupplierSku = supplierSku;
        PurchaseUnitOfMeasure = purchaseUnitOfMeasure;
        Quantity = quantity;
        UnitPrice = unitPrice;
        CurrencyCode = currencyCode;
    }

    public Guid Id { get; private set; }
    public Guid SupplierProductId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductSku { get; private set; } = null!;
    public string ProductName { get; private set; } = null!;
    public string? SupplierSku { get; private set; }
    public string PurchaseUnitOfMeasure { get; private set; } = null!;
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public string CurrencyCode { get; private set; } = null!;

    public static PurchaseOrderLine Create(
        SupplierProduct supplierProduct,
        string productSku,
        string productName,
        decimal quantity)
    {
        ArgumentNullException.ThrowIfNull(supplierProduct);
        if (quantity < supplierProduct.MinimumOrderQuantity)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must meet the supplier minimum order quantity.");
        }

        return new PurchaseOrderLine(
            Guid.NewGuid(),
            supplierProduct.Id,
            supplierProduct.ProductId,
            NormalizeRequired(productSku, PurchaseOrderRules.MaxProductSkuLength, nameof(productSku)),
            NormalizeRequired(productName, PurchaseOrderRules.MaxProductNameLength, nameof(productName)),
            supplierProduct.SupplierSku,
            supplierProduct.PurchaseUnitOfMeasure,
            quantity,
            supplierProduct.UnitPrice,
            supplierProduct.CurrencyCode);
    }

    private static string NormalizeRequired(string value, int maximumLength, string parameterName)
    {
        var normalized = value.Trim();
        if (string.IsNullOrWhiteSpace(normalized) || normalized.Length > maximumLength)
        {
            throw new ArgumentException("A valid value is required.", parameterName);
        }

        return normalized;
    }
}
