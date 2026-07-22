using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Warehouse.Domain.Auditing;
using Warehouse.Domain.Common;

namespace Warehouse.Infrastructure.Auditing;

public sealed class AuditProfile
{
    private static readonly HashSet<string> MetadataProperties = new(StringComparer.Ordinal)
    {
        nameof(PersistentEntity.CreatedAtUtc),
        nameof(PersistentEntity.UpdatedAtUtc),
        nameof(PersistentEntity.CreatedByUserId),
        nameof(PersistentEntity.UpdatedByUserId)
    };

    private static readonly string[] SensitiveNameFragments =
    [
        "password", "token", "secret", "hash", "credential", "securitystamp"
    ];

    public AuditProfile(IEntityType entityType, AuditEntityAttribute options, string tableName, string? schema)
    {
        EntityType = entityType;
        SnapshotOnCreate = options.SnapshotOnCreate;
        TableName = tableName;
        Schema = schema;
    }

    public IEntityType EntityType { get; }

    public bool SnapshotOnCreate { get; }

    public string TableName { get; }

    public string? Schema { get; }

    public bool Includes(IProperty property)
    {
        if (property.IsShadowProperty() || MetadataProperties.Contains(property.Name) || IsSensitivePropertyName(property.Name))
        {
            return false;
        }

        var propertyInfo = property.PropertyInfo;
        return propertyInfo?.GetCustomAttribute<AuditIgnoreAttribute>(inherit: true) is null;
    }

    public static bool IsSensitivePropertyName(string name) => SensitiveNameFragments.Any(fragment =>
        name.Contains(fragment, StringComparison.OrdinalIgnoreCase));
}

public interface IAuditProfileProvider
{
    AuditProfile? GetProfile(EntityEntry entry);

    bool IsEnabled(IReadOnlyEntityType entityType);
}

public sealed class AuditProfileProvider : IAuditProfileProvider
{
    public AuditProfile? GetProfile(EntityEntry entry)
    {
        var entityType = entry.Metadata;
        if (!IsEnabled(entityType))
        {
            return null;
        }

        var key = entityType.FindPrimaryKey();
        if (key is null || key.Properties.Count != 1 || key.Properties[0].ClrType != typeof(Guid))
        {
            throw new InvalidOperationException($"Audited entity '{entityType.DisplayName()}' must have exactly one Guid primary key.");
        }

        if (entityType.GetNavigations().Any(navigation => navigation.TargetEntityType.IsOwned()))
        {
            throw new InvalidOperationException($"Audited entity '{entityType.DisplayName()}' cannot contain owned navigations until owned-property auditing is explicitly configured.");
        }

        var tableName = entityType.GetTableName()
            ?? throw new InvalidOperationException($"Audited entity '{entityType.DisplayName()}' must be mapped to a table.");
        var options = entityType.ClrType.GetCustomAttribute<AuditEntityAttribute>(inherit: true)!;

        return new AuditProfile(entityType, options, $"{tableName}_AuditTrails", entityType.GetSchema());
    }

    public bool IsEnabled(IReadOnlyEntityType entityType)
    {
        if (entityType.IsOwned() || !typeof(PersistentEntity).IsAssignableFrom(entityType.ClrType))
        {
            return false;
        }

        return entityType.ClrType.GetCustomAttribute<AuditEntityAttribute>(inherit: true) is not null
            && entityType.ClrType.GetCustomAttribute<AuditDisabledAttribute>(inherit: true) is null;
    }
}
