namespace Warehouse.Application.Common.Errors;

public static class ApiErrorCodes
{
    public const string SystemUnexpected = "system.unexpected";
    public const string ValidationFailed = "validation.failed";
    public const string ValidationMaxLength = "validation.max_length";
    public const string ValidationInvalid = "validation.invalid";
    public const string ValidationRequired = "validation.required";
    public const string WarehouseNotFound = "warehouse.not_found";
    public const string WarehouseCodeConflict = "warehouse.code_conflict";
    public const string ProductNotFound = "product.not_found";
    public const string ProductSkuConflict = "product.sku_conflict";
    public const string SupplierNotFound = "supplier.not_found";
    public const string SupplierCodeConflict = "supplier.code_conflict";
    public const string SupplierProductNotFound = "supplier_product.not_found";
    public const string SupplierProductConflict = "supplier_product.conflict";
    public const string SupplierProductCurrencyNotSupported = "supplier_product.currency_not_supported";
    public const string ProductCategoryNotFound = "product.category_not_found";
    public const string ProductCategoryCodeConflict = "product_category.code_conflict";
    public const string InventoryProductNotFound = "inventory.product_not_found";
    public const string InventoryWarehouseNotFound = "inventory.warehouse_not_found";
    public const string InventoryInsufficientStock = "inventory.insufficient_stock";
    public const string InventoryConcurrencyConflict = "inventory.concurrency_conflict";
    public const string InventoryInvalidUnitOfMeasure = "inventory.invalid_unit_of_measure";
    public const string PurchaseOrderNotFound = "purchase_order.not_found";
    public const string PurchaseOrderImmutable = "purchase_order.immutable";
    public const string PurchaseOrderCatalogueInvalid = "purchase_order.catalogue_invalid";
    public const string PurchaseOrderSubmissionInvalid = "purchase_order.submission_invalid";
}
