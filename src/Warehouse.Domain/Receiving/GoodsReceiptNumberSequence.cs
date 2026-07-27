namespace Warehouse.Domain.Receiving;

public sealed class GoodsReceiptNumberSequence
{
    private GoodsReceiptNumberSequence(int year) => Year = year;

    public long Value { get; private set; }
    public int Year { get; private set; }

    public static GoodsReceiptNumberSequence Create(int year) => year is >= 2000 and <= 9999
        ? new GoodsReceiptNumberSequence(year)
        : throw new ArgumentOutOfRangeException(nameof(year));

    public string ToNumber() => $"GR-{Year}-{Value:D6}";
}
