using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Warehouse.Application.Common.Auditing;
using Warehouse.Application.Common.Identity;

namespace Warehouse.Infrastructure.Auditing;

public sealed class AuditSavePipeline(
    IAuditDiffEngine diffEngine,
    IAuditTrailWriter writer,
    IAuditContext auditContext,
    ICurrentUser currentUser)
{
    public int Save(DbContext dbContext, bool acceptAllChangesOnSuccess, Func<bool, int> saveChanges)
    {
        dbContext.ChangeTracker.DetectChanges();
        var auditEventContext = AuditEventContext.Create(currentUser, auditContext);
        var capture = diffEngine.Capture(dbContext.ChangeTracker, auditEventContext);
        if (!capture.HasAuditWork)
        {
            return saveChanges(acceptAllChangesOnSuccess);
        }

        using var transactionScope = BeginTransactionScope(dbContext);
        try
        {
            var result = saveChanges(false);
            var records = diffEngine.CreateSnapshots(capture, auditEventContext);
            writer.Write(dbContext, records);
            transactionScope.Commit();

            if (acceptAllChangesOnSuccess)
            {
                dbContext.ChangeTracker.AcceptAllChanges();
            }

            return result;
        }
        catch
        {
            transactionScope.Rollback();
            throw;
        }
    }

    public async Task<int> SaveAsync(
        DbContext dbContext,
        bool acceptAllChangesOnSuccess,
        Func<bool, CancellationToken, Task<int>> saveChanges,
        CancellationToken cancellationToken)
    {
        dbContext.ChangeTracker.DetectChanges();
        var auditEventContext = AuditEventContext.Create(currentUser, auditContext);
        var capture = diffEngine.Capture(dbContext.ChangeTracker, auditEventContext);
        if (!capture.HasAuditWork)
        {
            return await saveChanges(acceptAllChangesOnSuccess, cancellationToken);
        }

        await using var transactionScope = await BeginTransactionScopeAsync(dbContext, cancellationToken);
        try
        {
            var result = await saveChanges(false, cancellationToken);
            var records = diffEngine.CreateSnapshots(capture, auditEventContext);
            await writer.WriteAsync(dbContext, records, cancellationToken);
            await transactionScope.CommitAsync(cancellationToken);

            if (acceptAllChangesOnSuccess)
            {
                dbContext.ChangeTracker.AcceptAllChanges();
            }

            return result;
        }
        catch
        {
            await transactionScope.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static TransactionScope BeginTransactionScope(DbContext dbContext)
    {
        if (dbContext.Database.CurrentTransaction is { } transaction)
        {
            var savepoint = $"audit_{Guid.NewGuid():N}";
            transaction.CreateSavepoint(savepoint);
            return TransactionScope.ForSavepoint(transaction, savepoint);
        }

        return TransactionScope.ForTransaction(dbContext.Database.BeginTransaction());
    }

    private static async Task<TransactionScope> BeginTransactionScopeAsync(DbContext dbContext, CancellationToken cancellationToken)
    {
        if (dbContext.Database.CurrentTransaction is { } transaction)
        {
            var savepoint = $"audit_{Guid.NewGuid():N}";
            await transaction.CreateSavepointAsync(savepoint, cancellationToken);
            return TransactionScope.ForSavepoint(transaction, savepoint);
        }

        return TransactionScope.ForTransaction(await dbContext.Database.BeginTransactionAsync(cancellationToken));
    }

    private sealed class TransactionScope(IDbContextTransaction transaction, string? savepoint, bool ownsTransaction)
        : IDisposable, IAsyncDisposable
    {
        public static TransactionScope ForTransaction(IDbContextTransaction transaction) => new(transaction, null, true);

        public static TransactionScope ForSavepoint(IDbContextTransaction transaction, string savepoint) => new(transaction, savepoint, false);

        public void Commit()
        {
            if (ownsTransaction)
            {
                transaction.Commit();
            }
            else
            {
                transaction.ReleaseSavepoint(savepoint!);
            }
        }

        public Task CommitAsync(CancellationToken cancellationToken) => ownsTransaction
            ? transaction.CommitAsync(cancellationToken)
            : transaction.ReleaseSavepointAsync(savepoint!, cancellationToken);

        public void Rollback()
        {
            if (ownsTransaction)
            {
                transaction.Rollback();
            }
            else
            {
                transaction.RollbackToSavepoint(savepoint!);
            }
        }

        public Task RollbackAsync(CancellationToken cancellationToken) => ownsTransaction
            ? transaction.RollbackAsync(cancellationToken)
            : transaction.RollbackToSavepointAsync(savepoint!, cancellationToken);

        public void Dispose()
        {
            if (ownsTransaction)
            {
                transaction.Dispose();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (ownsTransaction)
            {
                await transaction.DisposeAsync();
            }
        }
    }
}
