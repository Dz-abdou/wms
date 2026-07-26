namespace Warehouse.Domain.Suppliers;

public static class SupplierRules
{
    public const string DefaultCurrencyCode = "DZD";
    public const int CurrencyCodeLength = 3;
    public const int MaxCodeLength = 32;
    public const int MaxNameLength = 200;
    public const int MaxEmailLength = 320;
    public const int MaxPhoneNumberLength = 50;
    public const int MaxAddressLength = 500;
}
