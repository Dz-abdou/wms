using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.Application.Common.Errors;
using Warehouse.Application.Common.Models;
using Warehouse.Application.Inventory;
using Warehouse.Application.Products;
using Warehouse.Application.Warehouses;
using Warehouse.Domain.Inventory;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.IntegrationTests;

[Collection(ProductApiCollection.Name)]
public sealed class InventoryEndpointTests(ProductApiFixture fixture)
{
    [Fact]
    public async Task Manual_adjustments_update_the_balance_and_create_one_movement_each()
    {
        var product = await CreateProductAsync();
        var warehouse = await CreateWarehouseAsync();

        var increase = await AdjustAsync(product.Id, warehouse.Id, 5m, InventoryAdjustmentDirection.Increase);
        var decrease = await AdjustAsync(product.Id, warehouse.Id, 2m, InventoryAdjustmentDirection.Decrease);

        Assert.Equal(5m, increase.Quantity);
        Assert.Equal(3m, decrease.Quantity);

        using var scope = fixture.Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        var balance = await dbContext.InventoryBalances.SingleAsync(
            candidate => candidate.ProductId == product.Id && candidate.WarehouseId == warehouse.Id);
        var movements = await dbContext.InventoryMovements
            .Where(candidate => candidate.ProductId == product.Id && candidate.WarehouseId == warehouse.Id)
            .OrderBy(movement => movement.CreatedAtUtc).ToListAsync();

        Assert.Equal(3m, balance.Quantity);
        Assert.Equal(2, movements.Count);
        Assert.Equal(5m, movements[0].QuantityDelta);
        Assert.Equal(-2m, movements[1].QuantityDelta);
        Assert.Equal("EA", movements[0].UnitOfMeasure);
        Assert.Equal(-2m, movements[1].QuantityDeltaInUnit);
        Assert.Equal(3m, movements[1].BalanceAfter);
    }

    [Fact]
    public async Task Negative_adjustment_that_would_go_below_zero_writes_nothing()
    {
        var product = await CreateProductAsync();
        var warehouse = await CreateWarehouseAsync();

        var response = await fixture.Client.PostAsJsonAsync("/api/inventory/adjustments", new
        {
            productId = product.Id,
            warehouseId = warehouse.Id,
            quantity = 1m,
            direction = InventoryAdjustmentDirection.Decrease,
            unitOfMeasure = "EA"
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemResponse>();
        Assert.NotNull(problem);
        Assert.Equal(ApiErrorCodes.InventoryInsufficientStock, problem.Code);

        using var scope = fixture.Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        Assert.False(await dbContext.InventoryBalances.AnyAsync(balance => balance.ProductId == product.Id));
        Assert.False(await dbContext.InventoryMovements.AnyAsync(movement => movement.ProductId == product.Id));
    }

    [Fact]
    public async Task Movement_history_filters_by_product_and_warehouse()
    {
        var product = await CreateProductAsync();
        var warehouse = await CreateWarehouseAsync();
        await AdjustAsync(product.Id, warehouse.Id, 3m, InventoryAdjustmentDirection.Increase);

        var history = await fixture.Client.GetFromJsonAsync<PagedResult<InventoryMovementResponse>>(
            $"/api/inventory/movements?productId={product.Id}&warehouseId={warehouse.Id}");

        Assert.NotNull(history);
        Assert.Single(history.Items);
        Assert.Equal(product.Id, history.Items[0].ProductId);
        Assert.Equal(warehouse.Id, history.Items[0].WarehouseId);
        Assert.Equal("ManualIncrease", history.Items[0].Type);
    }
    [Fact]
    public async Task Manual_adjustment_converts_valid_product_units_and_rejects_fractional_cartons()
    {
        var createProductResponse = await fixture.Client.PostAsJsonAsync("/api/products", new
        {
            sku = $"UOM-{Guid.NewGuid():N}"[..14],
            name = "Packaged inventory product",
            baseUnitOfMeasure = "EA",
            unitConversions = new[]
            {
                new { unitOfMeasure = "CTN", quantityInBaseUnit = 24m, allowsFractionalQuantity = false }
            }
        });
        createProductResponse.EnsureSuccessStatusCode();
        var product = (await createProductResponse.Content.ReadFromJsonAsync<ProductResponse>())!;
        var warehouse = await CreateWarehouseAsync();

        var invalidResponse = await fixture.Client.PostAsJsonAsync("/api/inventory/adjustments", new
        {
            productId = product.Id,
            warehouseId = warehouse.Id,
            quantity = 1.1m,
            direction = InventoryAdjustmentDirection.Increase,
            unitOfMeasure = "CTN"
        });

        Assert.Equal(HttpStatusCode.BadRequest, invalidResponse.StatusCode);
        var problem = await invalidResponse.Content.ReadFromJsonAsync<ProblemResponse>();
        Assert.NotNull(problem);
        Assert.Equal(ApiErrorCodes.InventoryInvalidUnitOfMeasure, problem.Code);

        var result = await AdjustAsync(product.Id, warehouse.Id, 2m, InventoryAdjustmentDirection.Increase, "CTN");
        Assert.Equal(48m, result.Quantity);
        Assert.Equal("EA", result.BaseUnitOfMeasure);

        using var scope = fixture.Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        var movement = await dbContext.InventoryMovements.SingleAsync(candidate => candidate.ProductId == product.Id);
        Assert.Equal("CTN", movement.UnitOfMeasure);
        Assert.Equal(2m, movement.QuantityDeltaInUnit);
        Assert.Equal(48m, movement.QuantityDelta);
    }


    private async Task<InventoryBalanceResponse> AdjustAsync(
        Guid productId,
        Guid warehouseId,
        decimal quantity,
        InventoryAdjustmentDirection direction,
        string unitOfMeasure = "EA")
    {
        var response = await fixture.Client.PostAsJsonAsync("/api/inventory/adjustments", new
        {
            productId,
            warehouseId,
            quantity,
            direction,
            unitOfMeasure
        });
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<InventoryBalanceResponse>())!;
    }

    private async Task<ProductResponse> CreateProductAsync()
    {
        var response = await fixture.Client.PostAsJsonAsync("/api/products", new
        {
            sku = $"INV-{Guid.NewGuid():N}"[..14],
            name = "Inventory product"
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ProductResponse>())!;
    }

    private async Task<WarehouseResponse> CreateWarehouseAsync()
    {
        var response = await fixture.Client.PostAsJsonAsync("/api/warehouses", new
        {
            code = $"INV-{Guid.NewGuid():N}"[..14],
            name = "Inventory warehouse"
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<WarehouseResponse>())!;
    }

    private sealed record ProblemResponse(string? Code);
}
