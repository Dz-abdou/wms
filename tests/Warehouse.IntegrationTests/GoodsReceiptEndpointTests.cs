using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.Application.Common.Errors;
using Warehouse.Application.Common.Models;
using Warehouse.Application.Products;
using Warehouse.Application.Purchasing;
using Warehouse.Application.Receiving;
using Warehouse.Application.Suppliers;
using Warehouse.Application.Warehouses;
using Warehouse.Domain.Inventory;
using Warehouse.Domain.Purchasing;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.IntegrationTests;

[Collection(ProductApiCollection.Name)]
public sealed class GoodsReceiptEndpointTests(ProductApiFixture fixture)
{
    [Fact]
    public async Task Posting_partial_and_final_receipts_updates_stock_and_purchase_order()
    {
        var order = await CreateSubmittedOrderAsync(10m);
        var candidate = await fixture.Client.GetFromJsonAsync<GoodsReceiptCandidateResponse>(
            $"/api/purchase-orders/{order.Id}/receipt-candidate");
        Assert.NotNull(candidate);
        Assert.Equal(10m, Assert.Single(candidate.Lines).OutstandingQuantity);

        var firstReceipt = await PostReceiptAsync(candidate, 4m);
        var afterPartial = await fixture.Client.GetFromJsonAsync<PurchaseOrderResponse>(
            $"/api/purchase-orders/{order.Id}");
        Assert.NotNull(afterPartial);
        Assert.Equal(PurchaseOrderStatus.PartiallyReceived, afterPartial.Status);

        var finalCandidate = await fixture.Client.GetFromJsonAsync<GoodsReceiptCandidateResponse>(
            $"/api/purchase-orders/{order.Id}/receipt-candidate");
        Assert.NotNull(finalCandidate);
        Assert.Equal(6m, Assert.Single(finalCandidate.Lines).OutstandingQuantity);
        var finalReceipt = await PostReceiptAsync(finalCandidate, 6m);

        var afterFinal = await fixture.Client.GetFromJsonAsync<PurchaseOrderResponse>(
            $"/api/purchase-orders/{order.Id}");
        Assert.NotNull(afterFinal);
        Assert.Equal(PurchaseOrderStatus.Received, afterFinal.Status);

        var list = await fixture.Client.GetFromJsonAsync<PagedResult<GoodsReceiptListItemResponse>>(
            $"/api/goods-receipts?purchaseOrderNumber={order.Number}&page=1&pageSize=20");
        Assert.NotNull(list);
        Assert.Equal(2, list.TotalCount);
        var detail = await fixture.Client.GetFromJsonAsync<GoodsReceiptDetailResponse>(
            $"/api/goods-receipts/{finalReceipt.Id}");
        Assert.NotNull(detail);
        Assert.Equal(order.Number, detail.PurchaseOrderNumber);
        Assert.Equal(6m, Assert.Single(detail.Lines).AcceptedQuantity);

        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        var balance = await db.InventoryBalances.SingleAsync(candidate => candidate.ProductId == order.Line.ProductId && candidate.WarehouseId == order.DestinationWarehouseId);
        var movements = await db.InventoryMovements.Where(candidate => candidate.GoodsReceiptId == firstReceipt.Id || candidate.GoodsReceiptId == finalReceipt.Id).ToListAsync();
        Assert.Equal(10m, balance.Quantity);
        Assert.Equal(2, movements.Count);
        Assert.All(movements, movement => Assert.Equal(InventoryMovementType.GoodsReceipt, movement.Type));
    }

    [Fact]
    public async Task Over_receipt_is_a_field_error_and_writes_nothing()
    {
        var order = await CreateSubmittedOrderAsync(2m);
        var candidate = await GetCandidateAsync(order.Id);
        var response = await fixture.Client.PostAsJsonAsync("/api/goods-receipts", new
        {
            purchaseOrderId = candidate.PurchaseOrderId,
            purchaseOrderVersion = candidate.Version,
            receivedAtUtc = DateTime.UtcNow,
            lines = new[] { new { purchaseOrderLineId = candidate.Lines.Single().PurchaseOrderLineId, acceptedQuantity = 3m } }
        });

        await AssertFieldErrorAsync(response, "Lines[0].AcceptedQuantity", ApiErrorCodes.GoodsReceiptOverReceipt);
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        Assert.False(await db.GoodsReceipts.AnyAsync(receipt => receipt.PurchaseOrderId == order.Id));
        Assert.False(await db.InventoryBalances.AnyAsync(balance => balance.ProductId == order.Line.ProductId));
        Assert.False(await db.InventoryMovements.AnyAsync(movement => movement.ProductId == order.Line.ProductId));
    }

    [Fact]
    public async Task Stale_receipt_version_is_rejected_without_partial_persistence()
    {
        var order = await CreateSubmittedOrderAsync(5m);
        var firstCandidate = await GetCandidateAsync(order.Id);
        var staleCandidate = await GetCandidateAsync(order.Id);
        await PostReceiptAsync(firstCandidate, 2m);

        var stale = await fixture.Client.PostAsJsonAsync("/api/goods-receipts", new
        {
            purchaseOrderId = staleCandidate.PurchaseOrderId,
            purchaseOrderVersion = staleCandidate.Version,
            receivedAtUtc = DateTime.UtcNow,
            lines = new[] { new { purchaseOrderLineId = staleCandidate.Lines.Single().PurchaseOrderLineId, acceptedQuantity = 2m } }
        });

        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        var problem = await stale.Content.ReadFromJsonAsync<ProblemResponse>();
        Assert.NotNull(problem);
        Assert.Equal(ApiErrorCodes.GoodsReceiptPurchaseOrderConcurrencyConflict, problem.Code);
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        Assert.Equal(1, await db.GoodsReceipts.CountAsync(receipt => receipt.PurchaseOrderId == order.Id));
        Assert.Equal(1, await db.InventoryMovements.CountAsync(movement => movement.ProductId == order.Line.ProductId));
    }

