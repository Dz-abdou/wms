using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Common.Identity;
using Warehouse.Application.Common.Models;
using Warehouse.Application.Common.Pagination;
using Warehouse.Application.Common.Persistence;
using Warehouse.Domain.Inventory;

namespace Warehouse.Application.Inventory;

public sealed class InventoryService(
    IWarehouseDbContext dbContext,
    TimeProvider timeProvider,
    ICurrentUser currentUser)
{
    public async Task<InventoryBalanceResponse> AdjustAsync(
        InventoryAdjustmentInput input,
        CancellationToken cancellationToken)
    {
        InventoryBalanceResponse? response = null;

        try
        {
            await dbContext.ExecuteInTransactionAsync(async token =>
            {
                if (!await dbContext.Products.AnyAsync(product => product.Id == input.ProductId, token))
                {
                    throw new InventoryProductNotFoundException(input.ProductId);
                }

                if (!await dbContext.Warehouses.AnyAsync(warehouse => warehouse.Id == input.WarehouseId, token))
                {
                    throw new InventoryWarehouseNotFoundException(input.WarehouseId);
                }

                var balance = await dbContext.InventoryBalances.SingleOrDefaultAsync(
                    candidate => candidate.ProductId == input.ProductId && candidate.WarehouseId == input.WarehouseId,
                    token);

                if (balance is null)
                {
                    balance = InventoryBalance.Create(input.ProductId, input.WarehouseId, UtcNow(), currentUser.UserId);
                    dbContext.InventoryBalances.Add(balance);
                }

                var quantityDelta = input.Direction == InventoryAdjustmentDirection.Increase
                    ? input.Quantity
                    : -input.Quantity;
                if (quantityDelta < 0m && balance.Quantity < -quantityDelta)
                {
                    throw new InsufficientInventoryException(input.ProductId, input.WarehouseId);
                }

                balance.ApplyAdjustment(quantityDelta, UtcNow(), currentUser.UserId);
                var movement = InventoryMovement.CreateManualAdjustment(
                    input.ProductId,
                    input.WarehouseId,
                    quantityDelta,
                    balance.Quantity,
                    UtcNow(),
                    currentUser.UserId);
                dbContext.InventoryMovements.Add(movement);

                await dbContext.SaveChangesAsync(token);
                response = ToResponse(balance);
            }, cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new InventoryConcurrencyException(exception);
        }

        return response ?? throw new InvalidOperationException("Inventory adjustment did not produce a balance.");
    }

    public async Task<PagedResult<InventoryMovementResponse>> GetMovementHistoryAsync(
        InventoryMovementListQuery query,
        CancellationToken cancellationToken)
    {
        var movements = dbContext.InventoryMovements.AsNoTracking();
        if (query.ProductId is { } productId)
        {
            movements = movements.Where(movement => movement.ProductId == productId);
        }

        if (query.WarehouseId is { } warehouseId)
        {
            movements = movements.Where(movement => movement.WarehouseId == warehouseId);
        }

        var totalCount = await movements.CountAsync(cancellationToken);
        var skip = (query.Page - PaginationConstants.DefaultPage) * query.PageSize;
        var items = await movements
            .OrderByDescending(movement => movement.CreatedAtUtc)
            .ThenByDescending(movement => movement.Id)
            .Skip(skip)
            .Take(query.PageSize)
            .Select(movement => new InventoryMovementResponse(
                movement.Id,
                movement.ProductId,
                movement.WarehouseId,
                movement.Type,
                movement.QuantityDelta,
                movement.BalanceAfter,
                movement.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return new PagedResult<InventoryMovementResponse>(items, query.Page, query.PageSize, totalCount);
    }

    private DateTime UtcNow() => timeProvider.GetUtcNow().UtcDateTime;

    private static InventoryBalanceResponse ToResponse(InventoryBalance balance) => new(
        balance.ProductId,
        balance.WarehouseId,
        balance.Quantity,
        balance.UpdatedAtUtc);
}
