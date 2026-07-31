namespace Warehouse.Api.Endpoints.Inventory;

public static class InventoryApiRoutes
{
    public const string BasePath = "/api/inventory";
    public const string MovementHistoryPath = "/movements";
    public const string OverviewPath = "/overview";
    public const string AdjustmentPath = "/adjustments";
    public const string AdjustmentByIdPath = "/adjustments/{id:guid}";
    public const string CycleCountPath = "/cycle-counts";
    public const string CycleCountCandidatePath = "/cycle-counts/candidate";
    public const string CycleCountByIdPath = "/cycle-counts/{id:guid}";
    public const string TransferPath = "/transfers";
    public const string TransferCandidatePath = "/transfers/candidate";
    public const string TransferByIdPath = "/transfers/{id:guid}";
}
