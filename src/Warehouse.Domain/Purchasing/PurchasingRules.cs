namespace Warehouse.Domain.Purchasing;

public static class SupplierProductRules
{
    public const int MaxSupplierSkuLength = 64;
    public const int CurrencyCodeLength = 3;
    public const int UnitOfMeasureLength = 16;
}

public static class PurchaseOrderRules
{
    public const int MaxSupplierSkuLength = SupplierProductRules.MaxSupplierSkuLength;
    public const int MaxProductSkuLength = 64;
    public const int MaxProductNameLength = 200;
}
