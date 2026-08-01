using Warehouse.Domain.Common;
using Warehouse.Domain.Products;

namespace Warehouse.Domain.Inventory;

public sealed class CycleCount : PersistentEntity
{
    private CycleCount(
        Guid id,
        string number,
        Guid warehouseId,
        string? reference,
        string? note,
        DateTime countedAtUtc,
        DateTime createdAtUtc,
        DateTime updatedAtUtc,
        Guid? createdByUserId,
        Guid? updatedByUserId)
        : base(id, createdAtUtc, updatedAtUtc, createdByUserId, updatedByUserId)
    {
        Number = number;
        WarehouseId = warehouseId;
        Reference = reference;
        Note = note;
        CountedAtUtc = countedAtUtc;
    }

    public string Number { get; private set; } = null!;

    public Guid WarehouseId { get; private set; }

    public string? Reference { get; private set; }

    public string? Note { get; private set; }

    public DateTime CountedAtUtc { get; private set; }

    public static CycleCount Create(
        Guid warehouseId,
        string? reference,
        string? note,
        DateTime countedAtUtc,
        Guid? actorUserId = null,
        string? number = null)
    {
        if (warehouseId == Guid.Empty)
        {
            throw new ArgumentException("A warehouse is required.", nameof(warehouseId));
        }

        if (countedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Timestamp must be UTC.", nameof(countedAtUtc));
        }

        return new CycleCount(
            Guid.NewGuid(),
            NormalizeNumber(number),
            warehouseId,
            NormalizeOptional(reference, CycleCountRules.MaxReferenceLength, nameof(reference)),
            NormalizeOptional(note, CycleCountRules.MaxNoteLength, nameof(note)),
            countedAtUtc,
            countedAtUtc,
            countedAtUtc,
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

    private static string NormalizeNumber(string? number)
    {
        var normalized = number?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return Guid.NewGuid().ToString("N").ToUpperInvariant();
        }

        if (normalized.Length > CycleCountRules.MaxNumberLength)
        {
            throw new ArgumentException($"Number cannot exceed {CycleCountRules.MaxNumberLength} characters.", nameof(number));
        }

        return normalized;
    }
}

public sealed class CycleCountLine : PersistentEntity
{
    private CycleCountLine(
        Guid id,
        Guid cycleCountId,
        int lineNumber,
        Guid productId,
        decimal systemQuantityInBase,
        int systemBalanceVersion,
        string countedUnitOfMeasure,
        decimal countedQuantityInUnit,
        decimal countedQuantityInBase,
        DateTime createdAtUtc,
        DateTime updatedAtUtc,
        Guid? createdByUserId,
        Guid? updatedByUserId)
        : base(id, createdAtUtc, updatedAtUtc, createdByUserId, updatedByUserId)
    {
        CycleCountId = cycleCountId;
        LineNumber = lineNumber;
        ProductId = productId;
        SystemQuantityInBase = systemQuantityInBase;
        SystemBalanceVersion = systemBalanceVersion;
        CountedUnitOfMeasure = countedUnitOfMeasure;
        CountedQuantityInUnit = countedQuantityInUnit;
        CountedQuantityInBase = countedQuantityInBase;
        VarianceQuantityInBase = countedQuantityInBase - systemQuantityInBase;
    }

    public Guid CycleCountId { get; private set; }

    public int LineNumber { get; private set; }

    public Guid ProductId { get; private set; }

    public decimal SystemQuantityInBase { get; private set; }

    public int SystemBalanceVersion { get; private set; }

    public string CountedUnitOfMeasure { get; private set; } = null!;

    public decimal CountedQuantityInUnit { get; private set; }

    public decimal CountedQuantityInBase { get; private set; }

    public decimal VarianceQuantityInBase { get; private set; }

    public Guid? InventoryMovementId { get; private set; }

    public static CycleCountLine Create(
        Guid cycleCountId,
        int lineNumber,
        Guid productId,
        decimal systemQuantityInBase,
        int systemBalanceVersion,
        string? countedUnitOfMeasure,
        decimal countedQuantityInUnit,
        decimal countedQuantityInBase,
        DateTime createdAtUtc,
        Guid? actorUserId = null)
    {
        if (cycleCountId == Guid.Empty || productId == Guid.Empty)
        {
            throw new ArgumentException("A cycle count and product are required.");
        }

        if (lineNumber <= 0 || systemQuantityInBase < 0m || systemBalanceVersion < 0 ||
            countedQuantityInUnit < 0m || countedQuantityInBase < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(countedQuantityInBase));
        }

        if (createdAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Timestamp must be UTC.", nameof(createdAtUtc));
        }

        return new CycleCountLine(
            Guid.NewGuid(),
            cycleCountId,
            lineNumber,
            productId,
            systemQuantityInBase,
            systemBalanceVersion,
            ProductUnitOfMeasure.NormalizeUnitOfMeasure(countedUnitOfMeasure),
            countedQuantityInUnit,
            countedQuantityInBase,
            createdAtUtc,
            createdAtUtc,
            actorUserId,
            actorUserId);
    }

    public void LinkInventoryMovement(Guid inventoryMovementId)
    {
        if (inventoryMovementId == Guid.Empty)
        {
            throw new ArgumentException("An inventory movement is required.", nameof(inventoryMovementId));
        }

        InventoryMovementId = inventoryMovementId;
    }
}

public static class CycleCountRules
{
    public const int MaxNumberLength = 32;
    public const int MaxReferenceLength = 100;
    public const int MaxNoteLength = 1000;
}
