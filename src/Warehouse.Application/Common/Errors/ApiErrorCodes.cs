namespace Warehouse.Application.Common.Errors;

public static class ApiErrorCodes
{
    public const string SystemUnexpected = "system.unexpected";
    public const string ValidationFailed = "validation.failed";
    public const string ValidationMaxLength = "validation.max_length";
    public const string ValidationRequired = "validation.required";
    public const string WarehouseNotFound = "warehouse.not_found";
    public const string WarehouseCodeConflict = "warehouse.code_conflict";
    public const string ProductNotFound = "product.not_found";
    public const string ProductSkuConflict = "product.sku_conflict";
    public const string InventoryProductNotFound = "inventory.product_not_found";
    public const string InventoryWarehouseNotFound = "inventory.warehouse_not_found";
    public const string InventoryInsufficientStock = "inventory.insufficient_stock";
    public const string InventoryConcurrencyConflict = "inventory.concurrency_conflict";
}