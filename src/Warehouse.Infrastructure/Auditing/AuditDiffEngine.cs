using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Warehouse.Infrastructure.Auditing;

public interface IAuditDiffEngine
{
    AuditCapture Capture(ChangeTracker changeTracker, AuditEventContext context);

    IReadOnlyList<AuditRecord> CreateSnapshots(AuditCapture capture, AuditEventContext context);
}

public sealed class AuditDiffEngine(IAuditProfileProvider profiles, IAuditEventFactory eventFactory) : IAuditDiffEngine
{
    public AuditCapture Capture(ChangeTracker changeTracker, AuditEventContext context)
    {
        var records = new List<AuditRecord>();
        var createEntries = new List<AuditedEntry>();

        foreach (var entry in changeTracker.Entries())
        {
            if (entry.State is not (Microsoft.EntityFrameworkCore.EntityState.Added or Microsoft.EntityFrameworkCore.EntityState.Modified or Microsoft.EntityFrameworkCore.EntityState.Deleted))
            {
                continue;
            }

            var profile = profiles.GetProfile(entry);
            if (profile is null)
            {
                continue;
            }

            if (entry.State == Microsoft.EntityFrameworkCore.EntityState.Added)
            {
                if (profile.SnapshotOnCreate)
                {
                    createEntries.Add(new AuditedEntry(entry, profile));
                }

                continue;
            }

            if (entry.State == Microsoft.EntityFrameworkCore.EntityState.Deleted)
            {
                records.Add(CreateRecord(profile, entry, AuditAction.Delete, "__deleted__", null, null, context));
                continue;
            }

            foreach (var property in entry.Properties.Where(property =>
                         property.IsModified
                         && !property.Metadata.IsPrimaryKey()
                         && profile.Includes(property.Metadata)
                         && !Equals(property.OriginalValue, property.CurrentValue)))
            {
                records.Add(CreateRecord(profile, entry, AuditAction.Update, property.Metadata.Name, property.OriginalValue, property.CurrentValue, context));
            }
        }

        return new AuditCapture(records, createEntries);
    }

    public IReadOnlyList<AuditRecord> CreateSnapshots(AuditCapture capture, AuditEventContext context)
    {
        var records = new List<AuditRecord>(capture.Records.Count + capture.CreateEntries.Count);
        records.AddRange(capture.Records);

        foreach (var auditableEntry in capture.CreateEntries)
        {
            foreach (var property in auditableEntry.Entry.Properties.Where(property => auditableEntry.Profile.Includes(property.Metadata)))
            {
                records.Add(CreateRecord(auditableEntry.Profile, auditableEntry.Entry, AuditAction.Create, property.Metadata.Name, null, property.CurrentValue, context));
            }
        }

        return records;
    }

    private AuditRecord CreateRecord(AuditProfile profile, EntityEntry entry, AuditAction action, string propertyPath, object? oldValue, object? newValue, AuditEventContext context) => eventFactory.Create(profile, entry, action, propertyPath, oldValue, newValue, context);
}

public sealed record AuditCapture(IReadOnlyList<AuditRecord> Records, IReadOnlyList<AuditedEntry> CreateEntries)
{
    public bool HasAuditWork => Records.Count > 0 || CreateEntries.Count > 0;
}

public sealed record AuditedEntry(EntityEntry Entry, AuditProfile Profile);
