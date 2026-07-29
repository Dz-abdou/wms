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
            reason = Warehouse.Domain.Inventory.InventoryAdjustmentReason.StockCorrection,
            lines = new[] { new { productId = product.Id, warehouseId = warehouse.Id, quantity = 1m, direction = InventoryAdjustmentDirection.Decrease, unitOfMeasure = "EA" } }
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
    public async Task Inventory_overview_returns_filtered_on_hand_balances()
    {
        var categoryResponse = await fixture.Client.PostAsJsonAsync("/api/product-categories", new
        {
            code = $"INV-{Guid.NewGuid():N}"[..14],
            name = "Inventory overview category"
        });
        categoryResponse.EnsureSuccessStatusCode();
        var category = (await categoryResponse.Content.ReadFromJsonAsync<ProductCategoryResponse>())!;

        var productResponse = await fixture.Client.PostAsJsonAsync("/api/products", new
        {
            sku = $"OVR-{Guid.NewGuid():N}"[..14],
            name = "Inventory overview product",
            categoryId = category.Id
        });
        productResponse.EnsureSuccessStatusCode();
        var product = (await productResponse.Content.ReadFromJsonAsync<ProductResponse>())!;
        var warehouse = await CreateWarehouseAsync();
        await AdjustAsync(product.Id, warehouse.Id, 7m, InventoryAdjustmentDirection.Increase);

        var overview = await fixture.Client.GetFromJsonAsync<PagedResult<InventoryOverviewItemResponse>>(
            $"/api/inventory/overview?search={Uri.EscapeDataString(product.Sku)}&warehouseId={warehouse.Id}&categoryId={category.Id}&isActive=true&page=1&pageSize=20");

        Assert.NotNull(overview);
        var item = Assert.Single(overview.Items);
        Assert.Equal(product.Id, item.ProductId);
        Assert.Equal(product.Sku, item.ProductSku);
        Assert.Equal(warehouse.Id, item.WarehouseId);
        Assert.Equal(7m, item.Quantity);
        Assert.Equal("EA", item.BaseUnitOfMeasure);
        Assert.True(item.ProductIsActive);

        var deactivateResponse = await fixture.Client.PatchAsJsonAsync(
            $"/api/products/{product.Id}/status",
            new { isActive = false });
        deactivateResponse.EnsureSuccessStatusCode();

        var inactiveOverview = await fixture.Client.GetFromJsonAsync<PagedResult<InventoryOverviewItemResponse>>(
            $"/api/inventory/overview?categoryId={category.Id}&isActive=false&page=1&pageSize=20");
        Assert.NotNull(inactiveOverview);
        Assert.Contains(inactiveOverview.Items, item => item.ProductId == product.Id);
    }

    [Fact]
    public async Task Adjustment_documents_are_listed_detailed_and_linked_to_ledger_movements()
    {
        var product = await CreateProductAsync();
        var warehouse = await CreateWarehouseAsync();
        var adjustment = await CreateAdjustmentAsync(
            product.Id,
            warehouse.Id,
            3m,
            InventoryAdjustmentDirection.Increase,
            "COUNT-2026-001");

        var list = await fixture.Client.GetFromJsonAsync<PagedResult<InventoryAdjustmentListItemResponse>>(
            "/api/inventory/adjustments?page=1&pageSize=20");
        Assert.NotNull(list);
        var listItem = Assert.Single(list.Items.Where(item => item.Id == adjustment.Id));
        Assert.Equal("COUNT-2026-001", listItem.Reference);
        Assert.Equal(1, listItem.LineCount);

        var detail = await fixture.Client.GetFromJsonAsync<InventoryAdjustmentDetailResponse>(
            $"/api/inventory/adjustments/{adjustment.Id}");
        Assert.NotNull(detail);
        var line = Assert.Single(detail.Lines);
        Assert.Equal(product.Sku, line.ProductSku);
        Assert.Equal(warehouse.Code, line.WarehouseCode);
        Assert.Equal("ManualIncrease", line.Type);

        var ledger = await fixture.Client.GetFromJsonAsync<PagedResult<InventoryMovementResponse>>(
            "/api/inventory/movements?type=ManualIncrease&reference=COUNT-2026-001&page=1&pageSize=20");
        Assert.NotNull(ledger);
        var movement = Assert.Single(ledger.Items.Where(item => item.InventoryAdjustmentId == adjustment.Id));
        Assert.Equal(product.Name, movement.ProductName);
        Assert.Equal(warehouse.Name, movement.WarehouseName);
        Assert.Equal("COUNT-2026-001", movement.AdjustmentReference);

        var missing = await fixture.Client.GetAsync($"/api/inventory/adjustments/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        var problem = await missing.Content.ReadFromJsonAsync<ProblemResponse>();
        Assert.NotNull(problem);
        Assert.Equal(ApiErrorCodes.InventoryAdjustmentNotFound, problem.Code);
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
            reason = Warehouse.Domain.Inventory.InventoryAdjustmentReason.StockCorrection,
            lines = new[] { new { productId = product.Id, warehouseId = warehouse.Id, quantity = 1.1m, direction = InventoryAdjustmentDirection.Increase, unitOfMeasure = "CTN" } }
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
            reason = Warehouse.Domain.Inventory.InventoryAdjustmentReason.StockCorrection,
            lines = new[] { new { productId, warehouseId, quantity, direction, unitOfMeasure } }
        });
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<InventoryAdjustmentResponse>())!.Lines.Single();
    }

    private async Task<InventoryAdjustmentResponse> CreateAdjustmentAsync(
        Guid productId,
        Guid warehouseId,
        decimal quantity,
        InventoryAdjustmentDirection direction,
        string reference)
    {
        var response = await fixture.Client.PostAsJsonAsync("/api/inventory/adjustments", new
        {
            reason = Warehouse.Domain.Inventory.InventoryAdjustmentReason.StockCorrection,
            reference,
            lines = new[] { new { productId, warehouseId, quantity, direction, unitOfMeasure = "EA" } }
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<InventoryAdjustmentResponse>())!;
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
