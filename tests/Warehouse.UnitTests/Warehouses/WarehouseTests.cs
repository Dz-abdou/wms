using Warehouse.Domain.Warehouses;
using WarehouseEntity = Warehouse.Domain.Warehouses.Warehouse;

namespace Warehouse.UnitTests.Warehouses;

public sealed class WarehouseTests
{
    private static readonly DateTime CreatedAtUtc = new(2026, 7, 21, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_normalizes_code_and_trims_text_fields()
    {
        var warehouse = WarehouseEntity.Create(" main-01 ", " Main warehouse ", " Description ", CreatedAtUtc);

        Assert.Equal("MAIN-01", warehouse.Code);
        Assert.Equal("Main warehouse", warehouse.Name);
        Assert.Equal("Description", warehouse.Description);
        Assert.True(warehouse.IsActive);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_blank_code(string? code) =>
        Assert.Throws<ArgumentException>(() => WarehouseEntity.Create(code, "Warehouse", null, CreatedAtUtc));

    [Fact]
    public void Create_rejects_values_above_supported_lengths()
    {
        Assert.Throws<ArgumentException>(() => WarehouseEntity.Create(new string('W', WarehouseRules.MaxCodeLength + 1), "Warehouse", null, CreatedAtUtc));
        Assert.Throws<ArgumentException>(() => WarehouseEntity.Create("MAIN", new string('W', WarehouseRules.MaxNameLength + 1), null, CreatedAtUtc));
    }

    [Fact]
    public void Set_status_is_idempotent()
    {
        var warehouse = WarehouseEntity.Create("MAIN", "Warehouse", null, CreatedAtUtc);
        var changedAtUtc = CreatedAtUtc.AddMinutes(1);
        warehouse.SetStatus(false, changedAtUtc);
        warehouse.SetStatus(false, changedAtUtc.AddMinutes(1));

        Assert.False(warehouse.IsActive);
        Assert.Equal(changedAtUtc, warehouse.UpdatedAtUtc);
    }
    [Fact]
    public void Persistent_metadata_tracks_create_update_and_no_op_status_actors()
    {
        var creatorId = Guid.NewGuid();
        var warehouse = WarehouseEntity.Create("MAIN", "Warehouse", null, CreatedAtUtc, creatorId);

        Assert.Equal(creatorId, warehouse.CreatedByUserId);
        Assert.Equal(creatorId, warehouse.UpdatedByUserId);

        var modifierId = Guid.NewGuid();
        var updatedAtUtc = CreatedAtUtc.AddMinutes(5);
        warehouse.Update("SECONDARY", "Updated", null, updatedAtUtc, modifierId);

        Assert.Equal(updatedAtUtc, warehouse.UpdatedAtUtc);
        Assert.Equal(modifierId, warehouse.UpdatedByUserId);

        var statusActorId = Guid.NewGuid();
        var statusChangedAtUtc = updatedAtUtc.AddMinutes(5);
        warehouse.SetStatus(false, statusChangedAtUtc, statusActorId);
        var noOpActorId = Guid.NewGuid();
        warehouse.SetStatus(false, statusChangedAtUtc.AddMinutes(5), noOpActorId);

        Assert.Equal(statusChangedAtUtc, warehouse.UpdatedAtUtc);
        Assert.Equal(statusActorId, warehouse.UpdatedByUserId);
    }

}