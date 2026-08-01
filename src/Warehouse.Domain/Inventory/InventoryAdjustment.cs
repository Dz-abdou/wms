using Warehouse.Domain.Common;

namespace Warehouse.Domain.Inventory;

public sealed class InventoryAdjustment : PersistentEntity
{
    private InventoryAdjustment(
        Guid id,
        string number,
        InventoryAdjustmentReason reason,
        string? reference,
        string? note,
        DateTime createdAtUtc,
        DateTime updatedAtUtc,
        Guid? createdByUserId,
        Guid? updatedByUserId)
        : base(id, createdAtUtc, updatedAtUtc, createdByUserId, updatedByUserId)
    {
        Number = number;
        Reason = reason;
        Reference = reference;
        Note = note;
    }

    public string Number { get; private set; } = null!;

    public InventoryAdjustmentReason Reason { get; private set; }

    public string? Reference { get; private set; }

    public string? Note { get; private set; }

    public static InventoryAdjustment Create(
        InventoryAdjustmentReason reason,
        string? reference,
        string? note,
        DateTime createdAtUtc,
        Guid? actorUserId = null,
        string? number = null)
    {
        if (createdAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Timestamp must be UTC.", nameof(createdAtUtc));
        }

        return new InventoryAdjustment(
            Guid.NewGuid(),
            NormalizeNumber(number),
            reason,
            NormalizeOptional(reference, InventoryAdjustmentRules.MaxReferenceLength, nameof(reference)),
            NormalizeOptional(note, InventoryAdjustmentRules.MaxNoteLength, nameof(note)),
            createdAtUtc,
            createdAtUtc,
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

        if (normalized.Length > InventoryAdjustmentRules.MaxNumberLength)
        {
            throw new ArgumentException($"Number cannot exceed {InventoryAdjustmentRules.MaxNumberLength} characters.", nameof(number));
        }

        return normalized;
    }
}

public static class InventoryAdjustmentRules
{
    public const int MaxNumberLength = 32;
    public const int MaxReferenceLength = 100;
    public const int MaxNoteLength = 1000;
}
