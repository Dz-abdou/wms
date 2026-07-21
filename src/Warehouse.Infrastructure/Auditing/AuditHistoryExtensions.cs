using Microsoft.EntityFrameworkCore;
using Warehouse.Domain.Common;

namespace Warehouse.Infrastructure.Auditing;

public static class AuditHistoryExtensions
{
    public static IQueryable<AuditTrail<TEntity>> AuditHistory<TEntity>(this DbContext dbContext, Guid entityId)
        where TEntity : PersistentEntity => dbContext.Set<AuditTrail<TEntity>>()
            .AsNoTracking()
            .Where(trail => trail.EntityId == entityId)
            .OrderByDescending(trail => trail.ChangedAtUtc)
            .ThenByDescending(trail => trail.Id);
}
