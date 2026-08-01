using Warehouse.Domain.Common;
using Warehouse.Domain.Products;

namespace Warehouse.Domain.Inventory;

public sealed class InventoryTransfer : PersistentEntity
{
    private InventoryTransfer(
        Guid id,
        Guid sourceWarehouseId,
        Guid destinationWarehouseId,
        string? reference,
        string? note,
        DateTime transferredAtUtc,
        DateTime createdAtUtc,
        DateTime updatedAtUtc,
        Guid? createdByUserId,
        Guid? updatedByUserId)
        : base(id, createdAtUtc, updatedAtUtc, createdByUserId, updatedByUserId)
    {
        SourceWarehouseId = sourceWarehouseId;
        DestinationWarehouseId = destinationWarehouseId;
        Reference = reference;
        Note = note;
        TransferredAtUtc = transferredAtUtc;
    }

    public Guid SourceWarehouseId { get; private set; }

    public Guid DestinationWarehouseId { get; private set; }

    public string? Reference { get; private set; }

    public string? Note { get; private set; }

    public DateTime TransferredAtUtc { get; private set; }

    public static InventoryTransfer Create(
        Guid sourceWarehouseId,
        Guid destinationWarehouseId,
        string? reference,
        string? note,
        DateTime transferredAtUtc,
        Guid? actorUserId = null)
    {
        if (sourceWarehouseId == Guid.Empty || destinationWarehouseId == Guid.Empty)
        {
            throw new ArgumentException("Source and destination warehouses are required.");
        }

        if (sourceWarehouseId == destinationWarehouseId)
        {
            throw new ArgumentException("Source and destination warehouses must be different.");
        }

        if (transferredAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Timestamp must be UTC.", nameof(transferredAtUtc));
        }

        return new InventoryTransfer(
            Guid.NewGuid(),
            sourceWarehouseId,
            destinationWarehouseId,
            NormalizeOptional(reference, InventoryTransferRules.MaxReferenceLength, nameof(reference)),
            NormalizeOptional(note, InventoryTransferRules.MaxNoteLength, nameof(note)),
            transferredAtUtc,
            transferredAtUtc,
            transferredAtUtc,
            actorUserId,
            actorUserId);
    }

    private static string? NormalizeOptional(string? value, int maxLength, string parameterName)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return null;
        }

        if (trimmed.Length > maxLength)
        {
            throw new ArgumentException($"Value cannot exceed {maxLength} characters.", parameterName);
        }

        return trimmed;
    }
}

public sealed class InventoryTransferLine : PersistentEntity
{
    private InventoryTransferLine(
        Guid id,
        Guid inventoryTransferId,
        int lineNumber,
        Guid productId,
        string unitOfMeasure,
        decimal quantityInUnit,
        decimal quantityInBaseUnit,
        DateTime createdAtUtc,
        DateTime updatedAtUtc,
        Guid? createdByUserId,
        Guid? updatedByUserId)
        : base(id, createdAtUtc, updatedAtUtc, createdByUserId, updatedByUserId)
    {
        InventoryTransferId = inventoryTransferId;
        LineNumber = lineNumber;
        ProductId = productId;
        UnitOfMeasure = unitOfMeasure;
        QuantityInUnit = quantityInUnit;
        QuantityInBaseUnit = quantityInBaseUnit;
    }

    public Guid InventoryTransferId { get; private set; }

    public int LineNumber { get; private set; }

    public Guid ProductId { get; private set; }

    public string UnitOfMeasure { get; private set; } = null!;

    public decimal QuantityInUnit { get; private set; }

    public decimal QuantityInBaseUnit { get; private set; }

    public Guid? TransferOutMovementId { get; private set; }

    public Guid? TransferInMovementId { get; private set; }

    public static InventoryTransferLine Create(
        Guid inventoryTransferId,
        int lineNumber,
        Guid productId,
        string? unitOfMeasure,
        decimal quantityInUnit,
        decimal quantityInBaseUnit,
        DateTime createdAtUtc,
        Guid? actorUserId = null)
    {
        if (inventoryTransferId == Guid.Empty || productId == Guid.Empty)
        {
            throw new ArgumentException("A transfer and product are required.");
        }

        if (lineNumber <= 0 || quantityInUnit <= 0m || quantityInBaseUnit <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(quantityInBaseUnit));
        }

        if (createdAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Timestamp must be UTC.", nameof(createdAtUtc));
        }

        return new InventoryTransferLine(
            Guid.NewGuid(),
            inventoryTransferId,
            lineNumber,
            productId,
            ProductUnitOfMeasure.NormalizeUnitOfMeasure(unitOfMeasure),
            quantityInUnit,
            quantityInBaseUnit,
            createdAtUtc,
            createdAtUtc,
            actorUserId,
            actorUserId);
    }

    public void LinkMovements(Guid transferOutMovementId, Guid transferInMovementId)
    {
        if (transferOutMovementId == Guid.Empty || transferInMovementId == Guid.Empty ||
            transferOutMovementId == transferInMovementId)
        {
            throw new ArgumentException("Distinct transfer movements are required.");
        }

        TransferOutMovementId = transferOutMovementId;
        TransferInMovementId = transferInMovementId;
    }
}

public static class InventoryTransferRules
{
    public const int MaxReferenceLength = 100;
    public const int MaxNoteLength = 1000;
}