    [Fact]
    public async Task Duplicate_receipt_lines_are_rejected_as_a_specific_field_error()
    {
        var order = await CreateSubmittedOrderAsync(2m);
        var candidate = await GetCandidateAsync(order.Id);
        var lineId = candidate.Lines.Single().PurchaseOrderLineId;
        var response = await fixture.Client.PostAsJsonAsync("/api/goods-receipts", new
        {
            purchaseOrderId = candidate.PurchaseOrderId,
            purchaseOrderVersion = candidate.Version,
            receivedAtUtc = DateTime.UtcNow,
            lines = new[]
            {
                new { purchaseOrderLineId = lineId, acceptedQuantity = 1m },
                new { purchaseOrderLineId = lineId, acceptedQuantity = 1m }
            }
        });

        await AssertFieldErrorAsync(response, "Lines[1].PurchaseOrderLineId", ApiErrorCodes.GoodsReceiptDuplicatePurchaseOrderLine, HttpStatusCode.BadRequest);
    }

    private async Task<GoodsReceiptResponse> PostReceiptAsync(GoodsReceiptCandidateResponse candidate, decimal quantity)
    {
        var response = await fixture.Client.PostAsJsonAsync("/api/goods-receipts", new
        {
            purchaseOrderId = candidate.PurchaseOrderId,
            purchaseOrderVersion = candidate.Version,
            receivedAtUtc = DateTime.UtcNow,
            lines = new[] { new { purchaseOrderLineId = candidate.Lines.Single().PurchaseOrderLineId, acceptedQuantity = quantity } }
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<GoodsReceiptResponse>())!;
    }

    private async Task<GoodsReceiptCandidateResponse> GetCandidateAsync(Guid purchaseOrderId)
    {
        var candidate = await fixture.Client.GetFromJsonAsync<GoodsReceiptCandidateResponse>(
            $"/api/purchase-orders/{purchaseOrderId}/receipt-candidate");
        return Assert.IsType<GoodsReceiptCandidateResponse>(candidate);
    }

    private async Task<SubmittedOrder> CreateSubmittedOrderAsync(decimal quantity)
    {
        var supplier = await CreateSupplierAsync();
        var product = await CreateProductAsync();
        var warehouse = await CreateWarehouseAsync();
        var supplierProduct = await CreateSupplierProductAsync(supplier.Id, product.Id);
        var create = await fixture.Client.PostAsJsonAsync("/api/purchase-orders", new
        {
            supplierId = supplier.Id,
            destinationWarehouseId = warehouse.Id,
            currencyCode = "DZD",
            orderDate = "2026-07-27",
            lines = new[] { new { supplierProductId = supplierProduct.Id, quantity } }
        });
        create.EnsureSuccessStatusCode();
        var draft = (await create.Content.ReadFromJsonAsync<PurchaseOrderResponse>())!;
        var submit = await fixture.Client.PatchAsJsonAsync($"/api/purchase-orders/{draft.Id}/submit", new { version = draft.Version });
        submit.EnsureSuccessStatusCode();
        var submitted = (await submit.Content.ReadFromJsonAsync<PurchaseOrderResponse>())!;
        return new SubmittedOrder(submitted.Id, submitted.Number!, warehouse.Id, submitted.Lines.Single());
    }

    private async Task<SupplierResponse> CreateSupplierAsync()
    {
        var response = await fixture.Client.PostAsJsonAsync("/api/suppliers", new { code = $"REC-{Guid.NewGuid():N}"[..16], name = "Receipt supplier", defaultCurrencyCode = "DZD" });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<SupplierResponse>())!;
    }

    private async Task<ProductResponse> CreateProductAsync()
    {
        var response = await fixture.Client.PostAsJsonAsync("/api/products", new { sku = $"REC-{Guid.NewGuid():N}"[..16], name = "Receipt product" });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ProductResponse>())!;
    }

    private async Task<WarehouseResponse> CreateWarehouseAsync()
    {
        var response = await fixture.Client.PostAsJsonAsync("/api/warehouses", new { code = $"REC-{Guid.NewGuid():N}"[..16], name = "Receipt warehouse" });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<WarehouseResponse>())!;
    }

    private async Task<SupplierProductResponse> CreateSupplierProductAsync(Guid supplierId, Guid productId)
    {
        var response = await fixture.Client.PostAsJsonAsync("/api/supplier-products", new { supplierId, productId, purchaseUnitOfMeasure = "EA", minimumOrderQuantity = 1m, unitPrice = 2m, currencyCode = "DZD" });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<SupplierProductResponse>())!;
    }

    private static async Task AssertFieldErrorAsync(HttpResponseMessage response, string propertyName, string expectedCode, HttpStatusCode expectedStatus = HttpStatusCode.UnprocessableEntity)
    {
        Assert.Equal(expectedStatus, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemResponse>();
        Assert.NotNull(problem);
        Assert.Equal(expectedCode, Assert.Single(problem.ErrorCodes[propertyName]));
    }

    private sealed record SubmittedOrder(Guid Id, string Number, Guid DestinationWarehouseId, PurchaseOrderLineResponse Line);
    private sealed record ProblemResponse(string? Code);
    private sealed record ValidationProblemResponse(Dictionary<string, string[]> ErrorCodes);
}
