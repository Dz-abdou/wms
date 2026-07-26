using Warehouse.Domain.Common;

namespace Warehouse.Domain.Purchasing;

public sealed class PurchaseOrder : PersistentEntity
{
    private readonly List<PurchaseOrderLine> lines = [];
    private readonly List<PurchaseOrderStatusHistory> statusHistory = [];

    private PurchaseOrder(
        Guid id,
        Guid supplierId,
        PurchaseOrderStatus status,
        DateTime createdAtUtc,
        DateTime updatedAtUtc,
        Guid? createdByUserId,
        Guid? updatedByUserId)
        : base(id, createdAtUtc, updatedAtUtc, createdByUserId, updatedByUserId)
    {
        SupplierId = RequireId(supplierId, nameof(supplierId));
        Status = status;
    }

    public Guid SupplierId { get; private set; }
    public string? Number { get; private set; }
    public Guid? DestinationWarehouseId { get; private set; }
    public string? CurrencyCode { get; private set; }
    public DateOnly? OrderDate { get; private set; }
    public DateOnly? ExpectedDeliveryDate { get; private set; }
    public Guid? BuyerUserId { get; private set; }
    public string? SupplierReference { get; private set; }
    public string? Notes { get; private set; }
    public PurchaseOrderStatus Status { get; private set; }
    public DateTime? SubmittedAtUtc { get; private set; }
    public int Version { get; private set; }
    public IReadOnlyCollection<PurchaseOrderLine> Lines => lines;
    public IReadOnlyCollection<PurchaseOrderStatusHistory> StatusHistory => statusHistory;

    public static PurchaseOrder Create(Guid supplierId, DateTime createdAtUtc, Guid? actorUserId = null)
    {
        EnsureUtc(createdAtUtc);
        return new PurchaseOrder(
            Guid.NewGuid(),
            supplierId,
            PurchaseOrderStatus.Draft,
            createdAtUtc,
            createdAtUtc,
            actorUserId,
            actorUserId);
    }

    public static PurchaseOrder Create(
        string number,
        Guid supplierId,
        Guid destinationWarehouseId,
        string currencyCode,
        DateOnly orderDate,
        DateOnly? expectedDeliveryDate,
        Guid buyerUserId,
        string? supplierReference,
        string? notes,
        DateTime createdAtUtc)
    {
        var order = Create(supplierId, createdAtUtc, buyerUserId);
        order.Number = NormalizeRequired(number, PurchaseOrderRules.MaxNumberLength, nameof(number)).ToUpperInvariant();
        order.DestinationWarehouseId = RequireId(destinationWarehouseId, nameof(destinationWarehouseId));
        order.CurrencyCode = NormalizeCurrencyCode(currencyCode);
        order.OrderDate = orderDate;
        order.ExpectedDeliveryDate = ValidateExpectedDeliveryDate(orderDate, expectedDeliveryDate);
        order.BuyerUserId = RequireId(buyerUserId, nameof(buyerUserId));
        order.SupplierReference = NormalizeOptional(supplierReference, PurchaseOrderRules.MaxSupplierReferenceLength, nameof(supplierReference));
        order.Notes = NormalizeOptional(notes, PurchaseOrderRules.MaxNotesLength, nameof(notes));
        order.statusHistory.Add(PurchaseOrderStatusHistory.Create(null, PurchaseOrderStatus.Draft, createdAtUtc, buyerUserId, null));
        return order;
    }

