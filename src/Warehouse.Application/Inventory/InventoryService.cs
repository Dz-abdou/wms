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

    public async Task<PagedResult<InventoryMovementResponse>> GetMovementHistoryAsync(InventoryMovementListQuery query, CancellationToken cancellationToken)
    {
        var movements = dbContext.InventoryMovements.AsNoTracking();
        if (query.ProductId is { } productId) movements = movements.Where(movement => movement.ProductId == productId);
        if (query.WarehouseId is { } warehouseId) movements = movements.Where(movement => movement.WarehouseId == warehouseId);
        var totalCount = await movements.CountAsync(cancellationToken);
        var items = await movements.OrderByDescending(movement => movement.CreatedAtUtc).ThenByDescending(movement => movement.Id).Skip((query.Page - PaginationConstants.DefaultPage) * query.PageSize).Take(query.PageSize).Select(movement => new InventoryMovementResponse(movement.Id, movement.ProductId, movement.WarehouseId, movement.Type.ToString(), movement.UnitOfMeasure, movement.QuantityDeltaInUnit, movement.QuantityDelta, movement.BalanceAfter, movement.CreatedAtUtc)).ToListAsync(cancellationToken);
        return new PagedResult<InventoryMovementResponse>(items, query.Page, query.PageSize, totalCount);
    }

    private DateTime UtcNow() => timeProvider.GetUtcNow().UtcDateTime;
    private static InventoryBalanceResponse ToResponse(InventoryBalance balance, string baseUnitOfMeasure) => new(balance.ProductId, balance.WarehouseId, balance.Quantity, balance.UpdatedAtUtc, baseUnitOfMeasure);
}
