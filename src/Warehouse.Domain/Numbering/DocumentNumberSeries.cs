namespace Warehouse.Domain.Numbering;

public sealed class DocumentNumberSeries
{
    private DocumentNumberSeries(string definitionCode, int year)
    {
        DefinitionCode = definitionCode;
        Year = year;
        NextValue = 1;
    }

    public Guid Id { get; private set; }

    public string DefinitionCode { get; private set; } = null!;

    public int Year { get; private set; }

    public long NextValue { get; private set; }

    public static DocumentNumberSeries Create(string definitionCode, int year)
    {
        if (string.IsNullOrWhiteSpace(definitionCode))
        {
            throw new ArgumentException("A document-number definition code is required.", nameof(definitionCode));
        }

        if (year is < 2000 or > 9999)
        {
            throw new ArgumentOutOfRangeException(nameof(year));
        }

        return new DocumentNumberSeries(definitionCode.Trim().ToUpperInvariant(), year)
        {
            Id = Guid.NewGuid()
        };
    }
}
