namespace Warehouse.Api.Endpoints.Inventory;

public static class InventoryApiRoutes
{
    public const string BasePath = "/api/inventory";
    public const string MovementHistoryPath = "/movements";
    public const string OverviewPath = "/overview";
    public const string AdjustmentPath = "/adjustments";
    public const string AdjustmentByIdPath = "/adjustments/{id:guid}";
}
