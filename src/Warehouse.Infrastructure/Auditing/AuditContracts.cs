using Warehouse.Application.Common.Auditing;
using Warehouse.Application.Common.Identity;
using Warehouse.Domain.Common;

namespace Warehouse.Infrastructure.Auditing;

public enum AuditAction
{
    Create,
    Update,
    Delete
}

public sealed class AuditTrail<TEntity>
    where TEntity : PersistentEntity
{
    public Guid Id { get; private set; }

    public Guid EntityId { get; private set; }

    public DateTime ChangedAtUtc { get; private set; }

    public Guid? ActorUserId { get; private set; }

    public AuditAction Action { get; private set; }

    public string PropertyPath { get; private set; } = null!;

    public string? OldValue { get; private set; }

    public string? NewValue { get; private set; }

    public string? CorrelationId { get; private set; }

    public string? Reason { get; private set; }
}

public sealed record AuditRecord(
    AuditProfile Profile,
    Guid EntityId,
    AuditAction Action,
    string PropertyPath,
    string? OldValue,
    string? NewValue,
    Guid? ActorUserId,
    string? CorrelationId,
    string? Reason);

public sealed record AuditEventContext(
    Guid? ActorUserId,
    string? CorrelationId,
    string? Reason)
{
    public static AuditEventContext Create(ICurrentUser currentUser, IAuditContext auditContext) => new(
        currentUser.UserId,
        auditContext.CorrelationId,
        auditContext.Reason);
}
