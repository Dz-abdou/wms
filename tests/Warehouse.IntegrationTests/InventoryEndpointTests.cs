using System.Net;
using System.Net.Http.Json;
using System.Text;
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
    public async Task Manual_adjustment_accepts_string_enum_values_from_the_frontend()
    {
        var product = await CreateProductAsync();
        var warehouse = await CreateWarehouseAsync();
        var requestBody = $$"""
        {
          "reason": "StockCorrection",
          "lines": [
            {
              "productId": "{{product.Id}}",
              "warehouseId": "{{warehouse.Id}}",
              "quantity": 2,
              "direction": "Increase",
              "unitOfMeasure": "EA"
            }
          ]
        }
        """;

        var response = await fixture.Client.PostAsync(
            "/api/inventory/adjustments",
            new StringContent(requestBody, Encoding.UTF8, "application/json"));

        response.EnsureSuccessStatusCode();
        var adjustment = await response.Content.ReadFromJsonAsync<InventoryAdjustmentResponse>();
        Assert.NotNull(adjustment);
        Assert.Equal(InventoryAdjustmentReason.StockCorrection, adjustment.Reason);
        Assert.Equal(2m, Assert.Single(adjustment.Lines).Quantity);
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

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemResponse>();
        Assert.NotNull(problem);
        Assert.Equal(ApiErrorCodes.InventoryInsufficientStock, problem.Code);
        Assert.Equal(
            ApiErrorCodes.InventoryInsufficientStock,
            Assert.Single(problem.ErrorCodes!["Lines[0].Quantity"]));
        Assert.Contains("Lines[0].Quantity", problem.Errors!.Keys);

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

    [Fact]
    public async Task Cycle_count_posts_only_variances_and_links_them_to_the_ledger()
    {
        var product = await CreateProductAsync();
        var warehouse = await CreateWarehouseAsync();
        await IncreaseAsync(product.Id, warehouse.Id, 10m);
        var candidate = await fixture.Client.GetFromJsonAsync<CycleCountCandidateResponse>(
            $"/api/inventory/cycle-counts/candidate?warehouseId={warehouse.Id}&productId={product.Id}");

        Assert.NotNull(candidate);
        Assert.Equal(10m, candidate.SystemQuantityInBase);
        var response = await fixture.Client.PostAsJsonAsync("/api/inventory/cycle-counts", new CycleCountInput(
            warehouse.Id,
            "CC-2026-001",
            "Monthly verification",
            [new CycleCountLineInput(product.Id, candidate.SystemQuantityInBase, candidate.SystemBalanceVersion, "EA", 7m)]));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var cycleCount = await response.Content.ReadFromJsonAsync<CycleCountDetailResponse>();
        Assert.NotNull(cycleCount);
        var line = Assert.Single(cycleCount.Lines);
        Assert.Equal(-3m, line.VarianceQuantityInBase);
        Assert.NotNull(line.InventoryMovementId);

        using var scope = fixture.Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        var balance = await dbContext.InventoryBalances.SingleAsync(candidate =>
            candidate.ProductId == product.Id && candidate.WarehouseId == warehouse.Id);
        var movement = await dbContext.InventoryMovements.SingleAsync(candidate =>
            candidate.CycleCountId == cycleCount.Id);

        Assert.Equal(7m, balance.Quantity);
        Assert.Equal(InventoryMovementType.CycleCountDecrease, movement.Type);
        Assert.Equal(-3m, movement.QuantityDelta);
        Assert.Equal(cycleCount.Id, movement.CycleCountId);

        var history = await fixture.Client.GetFromJsonAsync<PagedResult<InventoryMovementResponse>>(
            "/api/inventory/movements?type=CycleCountDecrease&reference=CC-2026-001&page=1&pageSize=20");
        Assert.NotNull(history);
        var historyItem = Assert.Single(history.Items.Where(item => item.CycleCountId == cycleCount.Id));
        Assert.Equal("CC-2026-001", historyItem.CycleCountReference);

        var list = await fixture.Client.GetFromJsonAsync<PagedResult<CycleCountListItemResponse>>(
            "/api/inventory/cycle-counts?page=1&pageSize=20");
        Assert.NotNull(list);
        var listItem = Assert.Single(list.Items.Where(item => item.Id == cycleCount.Id));
        Assert.Equal(1, listItem.LineCount);
        Assert.Equal(1, listItem.VarianceLineCount);
    }

    [Fact]
    public async Task Cycle_count_rejects_a_line_when_the_stock_snapshot_is_stale()
    {
        var product = await CreateProductAsync();
        var warehouse = await CreateWarehouseAsync();
        await IncreaseAsync(product.Id, warehouse.Id, 2m);
        var candidate = await fixture.Client.GetFromJsonAsync<CycleCountCandidateResponse>(
            $"/api/inventory/cycle-counts/candidate?warehouseId={warehouse.Id}&productId={product.Id}");

        Assert.NotNull(candidate);
        await IncreaseAsync(product.Id, warehouse.Id, 1m);
        var response = await fixture.Client.PostAsJsonAsync("/api/inventory/cycle-counts", new CycleCountInput(
            warehouse.Id,
            null,
            null,
            [new CycleCountLineInput(product.Id, candidate.SystemQuantityInBase, candidate.SystemBalanceVersion, "EA", 2m)]));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemResponse>();
        Assert.NotNull(problem);
        Assert.Equal(ApiErrorCodes.InventoryCycleCountStaleBalance, problem.Code);
        Assert.Equal(
            ApiErrorCodes.InventoryCycleCountStaleBalance,
            Assert.Single(problem.ErrorCodes!["Lines[0].SystemQuantityInBase"]));

        using var scope = fixture.Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        Assert.False(await dbContext.CycleCounts.AnyAsync(count => count.WarehouseId == warehouse.Id));
    }

    [Fact]
    public async Task Cycle_count_records_an_exact_count_without_a_zero_movement()
    {
        var product = await CreateProductAsync();
        var warehouse = await CreateWarehouseAsync();
        await IncreaseAsync(product.Id, warehouse.Id, 4m);
        var candidate = await fixture.Client.GetFromJsonAsync<CycleCountCandidateResponse>(
            $"/api/inventory/cycle-counts/candidate?warehouseId={warehouse.Id}&productId={product.Id}");

        Assert.NotNull(candidate);
        var response = await fixture.Client.PostAsJsonAsync("/api/inventory/cycle-counts", new CycleCountInput(
            warehouse.Id,
            null,
            null,
            [new CycleCountLineInput(product.Id, candidate.SystemQuantityInBase, candidate.SystemBalanceVersion, "EA", 4m)]));

        response.EnsureSuccessStatusCode();
        var cycleCount = await response.Content.ReadFromJsonAsync<CycleCountDetailResponse>();
        Assert.NotNull(cycleCount);
        var line = Assert.Single(cycleCount.Lines);
        Assert.Equal(0m, line.VarianceQuantityInBase);
        Assert.Null(line.InventoryMovementId);

        using var scope = fixture.Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        Assert.False(await dbContext.InventoryMovements.AnyAsync(movement => movement.CycleCountId == cycleCount.Id));
    }


    [Fact]
    public async Task Transfer_moves_stock_atomically_and_links_both_ledger_entries()
    {
        var product = await CreateProductAsync();
        var sourceWarehouse = await CreateWarehouseAsync();
        var destinationWarehouse = await CreateWarehouseAsync();
        await IncreaseAsync(product.Id, sourceWarehouse.Id, 10m);

        var response = await fixture.Client.PostAsJsonAsync("/api/inventory/transfers", new InventoryTransferInput(
            sourceWarehouse.Id,
            destinationWarehouse.Id,
            "TR-2026-001",
            "Replenish the destination warehouse",
            [new InventoryTransferLineInput(product.Id, 3m, "EA")]));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var transfer = await response.Content.ReadFromJsonAsync<InventoryTransferDetailResponse>();
        Assert.NotNull(transfer);
        var line = Assert.Single(transfer.Lines);
        Assert.Equal(3m, line.QuantityInBaseUnit);
        Assert.Equal(7m, line.SourceBalanceAfter);
        Assert.Equal(3m, line.DestinationBalanceAfter);

        using var scope = fixture.Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        var balances = await dbContext.InventoryBalances
            .Where(balance => balance.ProductId == product.Id)
            .OrderBy(balance => balance.WarehouseId)
            .ToListAsync();
        var movements = await dbContext.InventoryMovements
            .Where(movement => movement.InventoryTransferId == transfer.Id)
            .OrderBy(movement => movement.Type)
            .ToListAsync();

        Assert.Equal(2, balances.Count);
        Assert.Equal(7m, balances.Single(balance => balance.WarehouseId == sourceWarehouse.Id).Quantity);
        Assert.Equal(3m, balances.Single(balance => balance.WarehouseId == destinationWarehouse.Id).Quantity);
        Assert.Equal(2, movements.Count);
        Assert.Contains(movements, movement => movement.Type == InventoryMovementType.TransferOut && movement.QuantityDelta == -3m);
        Assert.Contains(movements, movement => movement.Type == InventoryMovementType.TransferIn && movement.QuantityDelta == 3m);

        var history = await fixture.Client.GetFromJsonAsync<PagedResult<InventoryMovementResponse>>(
            "/api/inventory/movements?type=TransferOut&reference=TR-2026-001&page=1&pageSize=20");
        Assert.NotNull(history);
        var historyItem = Assert.Single(history.Items);
        Assert.Equal(transfer.Id, historyItem.InventoryTransferId);
        Assert.Equal("TR-2026-001", historyItem.TransferReference);

        var list = await fixture.Client.GetFromJsonAsync<PagedResult<InventoryTransferListItemResponse>>(
            $"/api/inventory/transfers?sourceWarehouseId={sourceWarehouse.Id}&page=1&pageSize=20");
        Assert.NotNull(list);
        Assert.Equal(transfer.Id, Assert.Single(list.Items).Id);
    }

    [Fact]
    public async Task Transfer_rejects_insufficient_stock_without_partial_changes()
    {
        var stockedProduct = await CreateProductAsync();
        var unavailableProduct = await CreateProductAsync();
        var sourceWarehouse = await CreateWarehouseAsync();
        var destinationWarehouse = await CreateWarehouseAsync();
        await IncreaseAsync(stockedProduct.Id, sourceWarehouse.Id, 2m);

        var response = await fixture.Client.PostAsJsonAsync("/api/inventory/transfers", new InventoryTransferInput(
            sourceWarehouse.Id,
            destinationWarehouse.Id,
            null,
            null,
            [
                new InventoryTransferLineInput(stockedProduct.Id, 1m, "EA"),
                new InventoryTransferLineInput(unavailableProduct.Id, 1m, "EA")
            ]));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemResponse>();
        Assert.NotNull(problem);
        Assert.Equal(ApiErrorCodes.InventoryInsufficientStock, problem.Code);
        Assert.Equal(
            ApiErrorCodes.InventoryInsufficientStock,
            Assert.Single(problem.ErrorCodes!["Lines[1].Quantity"]));

        using var scope = fixture.Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        var sourceBalance = await dbContext.InventoryBalances.SingleAsync(balance =>
            balance.ProductId == stockedProduct.Id && balance.WarehouseId == sourceWarehouse.Id);
        Assert.Equal(2m, sourceBalance.Quantity);
        Assert.False(await dbContext.InventoryBalances.AnyAsync(balance => balance.WarehouseId == destinationWarehouse.Id));
        Assert.False(await dbContext.InventoryTransfers.AnyAsync());
        Assert.False(await dbContext.InventoryMovements.AnyAsync(movement =>
            movement.InventoryTransferId != null));
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

    private async Task IncreaseAsync(Guid productId, Guid warehouseId, decimal quantity)
    {
        var response = await fixture.Client.PostAsJsonAsync("/api/inventory/adjustments", new
        {
            reason = InventoryAdjustmentReason.StockCorrection,
            lines = new[] { new { productId, warehouseId, quantity, direction = InventoryAdjustmentDirection.Increase, unitOfMeasure = "EA" } }
        });
        response.EnsureSuccessStatusCode();
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

    private sealed record ProblemResponse(
        string? Code,
        Dictionary<string, string[]>? Errors = null,
        Dictionary<string, string[]>? ErrorCodes = null);
}
