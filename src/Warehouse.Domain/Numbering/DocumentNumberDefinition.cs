namespace Warehouse.Domain.Numbering;

public sealed class DocumentNumberDefinition
{
    private DocumentNumberDefinition(
        string code,
        string description,
        string prefix,
        int digitCount,
        DocumentNumberResetPeriod resetPeriod,
        bool isActive,
        bool allowsManualEntry)
    {
        Code = code;
        Description = description;
        Prefix = prefix;
        DigitCount = digitCount;
        ResetPeriod = resetPeriod;
        IsActive = isActive;
        AllowsManualEntry = allowsManualEntry;
    }

    public string Code { get; private set; } = null!;

    public string Description { get; private set; } = null!;

    public string Prefix { get; private set; } = null!;

    public int DigitCount { get; private set; }

    public DocumentNumberResetPeriod ResetPeriod { get; private set; }

    public bool IsActive { get; private set; }

    public bool AllowsManualEntry { get; private set; }

    public static IReadOnlyList<DocumentNumberDefinition> InitialDefinitions { get; } =
    [
        CreateSeed(DocumentNumberCodes.PurchaseOrder, "Purchase order", "PO"),
        CreateSeed(DocumentNumberCodes.GoodsReceipt, "Goods receipt", "GR"),
        CreateSeed(DocumentNumberCodes.InventoryAdjustment, "Inventory adjustment", "IA"),
        CreateSeed(DocumentNumberCodes.CycleCount, "Cycle count", "CC"),
        CreateSeed(DocumentNumberCodes.InventoryTransfer, "Inventory transfer", "TR"),
        CreateSeed(DocumentNumberCodes.SalesOrder, "Sales order", "SO")
    ];

    private static DocumentNumberDefinition CreateSeed(string code, string description, string prefix) =>
        new(code, description, prefix, DocumentNumberRules.DefaultDigitCount, DocumentNumberResetPeriod.Yearly, true, false);
}

public enum DocumentNumberResetPeriod
{
    Yearly = 1
}

public static class DocumentNumberCodes
{
    public const string PurchaseOrder = "PO";
    public const string GoodsReceipt = "GR";
    public const string InventoryAdjustment = "IA";
    public const string CycleCount = "CC";
    public const string InventoryTransfer = "TR";
    public const string SalesOrder = "SO";
}

public static class DocumentNumberRules
{
    public const int MaxCodeLength = 32;
    public const int MaxDescriptionLength = 200;
    public const int MaxPrefixLength = 16;
    public const int DefaultDigitCount = 6;
    public const int MinimumDigitCount = 1;
    public const int MaximumDigitCount = 12;
}
