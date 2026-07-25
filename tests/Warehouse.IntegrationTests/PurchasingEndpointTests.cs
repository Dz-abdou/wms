using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.Application.Common.Errors;
using Warehouse.Application.Products;
using Warehouse.Application.Purchasing;
using Warehouse.Application.Suppliers;
using Warehouse.Domain.Purchasing;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.IntegrationTests;

[Collection(ProductApiCollection.Name)]
public sealed class PurchasingEndpointTests(ProductApiFixture fixture)
{
    [Fact]
    public async Task Supplier_catalogue_enforces_unique_supplier_product_unit()
    {
        var supplier = await CreateSupplierAsync();
        var product = await CreateProductAsync();
        var catalogueItem = await CreateCatalogueItemAsync(supplier.Id, product.Id, "EA", 1m);

        Assert.Equal("DZD", catalogueItem.CurrencyCode);
        var list = await fixture.Client.GetFromJsonAsync<SupplierProductListResult>($"/api/supplier-products?supplierId={supplier.Id}&page=1&pageSize=20");
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
        var catalogueItem = await CreateCatalogueItemAsync(supplier.Id, product.Id, "EA", 2m);

        var create = await fixture.Client.PostAsJsonAsync("/api/purchase-orders", new
        {
            supplierId = supplier.Id,
            lines = new[] { new { supplierProductId = catalogueItem.Id, quantity = 3m } }
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var draft = await create.Content.ReadFromJsonAsync<PurchaseOrderResponse>();
        Assert.NotNull(draft);
        Assert.Equal(PurchaseOrderStatus.Draft, draft.Status);
        var line = Assert.Single(draft.Lines);
        Assert.Equal(product.Sku, line.ProductSku);
        Assert.Equal(catalogueItem.UnitPrice, line.UnitPrice);

        var submitted = await fixture.Client.PatchAsync($"/api/purchase-orders/{draft.Id}/submit", null);
        submitted.EnsureSuccessStatusCode();
        var submittedOrder = await submitted.Content.ReadFromJsonAsync<PurchaseOrderResponse>();
        Assert.NotNull(submittedOrder);
        Assert.Equal(PurchaseOrderStatus.Submitted, submittedOrder.Status);

        var update = await fixture.Client.PutAsJsonAsync($"/api/purchase-orders/{draft.Id}", new
        {
            supplierId = supplier.Id,
            lines = new[] { new { supplierProductId = catalogueItem.Id, quantity = 4m } }
        });
        Assert.Equal(HttpStatusCode.Conflict, update.StatusCode);
        await AssertCodeAsync(update, ApiErrorCodes.PurchaseOrderImmutable);
    }

    [Fact]
    public async Task Purchase_order_rejects_quantities_below_the_catalogue_minimum()
    {
        var supplier = await CreateSupplierAsync();
        var product = await CreateProductAsync();
        var catalogueItem = await CreateCatalogueItemAsync(supplier.Id, product.Id, "EA", 5m);

        var response = await fixture.Client.PostAsJsonAsync("/api/purchase-orders", new
        {
            supplierId = supplier.Id,
            lines = new[] { new { supplierProductId = catalogueItem.Id, quantity = 4m } }
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        await AssertCodeAsync(response, ApiErrorCodes.PurchaseOrderCatalogueInvalid);
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

    private static async Task AssertCodeAsync(HttpResponseMessage response, string expectedCode)
    {
        var problem = await response.Content.ReadFromJsonAsync<Problem>();
        Assert.NotNull(problem);
        Assert.Equal(expectedCode, problem.Code);
    }

    private sealed record Problem(string? Code);
}
