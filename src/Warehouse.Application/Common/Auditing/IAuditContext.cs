namespace Warehouse.Application.Common.Auditing;

public interface IAuditContext
{
    string? CorrelationId { get; }

    string? Reason { get; }
}

public sealed record AuditContext(
    string? CorrelationId,
    string? Reason = null) : IAuditContext;
