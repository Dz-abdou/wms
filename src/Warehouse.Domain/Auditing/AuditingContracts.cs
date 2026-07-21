namespace Warehouse.Domain.Auditing;

public interface IAuditableEntity
{
}

[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public sealed class AuditEntityAttribute : Attribute
{
    public bool SnapshotOnCreate { get; init; } = true;
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class AuditIgnoreAttribute : Attribute;

[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public sealed class AuditDisabledAttribute : Attribute;

public abstract class AuditableEntity : IAuditableEntity
{
}
