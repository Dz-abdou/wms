namespace Warehouse.Domain.Common;

public abstract class PersistentEntity
{
    protected PersistentEntity(
        Guid id,
        DateTime createdAtUtc,
        DateTime updatedAtUtc,
        Guid? createdByUserId = null,
        Guid? updatedByUserId = null)
    {
        Id = id;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
        CreatedByUserId = createdByUserId;
        UpdatedByUserId = updatedByUserId;
    }

    public Guid Id { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; protected set; }

    public Guid? CreatedByUserId { get; private set; }

    public Guid? UpdatedByUserId { get; protected set; }

    protected void SetUpdatedByUser(Guid? userId)
    {
        UpdatedByUserId = userId;
    }
}
