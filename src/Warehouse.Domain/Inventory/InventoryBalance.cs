using Warehouse.Domain.Common;

namespace Warehouse.Domain.Inventory;

public sealed class InventoryBalance : PersistentEntity
{
    private InventoryBalance(
        Guid id,
        Guid productId,
        Guid warehouseId,
        decimal quantity,
        DateTime createdAtUtc,
        DateTime updatedAtUtc,
        Guid? createdByUserId,
        Guid? updatedByUserId)
        : base(id, createdAtUtc, updatedAtUtc, createdByUserId, updatedByUserId)
    {
        ProductId = productId;
        WarehouseId = warehouseId;
        Quantity = quantity;
    }

    public Guid ProductId { get; private set; }

    public Guid WarehouseId { get; private set; }

    public decimal Quantity { get; private set; }

    public int Version { get; private set; }

    public static InventoryBalance Create(
        Guid productId,
        Guid warehouseId,
        DateTime createdAtUtc,
        Guid? actorUserId = null)
    {
        EnsureValidIdentity(productId, nameof(productId));
        EnsureValidIdentity(warehouseId, nameof(warehouseId));
        EnsureUtc(createdAtUtc, nameof(createdAtUtc));

        return new InventoryBalance(
            Guid.NewGuid(), productId, warehouseId, 0m, createdAtUtc, createdAtUtc, actorUserId, actorUserId);
    }

    public void ApplyAdjustment(decimal quantityDelta, DateTime changedAtUtc, Guid? actorUserId = null)
    {
        EnsureNonZero(quantityDelta, nameof(quantityDelta));
        EnsureUtc(changedAtUtc, nameof(changedAtUtc));

        var resultingQuantity = Quantity + quantityDelta;
        if (resultingQuantity < 0m)
        {
            throw new InvalidOperationException("Inventory quantity cannot become negative.");
        }

        Quantity = resultingQuantity;
        UpdatedAtUtc = changedAtUtc;
        SetUpdatedByUser(actorUserId);
        Version++;
    }

    private static void EnsureValidIdentity(Guid id, string parameterName)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("An entity ID is required.", parameterName);
        }
    }

    private static void EnsureNonZero(decimal quantity, string parameterName)
    {
        if (quantity == 0m)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Quantity must not be zero.");
        }
    }

    private static void EnsureUtc(DateTime timestamp, string parameterName)
    {
        if (timestamp.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Timestamps must be UTC.", parameterName);
        }
    }
}
