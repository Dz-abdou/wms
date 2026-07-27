namespace Warehouse.Domain.Purchasing;

public static class SupplierProductRules
{
    public const int MaxSupplierSkuLength = 64;
    public const int CurrencyCodeLength = 3;
    public const int UnitOfMeasureLength = 16;
}

public static class PurchaseOrderRules
{
    public const int MaxNumberLength = 32;
    public const int MaxSupplierSkuLength = SupplierProductRules.MaxSupplierSkuLength;
    public const int MaxProductSkuLength = 64;
    public const int MaxProductNameLength = 200;
    public const int MaxSupplierReferenceLength = 128;
    public const int MaxNotesLength = 2_000;
    public const int MaxStatusReasonLength = 500;
}
