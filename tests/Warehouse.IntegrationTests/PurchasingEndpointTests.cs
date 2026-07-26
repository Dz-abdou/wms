using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.Application.Common.Errors;
using Warehouse.Application.Common.Models;
using Warehouse.Application.Currencies;
using Warehouse.Application.Products;
using Warehouse.Application.Purchasing;
using Warehouse.Application.Suppliers;
using Warehouse.Application.Warehouses;
using Warehouse.Domain.Purchasing;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.IntegrationTests;

[Collection(ProductApiCollection.Name)]
public sealed class PurchasingEndpointTests(ProductApiFixture fixture)
{
    [Fact]
    public async Task Supplier_catalogue_uses_the_centralized_currency_catalogue()
    {
        var currencies = await fixture.Client.GetFromJsonAsync<PagedResult<CurrencyResponse>>("/api/currencies?activeOnly=true&page=1&pageSize=20");

        Assert.NotNull(currencies);
        Assert.Contains(currencies.Items, currency => currency.Code == "DZD" && currency.IsDefault);

        var supplier = await CreateSupplierAsync();
        var product = await CreateProductAsync();
        var response = await fixture.Client.PostAsJsonAsync("/api/supplier-products", new
        {
            supplierId = supplier.Id,
            productId = product.Id,
            purchaseUnitOfMeasure = "EA",
            minimumOrderQuantity = 1m,
            unitPrice = 20m,
            currencyCode = "ZZZ"
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        await AssertCodeAsync(response, ApiErrorCodes.SupplierProductCurrencyNotSupported);
    }

    [Fact]
    public async Task Supplier_catalogue_enforces_unique_supplier_product_unit()
    {
        var supplier = await CreateSupplierAsync();
        var product = await CreateProductAsync();
        var catalogueItem = await CreateCatalogueItemAsync(supplier.Id, product.Id, "EA", 1m);

        Assert.Equal("DZD", catalogueItem.CurrencyCode);
        var list = await fixture.Client.GetFromJsonAsync<PagedResult<SupplierProductResponse>>($"/api/supplier-products?supplierId={supplier.Id}&page=1&pageSize=20");
        Assert.NotNull(list);
        Assert.Contains(list.Items, item => item.Id == catalogueItem.Id);
        var duplicate = await fixture.Client.PostAsJsonAsync("/api/supplier-products", new
        {
            supplierId = supplier.Id,
            productId = product.Id,
            purchaseUnitOfMeasure = "EA",
            minimumOrderQuantity = 1m,
            unitPrice = 20m,
            currencyCode = "DZD"
        });

        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        await AssertCodeAsync(duplicate, ApiErrorCodes.SupplierProductConflict);
    }

    [Fact]
    public async Task Draft_purchase_order_snapshots_catalogue_and_becomes_immutable_when_submitted()
    {
        var supplier = await CreateSupplierAsync();
        var product = await CreateProductAsync();
        var warehouse = await CreateWarehouseAsync();
        var catalogueItem = await CreateCatalogueItemAsync(supplier.Id, product.Id, "EA", 2m);

        var create = await fixture.Client.PostAsJsonAsync("/api/purchase-orders", new
        {
            supplierId = supplier.Id,
            destinationWarehouseId = warehouse.Id,
            currencyCode = "DZD",
            orderDate = "2026-07-26",
            lines = new[] { new { supplierProductId = catalogueItem.Id, quantity = 3m } }
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var draft = await create.Content.ReadFromJsonAsync<PurchaseOrderResponse>();
        Assert.NotNull(draft);
        Assert.Equal(PurchaseOrderStatus.Draft, draft.Status);
        var line = Assert.Single(draft.Lines);
        Assert.Equal(product.Sku, line.ProductSku);
        Assert.Equal(1, line.LineNumber);
        Assert.Equal(3m, line.QuantityInBaseUnit);
        Assert.Equal(1m, line.ConversionFactorToBaseUnit);
        Assert.Equal(catalogueItem.UnitPrice, line.UnitPrice);
        Assert.Equal(37.5m, line.LineAmount);

        var submitted = await fixture.Client.PatchAsJsonAsync($"/api/purchase-orders/{draft.Id}/submit", new { version = draft.Version });
        submitted.EnsureSuccessStatusCode();
        var submittedOrder = await submitted.Content.ReadFromJsonAsync<PurchaseOrderResponse>();
        Assert.NotNull(submittedOrder);
        Assert.Equal(PurchaseOrderStatus.Submitted, submittedOrder.Status);
        Assert.Equal(draft.Version + 1, submittedOrder.Version);

        var catalogueUpdate = await fixture.Client.PutAsJsonAsync($"/api/supplier-products/{catalogueItem.Id}", new
        {
            supplierSku = "SUP-ITEM-001",
            purchaseUnitOfMeasure = "EA",
            minimumOrderQuantity = 2m,
            unitPrice = 20m,
            currencyCode = "DZD"
        });
        catalogueUpdate.EnsureSuccessStatusCode();
        var reloaded = await fixture.Client.GetFromJsonAsync<PurchaseOrderResponse>($"/api/purchase-orders/{draft.Id}");
        Assert.NotNull(reloaded);
        Assert.Equal(12.5m, Assert.Single(reloaded.Lines).UnitPrice);

        var update = await fixture.Client.PutAsJsonAsync($"/api/purchase-orders/{draft.Id}", new
        {
            supplierId = supplier.Id,
            destinationWarehouseId = warehouse.Id,
            currencyCode = "DZD",
            orderDate = "2026-07-26",
            version = submittedOrder.Version,
            lines = new[] { new { supplierProductId = catalogueItem.Id, quantity = 4m } }
        });
        Assert.Equal(HttpStatusCode.Conflict, update.StatusCode);
        await AssertCodeAsync(update, ApiErrorCodes.PurchaseOrderImmutable);

        var cancelled = await fixture.Client.PatchAsJsonAsync($"/api/purchase-orders/{draft.Id}/cancel", new { version = submittedOrder.Version, reason = "Supplier unavailable" });
        cancelled.EnsureSuccessStatusCode();
        var cancelledOrder = await cancelled.Content.ReadFromJsonAsync<PurchaseOrderResponse>();
        Assert.NotNull(cancelledOrder);
        Assert.Equal(PurchaseOrderStatus.Cancelled, cancelledOrder.Status);
        Assert.Equal(submittedOrder.Version + 1, cancelledOrder.Version);
        Assert.Equal(3, cancelledOrder.StatusHistory.Count);

        var invalidCancel = await fixture.Client.PatchAsJsonAsync($"/api/purchase-orders/{draft.Id}/cancel", new { version = cancelledOrder.Version });
        Assert.Equal(HttpStatusCode.Conflict, invalidCancel.StatusCode);
        await AssertCodeAsync(invalidCancel, ApiErrorCodes.PurchaseOrderInvalidTransition);
    }

    [Fact]
    public async Task Purchase_order_update_rejects_a_stale_version_without_overwriting_the_draft()
    {
        var supplier = await CreateSupplierAsync();
        var product = await CreateProductAsync();
        var warehouse = await CreateWarehouseAsync();
        var catalogueItem = await CreateCatalogueItemAsync(supplier.Id, product.Id, "EA", 1m);
        var create = await fixture.Client.PostAsJsonAsync("/api/purchase-orders", new
        {
            supplierId = supplier.Id,
            destinationWarehouseId = warehouse.Id,
            currencyCode = "DZD",
            orderDate = "2026-07-26",
            notes = "Original",
            lines = new[] { new { supplierProductId = catalogueItem.Id, quantity = 1m } }
        });
        create.EnsureSuccessStatusCode();
        var draft = await create.Content.ReadFromJsonAsync<PurchaseOrderResponse>();
        Assert.NotNull(draft);

        var current = await fixture.Client.PutAsJsonAsync($"/api/purchase-orders/{draft.Id}", new
        {
            supplierId = supplier.Id,
            destinationWarehouseId = warehouse.Id,
            currencyCode = "DZD",
            orderDate = "2026-07-26",
            notes = "Current update",
            version = draft.Version,
            lines = new[] { new { supplierProductId = catalogueItem.Id, quantity = 1m } }
        });
        current.EnsureSuccessStatusCode();

        var stale = await fixture.Client.PutAsJsonAsync($"/api/purchase-orders/{draft.Id}", new
        {
            supplierId = supplier.Id,
            destinationWarehouseId = warehouse.Id,
            currencyCode = "DZD",
            orderDate = "2026-07-26",
            notes = "Stale update",
            version = draft.Version,
            lines = new[] { new { supplierProductId = catalogueItem.Id, quantity = 1m } }
        });
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        await AssertCodeAsync(stale, ApiErrorCodes.PurchaseOrderConcurrencyConflict);

        var persisted = await fixture.Client.GetFromJsonAsync<PurchaseOrderResponse>($"/api/purchase-orders/{draft.Id}");
        Assert.NotNull(persisted);
        Assert.Equal("Current update", persisted.Notes);
        Assert.Equal(draft.Version + 1, persisted.Version);
    }

    [Fact]
    public async Task Concurrent_draft_creation_allocates_unique_purchase_order_numbers()
    {
        var supplier = await CreateSupplierAsync();
        var product = await CreateProductAsync();
        var warehouse = await CreateWarehouseAsync();
        var catalogueItem = await CreateCatalogueItemAsync(supplier.Id, product.Id, "EA", 1m);

        var drafts = await Task.WhenAll(Enumerable.Range(0, 4).Select(async _ =>
        {
            var response = await fixture.Client.PostAsJsonAsync("/api/purchase-orders", new
            {
                supplierId = supplier.Id,
                destinationWarehouseId = warehouse.Id,
                currencyCode = "DZD",
                orderDate = "2026-07-26",
                lines = new[] { new { supplierProductId = catalogueItem.Id, quantity = 1m } }
            });
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<PurchaseOrderResponse>())!;
        }));

        Assert.All(drafts, draft => Assert.Matches("^PO-2026-\\d{6}$", draft.Number));
        Assert.Equal(drafts.Length, drafts.Select(draft => draft.Number).Distinct().Count());
    }

    [Fact]
    public async Task Purchase_order_list_filters_before_pagination()
    {
        var supplier = await CreateSupplierAsync();
        var product = await CreateProductAsync();
        var firstWarehouse = await CreateWarehouseAsync();
        var secondWarehouse = await CreateWarehouseAsync();
        var catalogueItem = await CreateCatalogueItemAsync(supplier.Id, product.Id, "EA", 1m);

        await CreatePurchaseOrderAsync(supplier.Id, firstWarehouse.Id, catalogueItem.Id, "2026-07-11");
        await CreatePurchaseOrderAsync(supplier.Id, firstWarehouse.Id, catalogueItem.Id, "2026-07-11");
        await CreatePurchaseOrderAsync(supplier.Id, secondWarehouse.Id, catalogueItem.Id, "2026-07-12");

        var list = await fixture.Client.GetFromJsonAsync<PagedResult<PurchaseOrderResponse>>(
            $"/api/purchase-orders?supplierId={supplier.Id}&warehouseId={firstWarehouse.Id}&fromOrderDate=2026-07-11&toOrderDate=2026-07-11&page=1&pageSize=1");

        Assert.NotNull(list);
        Assert.Equal(2, list.TotalCount);
        var item = Assert.Single(list.Items);
        Assert.Equal(firstWarehouse.Id, item.DestinationWarehouseId);
        Assert.Equal(new DateOnly(2026, 7, 11), item.OrderDate);
    }

    [Fact]
    public async Task Purchase_order_rejects_quantities_below_the_catalogue_minimum()
    {
        var supplier = await CreateSupplierAsync();
        var product = await CreateProductAsync();
        var warehouse = await CreateWarehouseAsync();
        var catalogueItem = await CreateCatalogueItemAsync(supplier.Id, product.Id, "EA", 5m);

        var response = await fixture.Client.PostAsJsonAsync("/api/purchase-orders", new
        {
            supplierId = supplier.Id,
            destinationWarehouseId = warehouse.Id,
            currencyCode = "DZD",
            orderDate = "2026-07-26",
            lines = new[] { new { supplierProductId = catalogueItem.Id, quantity = 4m } }
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblem>();
        Assert.NotNull(problem);
        Assert.Equal(ApiErrorCodes.PurchaseOrderMinimumOrderQuantity, problem.Code);
        Assert.Equal(
            ApiErrorCodes.PurchaseOrderMinimumOrderQuantity,
            Assert.Single(problem.ErrorCodes["Lines[0].Quantity"]));
        Assert.NotEmpty(problem.Errors["Lines[0].Quantity"]);
    }

    [Fact]
    public async Task PostgreSql_rejects_a_duplicate_supplier_product_unit()
    {
        var supplier = await CreateSupplierAsync();
        var product = await CreateProductAsync();
        await CreateCatalogueItemAsync(supplier.Id, product.Id, "EA", 1m);

        using var scope = fixture.Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        dbContext.SupplierProducts.Add(SupplierProduct.Create(supplier.Id, product.Id, null, "EA", 1m, 1m, "DZD", DateTime.UtcNow));
        await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
    }

    private async Task<SupplierResponse> CreateSupplierAsync()
    {
        var response = await fixture.Client.PostAsJsonAsync("/api/suppliers", new { code = $"SUP-{Guid.NewGuid():N}"[..16], name = "Purchase supplier" });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<SupplierResponse>())!;
    }

    private async Task<ProductResponse> CreateProductAsync()
    {
        var response = await fixture.Client.PostAsJsonAsync("/api/products", new { sku = $"PUR-{Guid.NewGuid():N}"[..16], name = "Purchase product" });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ProductResponse>())!;
    }

    private async Task<WarehouseResponse> CreateWarehouseAsync()
    {
        var response = await fixture.Client.PostAsJsonAsync("/api/warehouses", new { code = $"PO-{Guid.NewGuid():N}"[..16], name = "Purchase warehouse" });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<WarehouseResponse>())!;
    }

    private async Task<SupplierProductResponse> CreateCatalogueItemAsync(Guid supplierId, Guid productId, string unitOfMeasure, decimal minimumOrderQuantity)
    {
        var response = await fixture.Client.PostAsJsonAsync("/api/supplier-products", new
        {
            supplierId,
            productId,
            supplierSku = "SUP-ITEM-001",
            purchaseUnitOfMeasure = unitOfMeasure,
            minimumOrderQuantity,
            unitPrice = 12.5m,
            currencyCode = "DZD"
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<SupplierProductResponse>())!;
    }

    private async Task<PurchaseOrderResponse> CreatePurchaseOrderAsync(Guid supplierId, Guid warehouseId, Guid supplierProductId, string orderDate)
    {
        var response = await fixture.Client.PostAsJsonAsync("/api/purchase-orders", new
        {
            supplierId,
            destinationWarehouseId = warehouseId,
            currencyCode = "DZD",
            orderDate,
            lines = new[] { new { supplierProductId, quantity = 1m } }
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PurchaseOrderResponse>())!;
    }

    private static async Task AssertCodeAsync(HttpResponseMessage response, string expectedCode)
    {
        var problem = await response.Content.ReadFromJsonAsync<Problem>();
        Assert.NotNull(problem);
        Assert.Equal(expectedCode, problem.Code);
    }

    private sealed record Problem(string? Code);
    private sealed record ValidationProblem(
        string? Code,
        Dictionary<string, string[]> Errors,
        Dictionary<string, string[]> ErrorCodes);
}
