using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Warehouse.Domain.Common;

namespace Warehouse.Infrastructure.Auditing;

public static class AuditTrailModelBuilderExtensions
{
    private static readonly MethodInfo ConfigureMethod = typeof(AuditTrailModelBuilderExtensions)
        .GetMethod(nameof(ConfigureAuditTrail), BindingFlags.NonPublic | BindingFlags.Static)!;

    public static ModelBuilder ApplyAuditTrailMappings(this ModelBuilder modelBuilder)
    {
        var profiles = new AuditProfileProvider();
        foreach (var entityType in modelBuilder.Model.GetEntityTypes().ToArray())
        {
            if (!profiles.IsEnabled(entityType))
            {
                continue;
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
            ConfigureMethod.MakeGenericMethod(entityType.ClrType)
                .Invoke(null, [modelBuilder, $"{tableName}_AuditTrails", entityType.GetSchema()]);
        }

        return modelBuilder;
    }

    private static void ConfigureAuditTrail<TEntity>(ModelBuilder modelBuilder, string tableName, string? schema)
        where TEntity : PersistentEntity
    {
        var builder = modelBuilder.Entity<AuditTrail<TEntity>>();
        builder.ToTable(tableName, schema);
        builder.HasKey(trail => trail.Id);
        builder.Property(trail => trail.Id).ValueGeneratedNever();
        builder.Property(trail => trail.EntityId).HasColumnType("uuid").IsRequired();
        builder.Property(trail => trail.ChangedAtUtc)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("transaction_timestamp()")
            .ValueGeneratedOnAdd()
            .IsRequired();
        builder.Property(trail => trail.ActorUserId).HasColumnType("uuid");
        builder.Property(trail => trail.Action).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(trail => trail.PropertyPath).HasMaxLength(256).IsRequired();
        builder.Property(trail => trail.OldValue).HasColumnType("text");
        builder.Property(trail => trail.NewValue).HasColumnType("text");
        builder.Property(trail => trail.CorrelationId).HasMaxLength(256);
        builder.Property(trail => trail.Reason).HasMaxLength(1024);
        builder.HasIndex(trail => new { trail.EntityId, trail.ChangedAtUtc });
    }
}
