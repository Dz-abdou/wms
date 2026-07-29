using Warehouse.Domain.Common;

namespace Warehouse.Domain.Receiving;

public static class GoodsReceiptRules
{
    public const int MaxSupplierDeliveryNoteLength = 128;
    public const int MaxNotesLength = 2000;
}

public sealed class GoodsReceipt : PersistentEntity
{
    private readonly List<GoodsReceiptLine> lines = [];

    private GoodsReceipt(Guid id, string number, Guid purchaseOrderId, Guid warehouseId, DateTime receivedAtUtc, string? supplierDeliveryNote, string? notes, Guid receiverUserId, DateTime createdAtUtc)
        : base(id, createdAtUtc, createdAtUtc, receiverUserId, receiverUserId)
    {
        Number = number; PurchaseOrderId = purchaseOrderId; WarehouseId = warehouseId; ReceivedAtUtc = receivedAtUtc;
        SupplierDeliveryNote = supplierDeliveryNote; Notes = notes; ReceiverUserId = receiverUserId;
    }

    public string Number { get; private set; } = null!;
    public Guid PurchaseOrderId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public DateTime ReceivedAtUtc { get; private set; }
    public string? SupplierDeliveryNote { get; private set; }
    public string? Notes { get; private set; }
    public Guid ReceiverUserId { get; private set; }
    public IReadOnlyCollection<GoodsReceiptLine> Lines => lines;

    public static GoodsReceipt Create(string number, Guid purchaseOrderId, Guid warehouseId, DateTime receivedAtUtc, string? supplierDeliveryNote, string? notes, Guid receiverUserId)
    {
        if (string.IsNullOrWhiteSpace(number) || purchaseOrderId == Guid.Empty || warehouseId == Guid.Empty || receiverUserId == Guid.Empty)
            throw new ArgumentException("A valid goods receipt requires document identifiers.");
        if (receivedAtUtc.Kind != DateTimeKind.Utc) throw new ArgumentException("Received time must be UTC.", nameof(receivedAtUtc));
        return new GoodsReceipt(Guid.NewGuid(), number.Trim().ToUpperInvariant(), purchaseOrderId, warehouseId, receivedAtUtc, NormalizeOptional(supplierDeliveryNote, GoodsReceiptRules.MaxSupplierDeliveryNoteLength), NormalizeOptional(notes, GoodsReceiptRules.MaxNotesLength), receiverUserId, receivedAtUtc);
    }

    public void AddLines(IEnumerable<GoodsReceiptLine> receiptLines)
    { var values = receiptLines.ToList(); if (values.Count == 0 || values.Select(line => line.PurchaseOrderLineId).Distinct().Count() != values.Count) throw new ArgumentException("A goods receipt requires distinct purchase-order lines.", nameof(receiptLines)); lines.AddRange(values); }

    private static string? NormalizeOptional(string? value, int maxLength) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().Length <= maxLength ? value.Trim() : throw new ArgumentOutOfRangeException(nameof(value));
}

public sealed class GoodsReceiptLine
{
    private GoodsReceiptLine(Guid id, Guid purchaseOrderLineId, int purchaseOrderLineNumber, Guid productId, string productSku, string productName, string unitOfMeasure, decimal acceptedQuantity, decimal acceptedQuantityInBaseUnit, decimal conversionFactorToBaseUnit, Guid inventoryMovementId)
    { Id=id; PurchaseOrderLineId=purchaseOrderLineId; PurchaseOrderLineNumber=purchaseOrderLineNumber; ProductId=productId; ProductSku=productSku; ProductName=productName; UnitOfMeasure=unitOfMeasure; AcceptedQuantity=acceptedQuantity; AcceptedQuantityInBaseUnit=acceptedQuantityInBaseUnit; ConversionFactorToBaseUnit=conversionFactorToBaseUnit; InventoryMovementId=inventoryMovementId; }
    public Guid Id { get; private set; } public Guid PurchaseOrderLineId { get; private set; } public int PurchaseOrderLineNumber { get; private set; } public Guid ProductId { get; private set; } public string ProductSku { get; private set; } = null!; public string ProductName { get; private set; } = null!; public string UnitOfMeasure { get; private set; } = null!; public decimal AcceptedQuantity { get; private set; } public decimal AcceptedQuantityInBaseUnit { get; private set; } public decimal ConversionFactorToBaseUnit { get; private set; } public Guid InventoryMovementId { get; private set; }
    public static GoodsReceiptLine Create(Guid purchaseOrderLineId, int lineNumber, Guid productId, string productSku, string productName, string unitOfMeasure, decimal acceptedQuantity, decimal conversionFactorToBaseUnit, Guid inventoryMovementId) => acceptedQuantity > 0m && conversionFactorToBaseUnit > 0m && purchaseOrderLineId != Guid.Empty && productId != Guid.Empty && inventoryMovementId != Guid.Empty ? new(Guid.NewGuid(), purchaseOrderLineId, lineNumber, productId, productSku, productName, unitOfMeasure.Trim().ToUpperInvariant(), acceptedQuantity, acceptedQuantity * conversionFactorToBaseUnit, conversionFactorToBaseUnit, inventoryMovementId) : throw new ArgumentOutOfRangeException(nameof(acceptedQuantity));
}
