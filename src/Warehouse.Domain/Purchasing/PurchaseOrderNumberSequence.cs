namespace Warehouse.Domain.Purchasing;

public sealed class PurchaseOrderNumberSequence
{
    private PurchaseOrderNumberSequence(int year)
    {
        Year = year;
    }

    public long Value { get; private set; }
    public int Year { get; private set; }

    public static PurchaseOrderNumberSequence Create(int year)
    {
        if (year is < 2000 or > 9999)
        {
            throw new ArgumentOutOfRangeException(nameof(year));
        }

        return new PurchaseOrderNumberSequence(year);
    }

    public string ToNumber() => $"PO-{Year}-{Value:D6}";
}
