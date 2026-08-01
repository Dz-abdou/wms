namespace Warehouse.Application.Common.Numbering;

public sealed class DocumentNumberDefinitionUnavailableException(string definitionCode)
    : Exception($"Document-number definition '{definitionCode}' is missing or inactive.");

public sealed class DocumentNumberCapacityExceededException(string definitionCode, int year)
    : Exception($"Document-number definition '{definitionCode}' has no available value for {year}.");
