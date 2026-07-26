using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Common.Identity;
using Warehouse.Application.Common.Errors;
using Warehouse.Application.Common.Models;
using Warehouse.Application.Common.Pagination;
using Warehouse.Application.Common.Persistence;
using Warehouse.Application.Products;
using Warehouse.Application.Suppliers;
using Warehouse.Domain.Products;
using Warehouse.Domain.Purchasing;
using WarehouseEntity = Warehouse.Domain.Warehouses.Warehouse;

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
                     join warehouse in dbContext.Warehouses.AsNoTracking() on purchaseOrder.DestinationWarehouseId equals warehouse.Id into warehouses
                     from warehouse in warehouses.DefaultIfEmpty()
                     select new { purchaseOrder, supplier, warehouse };
        if (query.Status is { } status)
        {
            orders = orders.Where(item => item.purchaseOrder.Status == status);
        }
        if (query.SupplierId is { } supplierId)
        {
            orders = orders.Where(item => item.purchaseOrder.SupplierId == supplierId);
        }
        if (query.WarehouseId is { } warehouseId) orders = orders.Where(item => item.purchaseOrder.DestinationWarehouseId == warehouseId);
        if (query.FromOrderDate is { } fromDate) orders = orders.Where(item => item.purchaseOrder.OrderDate >= fromDate);
        if (query.ToOrderDate is { } toDate) orders = orders.Where(item => item.purchaseOrder.OrderDate <= toDate);

        var totalCount = await orders.CountAsync(cancellationToken);
        var page = await orders
            .OrderByDescending(item => item.purchaseOrder.CreatedAtUtc)
            .Skip((query.Page - PaginationConstants.DefaultPage) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);
        var items = page.Select(item => ToResponse(item.purchaseOrder, item.supplier.Code, item.supplier.Name, item.warehouse)).ToList();
        return new PagedResult<PurchaseOrderResponse>(items, query.Page, query.PageSize, totalCount);
    }

    public async Task<PurchaseOrderResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var purchaseOrder = await dbContext.PurchaseOrders.AsNoTracking().Include(order => order.Lines)
            .SingleOrDefaultAsync(order => order.Id == id, cancellationToken)
            ?? throw new PurchaseOrderNotFoundException(id);
        var supplier = await dbContext.Suppliers.AsNoTracking().SingleAsync(candidate => candidate.Id == purchaseOrder.SupplierId, cancellationToken);
        var warehouse = purchaseOrder.DestinationWarehouseId is { } warehouseId
            ? await dbContext.Warehouses.AsNoTracking().SingleOrDefaultAsync(candidate => candidate.Id == warehouseId, cancellationToken)
            : null;
        return ToResponse(purchaseOrder, supplier.Code, supplier.Name, warehouse);
    }

    public async Task<PurchaseOrderResponse> CreateAsync(PurchaseOrderInput input, CancellationToken cancellationToken)
    {
        await EnsureSupplierIsActiveAsync(input.SupplierId, cancellationToken);
        await EnsureWarehouseIsActiveAsync(input.DestinationWarehouseId, cancellationToken);
        var lines = await ResolveLinesAsync(input.SupplierId, input.CurrencyCode, input.Lines ?? [], cancellationToken);
        var now = UtcNow();
        var sequence = PurchaseOrderNumberSequence.Create(now.Year);
        dbContext.PurchaseOrderNumberSequences.Add(sequence);
        await dbContext.SaveChangesAsync(cancellationToken);
        var buyerUserId = currentUser.UserId ?? throw new PurchaseOrderCatalogueInvalidException("An authenticated buyer is required.");
        var purchaseOrder = PurchaseOrder.Create(sequence.ToNumber(), input.SupplierId, input.DestinationWarehouseId, input.CurrencyCode!, input.OrderDate, input.ExpectedDeliveryDate, buyerUserId, input.SupplierReference, input.Notes, now);
        purchaseOrder.ReplaceLines(lines, UtcNow(), currentUser.UserId);
        dbContext.PurchaseOrders.Add(purchaseOrder);
        await SaveWithConcurrencyHandlingAsync(purchaseOrder.Id, cancellationToken);
        return await GetByIdAsync(purchaseOrder.Id, cancellationToken);
    }

    public async Task<PurchaseOrderResponse> UpdateAsync(Guid id, PurchaseOrderInput input, CancellationToken cancellationToken)
    {
        var purchaseOrder = await FindTrackedAsync(id, cancellationToken);
        EnsureDraft(purchaseOrder);
        if (input.Version != purchaseOrder.Version) throw new PurchaseOrderConcurrencyException(id);
        await EnsureSupplierIsActiveAsync(input.SupplierId, cancellationToken);
        await EnsureWarehouseIsActiveAsync(input.DestinationWarehouseId, cancellationToken);
        var lines = await ResolveLinesAsync(input.SupplierId, input.CurrencyCode, input.Lines ?? [], cancellationToken);
        var updatedAtUtc = UtcNow();
        purchaseOrder.UpdateOperationalDetails(input.SupplierId, input.DestinationWarehouseId, input.CurrencyCode!, input.OrderDate, input.ExpectedDeliveryDate, input.SupplierReference, input.Notes, input.Version ?? -1, updatedAtUtc, currentUser.UserId ?? Guid.Empty);
        purchaseOrder.ReplaceLines(lines, updatedAtUtc, currentUser.UserId);
        await SaveWithConcurrencyHandlingAsync(id, cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<PurchaseOrderResponse> SubmitAsync(Guid id, PurchaseOrderVersionInput input, CancellationToken cancellationToken)
    {
        var purchaseOrder = await FindTrackedAsync(id, cancellationToken);
        EnsureDraft(purchaseOrder);
        if (input.Version != purchaseOrder.Version) throw new PurchaseOrderConcurrencyException(id);
        if (purchaseOrder.Lines.Count == 0)
        {
            throw new PurchaseOrderSubmissionInvalidException("A purchase order requires at least one line before submission.");
        }

        await EnsureSupplierIsActiveAsync(purchaseOrder.SupplierId, cancellationToken);
        await ValidateDraftLinesAsync(purchaseOrder.SupplierId, purchaseOrder.CurrencyCode, purchaseOrder.Lines, cancellationToken);
        purchaseOrder.Submit(UtcNow(), currentUser.UserId);
        await SaveWithConcurrencyHandlingAsync(id, cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<PurchaseOrderResponse> CancelAsync(Guid id, PurchaseOrderCancelInput input, CancellationToken cancellationToken)
    {
        var purchaseOrder = await FindTrackedAsync(id, cancellationToken);
        if (input.Version != purchaseOrder.Version) throw new PurchaseOrderConcurrencyException(id);
        if (purchaseOrder.Status is not (PurchaseOrderStatus.Draft or PurchaseOrderStatus.Submitted))
            throw new PurchaseOrderInvalidTransitionException(id);
        var actorUserId = currentUser.UserId ?? throw new PurchaseOrderCatalogueInvalidException("An authenticated buyer is required.");
        purchaseOrder.Cancel(input.Reason, UtcNow(), actorUserId);
        await SaveWithConcurrencyHandlingAsync(id, cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    private async Task<PurchaseOrder> FindTrackedAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.PurchaseOrders.Include(order => order.Lines).SingleOrDefaultAsync(order => order.Id == id, cancellationToken)
        ?? throw new PurchaseOrderNotFoundException(id);

    private async Task EnsureSupplierIsActiveAsync(Guid supplierId, CancellationToken cancellationToken)
    {
        var supplier = await dbContext.Suppliers.SingleOrDefaultAsync(candidate => candidate.Id == supplierId, cancellationToken)
            ?? throw new PurchaseOrderFieldValidationException(
                "SupplierId",
                ApiErrorCodes.PurchaseOrderSupplierUnavailable,
                "The selected supplier no longer exists.");
        if (!supplier.IsActive)
        {
            throw new PurchaseOrderFieldValidationException(
                "SupplierId",
                ApiErrorCodes.PurchaseOrderSupplierUnavailable,
                "The selected supplier is inactive.");
        }
    }

    private async Task EnsureWarehouseIsActiveAsync(Guid warehouseId, CancellationToken cancellationToken)
    {
        var warehouse = await dbContext.Warehouses.SingleOrDefaultAsync(candidate => candidate.Id == warehouseId, cancellationToken);
        if (warehouse is null || !warehouse.IsActive)
        {
            throw new PurchaseOrderFieldValidationException(
                "DestinationWarehouseId",
                ApiErrorCodes.PurchaseOrderWarehouseUnavailable,
                "The selected destination warehouse is unavailable.");
        }
    }

    private async Task<IReadOnlyCollection<PurchaseOrderLine>> ResolveLinesAsync(Guid supplierId, string? currencyCode, IReadOnlyCollection<PurchaseOrderLineInput> inputs, CancellationToken cancellationToken)
    {
        var duplicateLine = inputs
            .Select((input, index) => (input, index))
            .GroupBy(item => item.input.SupplierProductId)
            .SelectMany(group => group.Skip(1))
            .FirstOrDefault();
        if (duplicateLine.input is not null)
        {
            throw new PurchaseOrderFieldValidationException(
                $"Lines[{duplicateLine.index}].SupplierProductId",
                ApiErrorCodes.PurchaseOrderDuplicateCatalogueItem,
                "Each supplier catalogue item can be selected only once.");
        }

        var catalogueIds = inputs.Select(input => input.SupplierProductId).ToArray();
        var catalogueItems = await dbContext.SupplierProducts.Where(item => catalogueIds.Contains(item.Id)).ToDictionaryAsync(item => item.Id, cancellationToken);
        var products = await dbContext.Products.Where(product => catalogueItems.Values.Select(item => item.ProductId).Contains(product.Id)).ToDictionaryAsync(product => product.Id, cancellationToken);
        var lines = new List<PurchaseOrderLine>(inputs.Count);
        foreach (var (input, lineIndex) in inputs.Select((input, index) => (input, index)))
        {
            if (!catalogueItems.TryGetValue(input.SupplierProductId, out var catalogueItem)
                || catalogueItem.SupplierId != supplierId
                || !catalogueItem.IsActive
                || !products.TryGetValue(catalogueItem.ProductId, out var product)
                || !product.IsActive
                || !product.TryConvertToBaseQuantity(catalogueItem.PurchaseUnitOfMeasure, input.Quantity, out _))
            {
                throw new PurchaseOrderFieldValidationException(
                    $"Lines[{lineIndex}].SupplierProductId",
                    ApiErrorCodes.PurchaseOrderCatalogueItemUnavailable,
                    "The selected supplier catalogue item is unavailable. Choose an active item for the selected supplier.");
            }

            if (input.Quantity < catalogueItem.MinimumOrderQuantity)
            {
                throw new PurchaseOrderMinimumOrderQuantityException(lineIndex, catalogueItem.MinimumOrderQuantity);
            }
            if (!string.Equals(catalogueItem.CurrencyCode, currencyCode, StringComparison.OrdinalIgnoreCase))
            {
                throw new PurchaseOrderFieldValidationException(
                    "CurrencyCode",
                    ApiErrorCodes.PurchaseOrderCurrencyMismatch,
                    "The purchase-order currency must match the selected catalogue item currency.");
            }

            lines.Add(PurchaseOrderLine.Create(lineIndex + 1, catalogueItem, product, input.Quantity));
        }

        return lines;
    }

    private async Task ValidateDraftLinesAsync(Guid supplierId, string? currencyCode, IReadOnlyCollection<PurchaseOrderLine> lines, CancellationToken cancellationToken)
    {
        var inputs = lines.Select(line => new PurchaseOrderLineInput(line.SupplierProductId, line.Quantity)).ToArray();
        await ResolveLinesAsync(supplierId, currencyCode, inputs, cancellationToken);
    }

    private static void EnsureDraft(PurchaseOrder purchaseOrder)
    {
        if (purchaseOrder.Status != PurchaseOrderStatus.Draft)
        {
            throw new PurchaseOrderImmutableException(purchaseOrder.Id);
        }
    }

    private async Task SaveWithConcurrencyHandlingAsync(Guid purchaseOrderId, CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new PurchaseOrderConcurrencyException(purchaseOrderId, exception);
        }
    }

    private static PurchaseOrderResponse ToResponse(PurchaseOrder purchaseOrder, string supplierCode, string supplierName, WarehouseEntity? warehouse) => new(
        purchaseOrder.Id,
        purchaseOrder.SupplierId,
        supplierCode,
        supplierName,
        purchaseOrder.Number,
        purchaseOrder.DestinationWarehouseId,
        warehouse?.Code,
        warehouse?.Name,
        purchaseOrder.CurrencyCode,
        purchaseOrder.OrderDate,
        purchaseOrder.ExpectedDeliveryDate,
        purchaseOrder.BuyerUserId,
        purchaseOrder.SupplierReference,
        purchaseOrder.Notes,
        purchaseOrder.Status,
        purchaseOrder.Lines.Select(line => new PurchaseOrderLineResponse(
            line.Id,
            line.LineNumber,
            line.SupplierProductId,
            line.ProductId,
            line.ProductSku,
            line.ProductName,
            line.SupplierSku,
            line.PurchaseUnitOfMeasure,
            line.Quantity,
            line.QuantityInBaseUnit,
            line.ConversionFactorToBaseUnit,
            line.UnitPrice,
            line.CurrencyCode,
            line.LineAmount)).ToList(),
        purchaseOrder.Lines.Sum(line => line.LineAmount),
        purchaseOrder.Version,
        purchaseOrder.SubmittedAtUtc,
        purchaseOrder.StatusHistory.Select(history => new PurchaseOrderStatusHistoryResponse(
            history.Id,
            history.PreviousStatus,
            history.Status,
            history.ChangedAtUtc,
            history.ActorUserId,
            history.Reason)).ToList(),
        purchaseOrder.CreatedAtUtc,
        purchaseOrder.UpdatedAtUtc);

    private DateTime UtcNow() => timeProvider.GetUtcNow().UtcDateTime;
}
