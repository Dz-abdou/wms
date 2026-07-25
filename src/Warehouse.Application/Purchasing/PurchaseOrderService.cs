using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Common.Identity;
using Warehouse.Application.Common.Models;
using Warehouse.Application.Common.Pagination;
using Warehouse.Application.Common.Persistence;
using Warehouse.Application.Products;
using Warehouse.Application.Suppliers;
using Warehouse.Domain.Products;
using Warehouse.Domain.Purchasing;

namespace Warehouse.Application.Purchasing;

public sealed class PurchaseOrderService(
    IWarehouseDbContext dbContext,
    TimeProvider timeProvider,
    ICurrentUser currentUser)
{
    public async Task<PagedResult<PurchaseOrderResponse>> GetListAsync(PurchaseOrderListQuery query, CancellationToken cancellationToken)
    {
        var orders = from purchaseOrder in dbContext.PurchaseOrders.AsNoTracking()
                     join supplier in dbContext.Suppliers.AsNoTracking() on purchaseOrder.SupplierId equals supplier.Id
                     select new { purchaseOrder, supplier };
        if (query.Status is { } status)
        {
            orders = orders.Where(item => item.purchaseOrder.Status == status);
        }

        var totalCount = await orders.CountAsync(cancellationToken);
        var page = await orders
            .OrderByDescending(item => item.purchaseOrder.CreatedAtUtc)
            .Skip((query.Page - PaginationConstants.DefaultPage) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);
        var items = page.Select(item => new PurchaseOrderResponse(
            item.purchaseOrder.Id,
            item.purchaseOrder.SupplierId,
            item.supplier.Code,
            item.supplier.Name,
            item.purchaseOrder.Status,
            Array.Empty<PurchaseOrderLineResponse>(),
            item.purchaseOrder.CreatedAtUtc,
            item.purchaseOrder.UpdatedAtUtc)).ToList();
        return new PagedResult<PurchaseOrderResponse>(items, query.Page, query.PageSize, totalCount);
    }

    public async Task<PurchaseOrderResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var purchaseOrder = await dbContext.PurchaseOrders.AsNoTracking().Include(order => order.Lines)
            .SingleOrDefaultAsync(order => order.Id == id, cancellationToken)
            ?? throw new PurchaseOrderNotFoundException(id);
        var supplier = await dbContext.Suppliers.AsNoTracking().SingleAsync(candidate => candidate.Id == purchaseOrder.SupplierId, cancellationToken);
        return ToResponse(purchaseOrder, supplier.Code, supplier.Name);
    }

    public async Task<PurchaseOrderResponse> CreateAsync(PurchaseOrderInput input, CancellationToken cancellationToken)
    {
        await EnsureSupplierIsActiveAsync(input.SupplierId, cancellationToken);
        var lines = await ResolveLinesAsync(input.SupplierId, input.Lines ?? [], cancellationToken);
        var purchaseOrder = PurchaseOrder.Create(input.SupplierId, UtcNow(), currentUser.UserId);
        purchaseOrder.ReplaceLines(lines, UtcNow(), currentUser.UserId);
        dbContext.PurchaseOrders.Add(purchaseOrder);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(purchaseOrder.Id, cancellationToken);
    }

    public async Task<PurchaseOrderResponse> UpdateAsync(Guid id, PurchaseOrderInput input, CancellationToken cancellationToken)
    {
        var purchaseOrder = await FindTrackedAsync(id, cancellationToken);
        EnsureDraft(purchaseOrder);
        await EnsureSupplierIsActiveAsync(input.SupplierId, cancellationToken);
        var lines = await ResolveLinesAsync(input.SupplierId, input.Lines ?? [], cancellationToken);
        var updatedAtUtc = UtcNow();
        purchaseOrder.UpdateSupplier(input.SupplierId, updatedAtUtc, currentUser.UserId);
        purchaseOrder.ReplaceLines(lines, updatedAtUtc, currentUser.UserId);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<PurchaseOrderResponse> SubmitAsync(Guid id, CancellationToken cancellationToken)
    {
        var purchaseOrder = await FindTrackedAsync(id, cancellationToken);
        EnsureDraft(purchaseOrder);
        if (purchaseOrder.Lines.Count == 0)
        {
            throw new PurchaseOrderSubmissionInvalidException("A purchase order requires at least one line before submission.");
        }

        await EnsureSupplierIsActiveAsync(purchaseOrder.SupplierId, cancellationToken);
        await ValidateDraftLinesAsync(purchaseOrder.SupplierId, purchaseOrder.Lines, cancellationToken);
        purchaseOrder.Submit(UtcNow(), currentUser.UserId);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    private async Task<PurchaseOrder> FindTrackedAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.PurchaseOrders.Include(order => order.Lines).SingleOrDefaultAsync(order => order.Id == id, cancellationToken)
        ?? throw new PurchaseOrderNotFoundException(id);

    private async Task EnsureSupplierIsActiveAsync(Guid supplierId, CancellationToken cancellationToken)
    {
        var supplier = await dbContext.Suppliers.SingleOrDefaultAsync(candidate => candidate.Id == supplierId, cancellationToken)
            ?? throw new SupplierNotFoundException(supplierId);
        if (!supplier.IsActive)
        {
            throw new PurchaseOrderCatalogueInvalidException("An inactive supplier cannot be used for purchasing.");
        }
    }

    private async Task<IReadOnlyCollection<PurchaseOrderLine>> ResolveLinesAsync(Guid supplierId, IReadOnlyCollection<PurchaseOrderLineInput> inputs, CancellationToken cancellationToken)
    {
        if (inputs.Select(input => input.SupplierProductId).Distinct().Count() != inputs.Count)
        {
            throw new PurchaseOrderCatalogueInvalidException("A purchase order can contain each supplier catalogue item only once.");
        }

        var catalogueIds = inputs.Select(input => input.SupplierProductId).ToArray();
        var catalogueItems = await dbContext.SupplierProducts.Where(item => catalogueIds.Contains(item.Id)).ToDictionaryAsync(item => item.Id, cancellationToken);
        if (catalogueItems.Count != catalogueIds.Length)
        {
            throw new PurchaseOrderCatalogueInvalidException("One or more supplier catalogue items do not exist.");
        }

        var products = await dbContext.Products.Where(product => catalogueItems.Values.Select(item => item.ProductId).Contains(product.Id)).ToDictionaryAsync(product => product.Id, cancellationToken);
        var lines = new List<PurchaseOrderLine>(inputs.Count);
        foreach (var input in inputs)
        {
            var catalogueItem = catalogueItems[input.SupplierProductId];
            if (catalogueItem.SupplierId != supplierId || !catalogueItem.IsActive || !products.TryGetValue(catalogueItem.ProductId, out var product) || !product.IsActive ||
                !product.TryConvertToBaseQuantity(catalogueItem.PurchaseUnitOfMeasure, input.Quantity, out _) || input.Quantity < catalogueItem.MinimumOrderQuantity)
            {
                throw new PurchaseOrderCatalogueInvalidException("A purchase-order line does not match an active supplier catalogue item or its minimum order quantity.");
            }

            lines.Add(PurchaseOrderLine.Create(catalogueItem, product.Sku, product.Name, input.Quantity));
        }

        return lines;
    }

    private async Task ValidateDraftLinesAsync(Guid supplierId, IReadOnlyCollection<PurchaseOrderLine> lines, CancellationToken cancellationToken)
    {
        var inputs = lines.Select(line => new PurchaseOrderLineInput(line.SupplierProductId, line.Quantity)).ToArray();
        await ResolveLinesAsync(supplierId, inputs, cancellationToken);
    }

    private static void EnsureDraft(PurchaseOrder purchaseOrder)
    {
        if (purchaseOrder.Status != PurchaseOrderStatus.Draft)
        {
            throw new PurchaseOrderImmutableException(purchaseOrder.Id);
        }
    }

    private static PurchaseOrderResponse ToResponse(PurchaseOrder purchaseOrder, string supplierCode, string supplierName) => new(
        purchaseOrder.Id,
        purchaseOrder.SupplierId,
        supplierCode,
        supplierName,
        purchaseOrder.Status,
        purchaseOrder.Lines.Select(line => new PurchaseOrderLineResponse(
            line.Id,
            line.SupplierProductId,
            line.ProductId,
            line.ProductSku,
            line.ProductName,
            line.SupplierSku,
            line.PurchaseUnitOfMeasure,
            line.Quantity,
            line.UnitPrice,
            line.CurrencyCode)).ToList(),
        purchaseOrder.CreatedAtUtc,
        purchaseOrder.UpdatedAtUtc);

    private DateTime UtcNow() => timeProvider.GetUtcNow().UtcDateTime;
}
