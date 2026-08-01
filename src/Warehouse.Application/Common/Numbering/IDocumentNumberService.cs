namespace Warehouse.Application.Common.Numbering;

public interface IDocumentNumberService
{
    Task<string> AllocateAsync(string definitionCode, DateTime occurredAtUtc, CancellationToken cancellationToken);
}
