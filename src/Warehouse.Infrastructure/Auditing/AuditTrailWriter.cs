using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Warehouse.Infrastructure.Auditing;

public interface IAuditTrailWriter
{
    void Write(DbContext dbContext, IReadOnlyList<AuditRecord> records);

    Task WriteAsync(DbContext dbContext, IReadOnlyList<AuditRecord> records, CancellationToken cancellationToken);
}

public sealed class AuditTrailWriter : IAuditTrailWriter
{
    public void Write(DbContext dbContext, IReadOnlyList<AuditRecord> records)
    {
        foreach (var record in records)
        {
            using var command = CreateCommand(dbContext, record);
            command.ExecuteNonQuery();
        }
    }

    public async Task WriteAsync(DbContext dbContext, IReadOnlyList<AuditRecord> records, CancellationToken cancellationToken)
    {
        foreach (var record in records)
        {
            await using var command = CreateCommand(dbContext, record);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static DbCommand CreateCommand(DbContext dbContext, AuditRecord record)
    {
        var command = dbContext.Database.GetDbConnection().CreateCommand();
        command.Transaction = dbContext.Database.CurrentTransaction?.GetDbTransaction()
            ?? throw new InvalidOperationException("Audit records must be written inside a database transaction.");
        command.CommandText = $"""
            INSERT INTO {Qualify(record.Profile.Schema, record.Profile.TableName)}
                ("Id", "EntityId", "ActorUserId", "Action", "PropertyPath", "OldValue", "NewValue", "CorrelationId", "Reason")
            VALUES
                (@id, @entityId, @actorUserId, @action, @propertyPath, @oldValue, @newValue, @correlationId, @reason)
            """;

        AddParameter(command, "@id", Guid.NewGuid());
        AddParameter(command, "@entityId", record.EntityId);
        AddParameter(command, "@actorUserId", record.ActorUserId);
        AddParameter(command, "@action", record.Action.ToString());
        AddParameter(command, "@propertyPath", record.PropertyPath);
        AddParameter(command, "@oldValue", record.OldValue);
        AddParameter(command, "@newValue", record.NewValue);
        AddParameter(command, "@correlationId", record.CorrelationId);
        AddParameter(command, "@reason", record.Reason);
        return command;
    }

    private static void AddParameter(DbCommand command, string name, object? value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value ?? DBNull.Value;
        command.Parameters.Add(parameter);
    }

    private static string Qualify(string? schema, string tableName) => string.IsNullOrWhiteSpace(schema)
        ? Quote(tableName)
        : $"{Quote(schema)}.{Quote(tableName)}";

    private static string Quote(string identifier) => $"\"{identifier.Replace("\"", "\"\"")}\"";
}
