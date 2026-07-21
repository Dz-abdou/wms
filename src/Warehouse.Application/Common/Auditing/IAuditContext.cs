namespace Warehouse.Application.Common.Auditing;

public interface IAuditContext
{
    Guid? ActorUserId { get; }

    string? CorrelationId { get; }

    string? Reason { get; }
}

public sealed record AuditContext(
    Guid? ActorUserId,
    string? CorrelationId,
    string? Reason = null) : IAuditContext;
