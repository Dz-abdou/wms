using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.Application.Inventory;
using Warehouse.Domain.Inventory;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.IntegrationTests;

[Collection(ProductApiCollection.Name)]
public sealed class InventoryConcurrencyTests(ProductApiFixture fixture)
{
    [Fact]
    public async Task Concurrent_balance_updates_raise_an_optimistic_concurrency_exception()
    {
        var productId = await CreateProductAsync();
        var warehouseId = await CreateWarehouseAsync();
        await fixture.Client.PostAsJsonAsync("/api/inventory/adjustments", new
        {
            productId,
            warehouseId,
            quantity = 1m,
            direction = InventoryAdjustmentDirection.Increase
        });

        using var firstScope = fixture.Factory.Services.CreateScope();
        using var secondScope = fixture.Factory.Services.CreateScope();
        var firstContext = firstScope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        var secondContext = secondScope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        var firstBalance = await firstContext.InventoryBalances.SingleAsync();
        var secondBalance = await secondContext.InventoryBalances.SingleAsync();

        firstBalance.ApplyAdjustment(1m, DateTime.UtcNow);
        secondBalance.ApplyAdjustment(1m, DateTime.UtcNow);

        await firstContext.SaveChangesAsync();
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => secondContext.SaveChangesAsync());
    }

    private async Task<Guid> CreateProductAsync()
    {
        var response = await fixture.Client.PostAsJsonAsync("/api/products", new
        {
            sku = $"CON-{Guid.NewGuid():N}"[..14],
            name = "Concurrency product"
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Warehouse.Application.Products.ProductResponse>())!.Id;
    }

    private async Task<Guid> CreateWarehouseAsync()
    {
        var response = await fixture.Client.PostAsJsonAsync("/api/warehouses", new
        {
            code = $"CON-{Guid.NewGuid():N}"[..14],
            name = "Concurrency warehouse"
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Warehouse.Application.Warehouses.WarehouseResponse>())!.Id;
    }
}
