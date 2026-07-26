namespace Warehouse.Domain.Inventory;

public enum InventoryAdjustmentReason
{
    StockCorrection,
    Damage,
    WriteOff,
    FoundStock,
    InitialBalance
}
