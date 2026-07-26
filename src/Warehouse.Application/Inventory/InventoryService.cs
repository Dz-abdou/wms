using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Common.Identity;
using Warehouse.Application.Common.Models;
using Warehouse.Application.Common.Pagination;
using Warehouse.Application.Common.Persistence;
using Warehouse.Domain.Inventory;
using Warehouse.Domain.Products;

namespace Warehouse.Application.Inventory;

public sealed class InventoryService(IWarehouseDbContext dbContext, TimeProvider timeProvider, ICurrentUser currentUser)
{
    public async Task<InventoryAdjustmentResponse> AdjustAsync(InventoryAdjustmentInput input, CancellationToken cancellationToken)
    {
        InventoryAdjustmentResponse? response = null;
        try
        {
            await dbContext.ExecuteInTransactionAsync(async token =>
            {
                var timestamp = UtcNow();
                var adjustment = InventoryAdjustment.Create(input.Reason, input.Reference, input.Note, timestamp, currentUser.UserId);
                dbContext.InventoryAdjustments.Add(adjustment);
                var results = new List<InventoryBalanceResponse>();
                foreach (var line in input.Lines)
                {
                    var product = await dbContext.Products.Include(candidate => candidate.UnitConversions).SingleOrDefaultAsync(candidate => candidate.Id == line.ProductId && candidate.IsActive, token)
                        ?? throw new InventoryProductNotFoundException(line.ProductId);
                    if (!await dbContext.Warehouses.AnyAsync(warehouse => warehouse.Id == line.WarehouseId && warehouse.IsActive, token)) throw new InventoryWarehouseNotFoundException(line.WarehouseId);
                    if (!product.TryConvertToBaseQuantity(line.UnitOfMeasure, line.Quantity, out var quantityInBaseUnit)) throw new InventoryInvalidUnitOfMeasureException(line.ProductId, line.UnitOfMeasure);
                    var balance = await dbContext.InventoryBalances.SingleOrDefaultAsync(candidate => candidate.ProductId == line.ProductId && candidate.WarehouseId == line.WarehouseId, token)
                        ?? InventoryBalance.Create(line.ProductId, line.WarehouseId, timestamp, currentUser.UserId);
                    if (balance.Id == Guid.Empty) throw new InvalidOperationException();
                    if (!dbContext.InventoryBalances.Local.Contains(balance)) dbContext.InventoryBalances.Add(balance);
                    var delta = line.Direction == InventoryAdjustmentDirection.Increase ? quantityInBaseUnit : -quantityInBaseUnit;
                    if (delta < 0m && balance.Quantity < -delta) throw new InsufficientInventoryException(line.ProductId, line.WarehouseId);
                    var quantityDeltaInUnit = line.Direction == InventoryAdjustmentDirection.Increase ? line.Quantity : -line.Quantity;
                    balance.ApplyAdjustment(delta, timestamp, currentUser.UserId);
                    dbContext.InventoryMovements.Add(InventoryMovement.CreateManualAdjustment(line.ProductId, line.WarehouseId, ProductUnitOfMeasure.NormalizeUnitOfMeasure(line.UnitOfMeasure), quantityDeltaInUnit, delta, balance.Quantity, timestamp, currentUser.UserId, adjustment.Id));
                    results.Add(ToResponse(balance, product.BaseUnitOfMeasure));
                }
                await dbContext.SaveChangesAsync(token);
                response = new InventoryAdjustmentResponse(adjustment.Id, adjustment.Reason, adjustment.Reference, adjustment.Note, adjustment.CreatedAtUtc, results);
            }, cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception) { throw new InventoryConcurrencyException(exception); }
        return response ?? throw new InvalidOperationException("Inventory adjustment did not produce a result.");
    }

    public async Task<PagedResult<InventoryAdjustmentListItemResponse>> GetAdjustmentsAsync(InventoryAdjustmentListQuery query, CancellationToken cancellationToken)
    {
        var adjustments = dbContext.InventoryAdjustments.AsNoTracking();
        if (query.Reason is { } reason) adjustments = adjustments.Where(adjustment => adjustment.Reason == reason);
        if (!string.IsNullOrWhiteSpace(query.Reference))
        {
            var reference = query.Reference.Trim();
            adjustments = adjustments.Where(adjustment => adjustment.Reference != null && adjustment.Reference.Contains(reference));
        }
        if (query.FromUtc is { } fromUtc) adjustments = adjustments.Where(adjustment => adjustment.CreatedAtUtc >= fromUtc);
        if (query.ToUtc is { } toUtc) adjustments = adjustments.Where(adjustment => adjustment.CreatedAtUtc <= toUtc);
        var totalCount = await adjustments.CountAsync(cancellationToken);
        var items = await adjustments
            .OrderByDescending(adjustment => adjustment.CreatedAtUtc)
            .ThenByDescending(adjustment => adjustment.Id)
            .Skip((query.Page - PaginationConstants.DefaultPage) * query.PageSize)
            .Take(query.PageSize)
            .Select(adjustment => new InventoryAdjustmentListItemResponse(
                adjustment.Id,
                adjustment.Reason,
                adjustment.Reference,
                adjustment.CreatedAtUtc,
                dbContext.InventoryMovements.Count(movement => movement.InventoryAdjustmentId == adjustment.Id)))
            .ToListAsync(cancellationToken);
        return new PagedResult<InventoryAdjustmentListItemResponse>(items, query.Page, query.PageSize, totalCount);
    }

    public async Task<InventoryAdjustmentDetailResponse> GetAdjustmentByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var adjustment = await dbContext.InventoryAdjustments.AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken)
            ?? throw new InventoryAdjustmentNotFoundException(id);
        var lines = await (from movement in dbContext.InventoryMovements.AsNoTracking()
                           join product in dbContext.Products.AsNoTracking() on movement.ProductId equals product.Id
                           join warehouse in dbContext.Warehouses.AsNoTracking() on movement.WarehouseId equals warehouse.Id
                           where movement.InventoryAdjustmentId == adjustment.Id
                           orderby movement.CreatedAtUtc, movement.Id
                           select new InventoryAdjustmentLineResponse(
                               movement.Id,
                               product.Id,
                               product.Sku,
                               product.Name,
                               warehouse.Id,
                               warehouse.Code,
                               warehouse.Name,
                               movement.Type.ToString(),
                               movement.UnitOfMeasure,
                               movement.QuantityDeltaInUnit,
                               movement.QuantityDelta,
                               movement.BalanceAfter,
                               movement.CreatedAtUtc))
            .ToListAsync(cancellationToken);
        return new InventoryAdjustmentDetailResponse(
            adjustment.Id,
            adjustment.Reason,
            adjustment.Reference,
            adjustment.Note,
            adjustment.CreatedAtUtc,
            lines);
    }

    public async Task<PagedResult<InventoryMovementResponse>> GetMovementHistoryAsync(InventoryMovementListQuery query, CancellationToken cancellationToken)
    {
        var movements = from movement in dbContext.InventoryMovements.AsNoTracking()
                        join product in dbContext.Products.AsNoTracking() on movement.ProductId equals product.Id
                        join warehouse in dbContext.Warehouses.AsNoTracking() on movement.WarehouseId equals warehouse.Id
                        join adjustment in dbContext.InventoryAdjustments.AsNoTracking() on movement.InventoryAdjustmentId equals adjustment.Id into adjustments
                        from adjustment in adjustments.DefaultIfEmpty()
                        select new { movement, product, warehouse, adjustment };
        if (query.ProductId is { } productId) movements = movements.Where(item => item.movement.ProductId == productId);
        if (query.WarehouseId is { } warehouseId) movements = movements.Where(item => item.movement.WarehouseId == warehouseId);
        if (query.Type is { } type) movements = movements.Where(item => item.movement.Type == type);
        if (query.FromUtc is { } fromUtc) movements = movements.Where(item => item.movement.CreatedAtUtc >= fromUtc);
        if (query.ToUtc is { } toUtc) movements = movements.Where(item => item.movement.CreatedAtUtc <= toUtc);
        if (!string.IsNullOrWhiteSpace(query.Reference))
        {
            var reference = query.Reference.Trim();
            movements = movements.Where(item => item.adjustment != null && item.adjustment.Reference != null && item.adjustment.Reference.Contains(reference));
        }
        var totalCount = await movements.CountAsync(cancellationToken);
        var items = await movements
            .OrderByDescending(item => item.movement.CreatedAtUtc)
            .ThenByDescending(item => item.movement.Id)
            .Skip((query.Page - PaginationConstants.DefaultPage) * query.PageSize)
            .Take(query.PageSize)
            .Select(item => new InventoryMovementResponse(
                item.movement.Id,
                item.movement.InventoryAdjustmentId,
                item.product.Id,
                item.product.Sku,
                item.product.Name,
                item.warehouse.Id,
                item.warehouse.Code,
                item.warehouse.Name,
                item.adjustment == null ? null : item.adjustment.Reference,
                item.movement.Type.ToString(),
                item.movement.UnitOfMeasure,
                item.movement.QuantityDeltaInUnit,
                item.movement.QuantityDelta,
                item.movement.BalanceAfter,
                item.movement.CreatedAtUtc))
            .ToListAsync(cancellationToken);
        return new PagedResult<InventoryMovementResponse>(items, query.Page, query.PageSize, totalCount);
    }

    private DateTime UtcNow() => timeProvider.GetUtcNow().UtcDateTime;
    private static InventoryBalanceResponse ToResponse(InventoryBalance balance, string baseUnitOfMeasure) => new(balance.ProductId, balance.WarehouseId, balance.Quantity, balance.UpdatedAtUtc, baseUnitOfMeasure);
}
