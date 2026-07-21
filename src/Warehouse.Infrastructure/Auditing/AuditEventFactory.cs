using Microsoft.EntityFrameworkCore.ChangeTracking;
using Warehouse.Application.Common.Auditing;

namespace Warehouse.Infrastructure.Auditing;

public interface IAuditEventFactory
{
    AuditRecord Create(
        AuditProfile profile,
        EntityEntry entry,
        AuditAction action,
        string propertyPath,
        object? oldValue,
        object? newValue,
        IAuditContext context);
}

public sealed class AuditEventFactory(IAuditValueSerializer serializer) : IAuditEventFactory
{
    public AuditRecord Create(
        AuditProfile profile,
        EntityEntry entry,
        AuditAction action,
        string propertyPath,
        object? oldValue,
        object? newValue,
        IAuditContext context)
    {
        var entityId = (Guid)(entry.Property("Id").CurrentValue
            ?? throw new InvalidOperationException($"Audited entity '{entry.Metadata.DisplayName()}' does not have an ID."));

        return new AuditRecord(
            profile,
            entityId,
            action,
            propertyPath,
            serializer.Serialize(oldValue),
            serializer.Serialize(newValue),
            context.ActorUserId,
            context.CorrelationId,
            context.Reason);
    }
}