    public void UpdateOperationalDetails(
        Guid supplierId,
        Guid destinationWarehouseId,
        string currencyCode,
        DateOnly orderDate,
        DateOnly? expectedDeliveryDate,
        string? supplierReference,
        string? notes,
        int version,
        DateTime updatedAtUtc,
        Guid actorUserId)
    {
        EnsureDraft();
        if (Version != version) throw new InvalidOperationException("The purchase order was changed by another user.");
        UpdateSupplier(supplierId, updatedAtUtc, actorUserId);
        DestinationWarehouseId = RequireId(destinationWarehouseId, nameof(destinationWarehouseId));
        CurrencyCode = NormalizeCurrencyCode(currencyCode);
        OrderDate = orderDate;
        ExpectedDeliveryDate = ValidateExpectedDeliveryDate(orderDate, expectedDeliveryDate);
        SupplierReference = NormalizeOptional(supplierReference, PurchaseOrderRules.MaxSupplierReferenceLength, nameof(supplierReference));
        Notes = NormalizeOptional(notes, PurchaseOrderRules.MaxNotesLength, nameof(notes));
        Version++;
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
        SubmittedAtUtc = updatedAtUtc;
        if (actorUserId is { } userId)
        {
            statusHistory.Add(PurchaseOrderStatusHistory.Create(PurchaseOrderStatus.Draft, Status, updatedAtUtc, userId, null));
        }
        Version++;
        UpdatedAtUtc = updatedAtUtc;
        SetUpdatedByUser(actorUserId);
    }

    public void Cancel(string? reason, DateTime updatedAtUtc, Guid actorUserId)
    {
        EnsureUtc(updatedAtUtc);
        if (Status is not (PurchaseOrderStatus.Draft or PurchaseOrderStatus.Submitted))
            throw new InvalidOperationException("This purchase order cannot be cancelled in its current state.");

        var previousStatus = Status;
        Status = PurchaseOrderStatus.Cancelled;
        statusHistory.Add(PurchaseOrderStatusHistory.Create(
            previousStatus,
            Status,
            updatedAtUtc,
            actorUserId,
            NormalizeOptional(reason, PurchaseOrderRules.MaxStatusReasonLength, nameof(reason))));
        Version++;
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

    private static string NormalizeRequired(string? value, int maximumLength, string parameterName)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized) || normalized.Length > maximumLength)
            throw new ArgumentException("A valid value is required.", parameterName);
        return normalized;
    }

    private static string NormalizeCurrencyCode(string? currencyCode)
    {
        var normalized = NormalizeRequired(currencyCode, SupplierProductRules.CurrencyCodeLength, nameof(currencyCode));
        if (normalized.Length != SupplierProductRules.CurrencyCodeLength || !normalized.All(char.IsAsciiLetter))
            throw new ArgumentException("A valid currency code is required.", nameof(currencyCode));
        return normalized.ToUpperInvariant();
    }

    private static DateOnly? ValidateExpectedDeliveryDate(DateOnly orderDate, DateOnly? expectedDeliveryDate) =>
        expectedDeliveryDate is null || expectedDeliveryDate >= orderDate
            ? expectedDeliveryDate
            : throw new ArgumentOutOfRangeException(nameof(expectedDeliveryDate));

    private static string? NormalizeOptional(string? value, int maximumLength, string parameterName)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized)) return null;
        if (normalized.Length > maximumLength) throw new ArgumentOutOfRangeException(parameterName);
        return normalized;
    }
}

public sealed class PurchaseOrderStatusHistory
{
    private PurchaseOrderStatusHistory(Guid id, PurchaseOrderStatus? previousStatus, PurchaseOrderStatus status, DateTime changedAtUtc, Guid actorUserId, string? reason)
    {
        Id = id; PreviousStatus = previousStatus; Status = status; ChangedAtUtc = changedAtUtc; ActorUserId = actorUserId; Reason = reason;
    }
    public Guid Id { get; private set; }
    public PurchaseOrderStatus? PreviousStatus { get; private set; }
    public PurchaseOrderStatus Status { get; private set; }
    public DateTime ChangedAtUtc { get; private set; }
    public Guid ActorUserId { get; private set; }
    public string? Reason { get; private set; }
    public static PurchaseOrderStatusHistory Create(PurchaseOrderStatus? previousStatus, PurchaseOrderStatus status, DateTime changedAtUtc, Guid actorUserId, string? reason) =>
        new(Guid.NewGuid(), previousStatus, status, changedAtUtc, RequireActor(actorUserId), reason);
    private static Guid RequireActor(Guid actorUserId) => actorUserId != Guid.Empty ? actorUserId : throw new ArgumentException("An actor is required.", nameof(actorUserId));
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
