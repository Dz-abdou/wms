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
                for (var lineIndex = 0; lineIndex < input.Lines.Count; lineIndex++)
                {
                    var line = input.Lines[lineIndex];
                    var product = await dbContext.Products.Include(candidate => candidate.UnitConversions).SingleOrDefaultAsync(candidate => candidate.Id == line.ProductId && candidate.IsActive, token)
                        ?? throw new InventoryProductNotFoundException(line.ProductId);
                    var warehouse = await dbContext.Warehouses.SingleOrDefaultAsync(candidate => candidate.Id == line.WarehouseId && candidate.IsActive, token)
                        ?? throw new InventoryWarehouseNotFoundException(line.WarehouseId);
                    if (!product.TryConvertToBaseQuantity(line.UnitOfMeasure, line.Quantity, out var quantityInBaseUnit)) throw new InventoryInvalidUnitOfMeasureException(line.ProductId, line.UnitOfMeasure);
                    var balance = await dbContext.InventoryBalances.SingleOrDefaultAsync(candidate => candidate.ProductId == line.ProductId && candidate.WarehouseId == line.WarehouseId, token)
                        ?? InventoryBalance.Create(line.ProductId, line.WarehouseId, timestamp, currentUser.UserId);
                    if (balance.Id == Guid.Empty) throw new InvalidOperationException();
                    if (!dbContext.InventoryBalances.Local.Contains(balance)) dbContext.InventoryBalances.Add(balance);
                    var delta = line.Direction == InventoryAdjustmentDirection.Increase ? quantityInBaseUnit : -quantityInBaseUnit;
                    if (delta < 0m && balance.Quantity < -delta)
                    {
                        throw new InsufficientInventoryException(
                            lineIndex,
                            line.ProductId,
                            line.WarehouseId,
                            balance.Quantity,
                            product.BaseUnitOfMeasure,
                            warehouse.Code,
                            warehouse.Name);
                    }
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

    public async Task<InventoryTransferDetailResponse> CreateTransferAsync(
        InventoryTransferInput input,
        CancellationToken cancellationToken)
    {
        InventoryTransferDetailResponse? response = null;
        try
        {
            await dbContext.ExecuteInTransactionAsync(async token =>
            {
                var now = UtcNow();
                var sourceWarehouse = await dbContext.Warehouses.SingleOrDefaultAsync(
                    warehouse => warehouse.Id == input.SourceWarehouseId && warehouse.IsActive,
                    token) ?? throw new InventoryWarehouseNotFoundException(input.SourceWarehouseId);
                var destinationWarehouse = await dbContext.Warehouses.SingleOrDefaultAsync(
                    warehouse => warehouse.Id == input.DestinationWarehouseId && warehouse.IsActive,
                    token) ?? throw new InventoryWarehouseNotFoundException(input.DestinationWarehouseId);
                var transfer = InventoryTransfer.Create(
                    sourceWarehouse.Id,
                    destinationWarehouse.Id,
                    input.Reference,
                    input.Note,
                    now,
                    currentUser.UserId);
                dbContext.InventoryTransfers.Add(transfer);
                var responseLines = new List<InventoryTransferLineResponse>();

                for (var lineIndex = 0; lineIndex < input.Lines.Count; lineIndex++)
                {
                    var inputLine = input.Lines[lineIndex];
                    var product = await dbContext.Products.Include(candidate => candidate.UnitConversions)
                        .SingleOrDefaultAsync(candidate => candidate.Id == inputLine.ProductId && candidate.IsActive, token)
                        ?? throw new InventoryProductNotFoundException(inputLine.ProductId);
                    if (!product.TryConvertToBaseQuantity(inputLine.UnitOfMeasure, inputLine.Quantity, out var quantityInBaseUnit))
                    {
                        throw new InventoryInvalidUnitOfMeasureException(product.Id, inputLine.UnitOfMeasure);
                    }

                    var sourceBalance = await dbContext.InventoryBalances.SingleOrDefaultAsync(balance =>
                        balance.ProductId == product.Id && balance.WarehouseId == sourceWarehouse.Id, token);
                    if (sourceBalance is null || sourceBalance.Quantity < quantityInBaseUnit)
                    {
                        throw new InsufficientInventoryException(
                            lineIndex,
                            product.Id,
                            sourceWarehouse.Id,
                            sourceBalance?.Quantity ?? 0m,
                            product.BaseUnitOfMeasure,
                            sourceWarehouse.Code,
                            sourceWarehouse.Name);
                    }

                    var destinationBalance = await dbContext.InventoryBalances.SingleOrDefaultAsync(balance =>
                        balance.ProductId == product.Id && balance.WarehouseId == destinationWarehouse.Id, token)
                        ?? InventoryBalance.Create(product.Id, destinationWarehouse.Id, now, currentUser.UserId);
                    if (!dbContext.InventoryBalances.Local.Contains(destinationBalance))
                    {
                        dbContext.InventoryBalances.Add(destinationBalance);
                    }

                    sourceBalance.ApplyAdjustment(-quantityInBaseUnit, now, currentUser.UserId);
                    destinationBalance.ApplyAdjustment(quantityInBaseUnit, now, currentUser.UserId);
                    var transferLine = InventoryTransferLine.Create(
                        transfer.Id,
                        lineIndex + 1,
                        product.Id,
                        inputLine.UnitOfMeasure,
                        inputLine.Quantity,
                        quantityInBaseUnit,
                        now,
                        currentUser.UserId);
                    var transferOut = InventoryMovement.CreateTransferOut(
                        transfer.Id,
                        product.Id,
                        sourceWarehouse.Id,
                        transferLine.UnitOfMeasure,
                        transferLine.QuantityInUnit,
                        transferLine.QuantityInBaseUnit,
                        sourceBalance.Quantity,
                        now,
                        currentUser.UserId);
                    var transferIn = InventoryMovement.CreateTransferIn(
                        transfer.Id,
                        product.Id,
                        destinationWarehouse.Id,
                        transferLine.UnitOfMeasure,
                        transferLine.QuantityInUnit,
                        transferLine.QuantityInBaseUnit,
                        destinationBalance.Quantity,
                        now,
                        currentUser.UserId);
                    transferLine.LinkMovements(transferOut.Id, transferIn.Id);
                    dbContext.InventoryTransferLines.Add(transferLine);
                    dbContext.InventoryMovements.AddRange(transferOut, transferIn);
                    responseLines.Add(new InventoryTransferLineResponse(
                        transferLine.Id,
                        transferLine.LineNumber,
                        product.Id,
                        product.Sku,
                        product.Name,
                        transferLine.UnitOfMeasure,
                        transferLine.QuantityInUnit,
                        transferLine.QuantityInBaseUnit,
                        transferOut.Id,
                        transferOut.BalanceAfter,
                        transferIn.Id,
                        transferIn.BalanceAfter));
                }

                await dbContext.SaveChangesAsync(token);
                response = new InventoryTransferDetailResponse(
                    transfer.Id,
                    sourceWarehouse.Id,
                    sourceWarehouse.Code,
                    sourceWarehouse.Name,
                    destinationWarehouse.Id,
                    destinationWarehouse.Code,
                    destinationWarehouse.Name,
                    transfer.Reference,
                    transfer.Note,
                    transfer.TransferredAtUtc,
                    responseLines);
            }, cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new InventoryConcurrencyException(exception);
        }

        return response ?? throw new InvalidOperationException("Inventory transfer did not produce a result.");
    }

    public async Task<InventoryTransferCandidateResponse> GetTransferCandidateAsync(
        InventoryTransferCandidateQuery query,
        CancellationToken cancellationToken)
    {
        if (!await dbContext.Warehouses.AnyAsync(
                warehouse => warehouse.Id == query.SourceWarehouseId && warehouse.IsActive,
                cancellationToken))
        {
            throw new InventoryWarehouseNotFoundException(query.SourceWarehouseId);
        }

        var product = await dbContext.Products.AsNoTracking().SingleOrDefaultAsync(
            candidate => candidate.Id == query.ProductId && candidate.IsActive,
            cancellationToken) ?? throw new InventoryProductNotFoundException(query.ProductId);
        var balance = await dbContext.InventoryBalances.AsNoTracking().SingleOrDefaultAsync(
            candidate => candidate.ProductId == product.Id && candidate.WarehouseId == query.SourceWarehouseId,
            cancellationToken);

        return new InventoryTransferCandidateResponse(
            product.Id,
            product.BaseUnitOfMeasure,
            balance?.Quantity ?? 0m);
    }

    public async Task<PagedResult<InventoryTransferListItemResponse>> GetTransfersAsync(
        InventoryTransferListQuery query,
        CancellationToken cancellationToken)
    {
        var transfers = from transfer in dbContext.InventoryTransfers.AsNoTracking()
                        join sourceWarehouse in dbContext.Warehouses.AsNoTracking() on transfer.SourceWarehouseId equals sourceWarehouse.Id
                        join destinationWarehouse in dbContext.Warehouses.AsNoTracking() on transfer.DestinationWarehouseId equals destinationWarehouse.Id
                        select new { transfer, sourceWarehouse, destinationWarehouse };
        if (query.SourceWarehouseId is { } sourceWarehouseId)
        {
            transfers = transfers.Where(item => item.transfer.SourceWarehouseId == sourceWarehouseId);
        }
        if (query.DestinationWarehouseId is { } destinationWarehouseId)
        {
            transfers = transfers.Where(item => item.transfer.DestinationWarehouseId == destinationWarehouseId);
        }
        if (!string.IsNullOrWhiteSpace(query.Reference))
        {
            var reference = query.Reference.Trim();
            transfers = transfers.Where(item => item.transfer.Reference != null && item.transfer.Reference.Contains(reference));
        }
        if (query.FromUtc is { } fromUtc) transfers = transfers.Where(item => item.transfer.TransferredAtUtc >= fromUtc);
        if (query.ToUtc is { } toUtc) transfers = transfers.Where(item => item.transfer.TransferredAtUtc <= toUtc);

        var totalCount = await transfers.CountAsync(cancellationToken);
        var items = await transfers
            .OrderByDescending(item => item.transfer.TransferredAtUtc)
            .ThenByDescending(item => item.transfer.Id)
            .Skip((query.Page - PaginationConstants.DefaultPage) * query.PageSize)
            .Take(query.PageSize)
            .Select(item => new InventoryTransferListItemResponse(
                item.transfer.Id,
                item.sourceWarehouse.Id,
                item.sourceWarehouse.Code,
                item.sourceWarehouse.Name,
                item.destinationWarehouse.Id,
                item.destinationWarehouse.Code,
                item.destinationWarehouse.Name,
                item.transfer.Reference,
                item.transfer.TransferredAtUtc,
                dbContext.InventoryTransferLines.Count(line => line.InventoryTransferId == item.transfer.Id)))
            .ToListAsync(cancellationToken);

        return new PagedResult<InventoryTransferListItemResponse>(items, query.Page, query.PageSize, totalCount);
    }

    public async Task<InventoryTransferDetailResponse> GetTransferByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var header = await (from transfer in dbContext.InventoryTransfers.AsNoTracking()
                            join sourceWarehouse in dbContext.Warehouses.AsNoTracking() on transfer.SourceWarehouseId equals sourceWarehouse.Id
                            join destinationWarehouse in dbContext.Warehouses.AsNoTracking() on transfer.DestinationWarehouseId equals destinationWarehouse.Id
                            where transfer.Id == id
                            select new { transfer, sourceWarehouse, destinationWarehouse })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new InventoryTransferNotFoundException(id);
        var lines = await (from line in dbContext.InventoryTransferLines.AsNoTracking()
                           join product in dbContext.Products.AsNoTracking() on line.ProductId equals product.Id
                           join transferOut in dbContext.InventoryMovements.AsNoTracking() on line.TransferOutMovementId equals transferOut.Id
                           join transferIn in dbContext.InventoryMovements.AsNoTracking() on line.TransferInMovementId equals transferIn.Id
                           where line.InventoryTransferId == id
                           orderby line.LineNumber
                           select new InventoryTransferLineResponse(
                               line.Id,
                               line.LineNumber,
                               product.Id,
                               product.Sku,
                               product.Name,
                               line.UnitOfMeasure,
                               line.QuantityInUnit,
                               line.QuantityInBaseUnit,
                               transferOut.Id,
                               transferOut.BalanceAfter,
                               transferIn.Id,
                               transferIn.BalanceAfter))
            .ToListAsync(cancellationToken);
        return new InventoryTransferDetailResponse(
            header.transfer.Id,
            header.sourceWarehouse.Id,
            header.sourceWarehouse.Code,
            header.sourceWarehouse.Name,
            header.destinationWarehouse.Id,
            header.destinationWarehouse.Code,
            header.destinationWarehouse.Name,
            header.transfer.Reference,
            header.transfer.Note,
            header.transfer.TransferredAtUtc,
            lines);
    }

    public async Task<PagedResult<InventoryMovementResponse>> GetMovementHistoryAsync(InventoryMovementListQuery query, CancellationToken cancellationToken)
    {
        var movements = from movement in dbContext.InventoryMovements.AsNoTracking()
                        join product in dbContext.Products.AsNoTracking() on movement.ProductId equals product.Id
                        join warehouse in dbContext.Warehouses.AsNoTracking() on movement.WarehouseId equals warehouse.Id
                        join adjustment in dbContext.InventoryAdjustments.AsNoTracking() on movement.InventoryAdjustmentId equals adjustment.Id into adjustments
                        from adjustment in adjustments.DefaultIfEmpty()
                        join receipt in dbContext.GoodsReceipts.AsNoTracking() on movement.GoodsReceiptId equals receipt.Id into receipts
                        from receipt in receipts.DefaultIfEmpty()
                        join cycleCount in dbContext.CycleCounts.AsNoTracking() on movement.CycleCountId equals cycleCount.Id into cycleCounts
                        from cycleCount in cycleCounts.DefaultIfEmpty()
                        join transfer in dbContext.InventoryTransfers.AsNoTracking() on movement.InventoryTransferId equals transfer.Id into transfers
                        from transfer in transfers.DefaultIfEmpty()
                        select new { movement, product, warehouse, adjustment, receipt, cycleCount, transfer };
        if (query.ProductId is { } productId) movements = movements.Where(item => item.movement.ProductId == productId);
        if (query.WarehouseId is { } warehouseId) movements = movements.Where(item => item.movement.WarehouseId == warehouseId);
        if (query.Type is { } type) movements = movements.Where(item => item.movement.Type == type);
        if (query.FromUtc is { } fromUtc) movements = movements.Where(item => item.movement.CreatedAtUtc >= fromUtc);
        if (query.ToUtc is { } toUtc) movements = movements.Where(item => item.movement.CreatedAtUtc <= toUtc);
        if (!string.IsNullOrWhiteSpace(query.Reference))
        {
            var reference = query.Reference.Trim();
            movements = movements.Where(item =>
                (item.adjustment != null && item.adjustment.Reference != null && item.adjustment.Reference.Contains(reference))
                || (item.receipt != null && item.receipt.Number.Contains(reference))
                || (item.cycleCount != null && item.cycleCount.Reference != null && item.cycleCount.Reference.Contains(reference))
                || (item.transfer != null && item.transfer.Reference != null && item.transfer.Reference.Contains(reference)));
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
                item.movement.GoodsReceiptId,
                item.movement.CycleCountId,
                item.movement.InventoryTransferId,
                item.product.Id,
                item.product.Sku,
                item.product.Name,
                item.warehouse.Id,
                item.warehouse.Code,
                item.warehouse.Name,
                item.adjustment == null ? null : item.adjustment.Reference,
                item.receipt == null ? null : item.receipt.Number,
                item.cycleCount == null ? null : item.cycleCount.Reference,
                item.transfer == null ? null : item.transfer.Reference,
                item.movement.Type.ToString(),
                item.movement.UnitOfMeasure,
                item.movement.QuantityDeltaInUnit,
                item.movement.QuantityDelta,
                item.movement.BalanceAfter,
                item.movement.CreatedAtUtc))
            .ToListAsync(cancellationToken);
        return new PagedResult<InventoryMovementResponse>(items, query.Page, query.PageSize, totalCount);
    }

    public async Task<PagedResult<InventoryOverviewItemResponse>> GetOverviewAsync(
        InventoryOverviewQuery query,
        CancellationToken cancellationToken)
    {
        var balances = from balance in dbContext.InventoryBalances.AsNoTracking()
                       join product in dbContext.Products.AsNoTracking() on balance.ProductId equals product.Id
                       join warehouse in dbContext.Warehouses.AsNoTracking() on balance.WarehouseId equals warehouse.Id
                       select new { balance, product, warehouse };

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToUpperInvariant();
            balances = balances.Where(item =>
                item.product.Sku.ToUpper().Contains(search) ||
                item.product.Name.ToUpper().Contains(search));
        }

        if (query.WarehouseId is { } warehouseId)
        {
            balances = balances.Where(item => item.warehouse.Id == warehouseId);
        }

        if (query.CategoryId is { } categoryId)
        {
            balances = balances.Where(item => item.product.CategoryId == categoryId);
        }

        if (query.IsActive is { } isActive)
        {
            balances = balances.Where(item => item.product.IsActive == isActive);
        }

        var totalCount = await balances.CountAsync(cancellationToken);
        var items = await balances
            .OrderBy(item => item.product.Sku)
            .ThenBy(item => item.warehouse.Code)
            .Skip((query.Page - PaginationConstants.DefaultPage) * query.PageSize)
            .Take(query.PageSize)
            .Select(item => new InventoryOverviewItemResponse(
                item.product.Id,
                item.product.Sku,
                item.product.Name,
                item.product.IsActive,
                item.warehouse.Id,
                item.warehouse.Code,
                item.warehouse.Name,
                item.balance.Quantity,
                item.product.BaseUnitOfMeasure,
                item.balance.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        return new PagedResult<InventoryOverviewItemResponse>(items, query.Page, query.PageSize, totalCount);
    }

    public async Task<CycleCountCandidateResponse> GetCycleCountCandidateAsync(
        CycleCountCandidateQuery query,
        CancellationToken cancellationToken)
    {
        if (!await dbContext.Warehouses.AnyAsync(
                warehouse => warehouse.Id == query.WarehouseId && warehouse.IsActive,
                cancellationToken))
        {
            throw new InventoryWarehouseNotFoundException(query.WarehouseId);
        }

        var product = await dbContext.Products.AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == query.ProductId && candidate.IsActive, cancellationToken)
            ?? throw new InventoryProductNotFoundException(query.ProductId);
        var balance = await dbContext.InventoryBalances.AsNoTracking()
            .SingleOrDefaultAsync(candidate =>
                candidate.ProductId == product.Id && candidate.WarehouseId == query.WarehouseId,
                cancellationToken);

        return new CycleCountCandidateResponse(
            product.Id,
            product.Sku,
            product.Name,
            product.BaseUnitOfMeasure,
            balance?.Quantity ?? 0m,
            balance?.Version ?? 0);
    }

    public async Task<CycleCountDetailResponse> CreateCycleCountAsync(
        CycleCountInput input,
        CancellationToken cancellationToken)
    {
        CycleCountDetailResponse? response = null;
        try
        {
            await dbContext.ExecuteInTransactionAsync(async token =>
            {
                var now = UtcNow();
                var warehouse = await dbContext.Warehouses.SingleOrDefaultAsync(
                    candidate => candidate.Id == input.WarehouseId && candidate.IsActive,
                    token) ?? throw new InventoryWarehouseNotFoundException(input.WarehouseId);
                var count = CycleCount.Create(input.WarehouseId, input.Reference, input.Note, now, currentUser.UserId);
                dbContext.CycleCounts.Add(count);
                var responseLines = new List<CycleCountLineResponse>();

                for (var lineIndex = 0; lineIndex < input.Lines.Count; lineIndex++)
                {
                    var inputLine = input.Lines[lineIndex];
                    var product = await dbContext.Products.Include(candidate => candidate.UnitConversions)
                        .SingleOrDefaultAsync(candidate => candidate.Id == inputLine.ProductId && candidate.IsActive, token)
                        ?? throw new InventoryProductNotFoundException(inputLine.ProductId);
                    var existingBalance = await dbContext.InventoryBalances.SingleOrDefaultAsync(candidate =>
                        candidate.ProductId == product.Id && candidate.WarehouseId == warehouse.Id, token);
                    var systemQuantity = existingBalance?.Quantity ?? 0m;
                    var systemVersion = existingBalance?.Version ?? 0;
                    if (systemQuantity != inputLine.SystemQuantityInBase || systemVersion != inputLine.SystemBalanceVersion)
                    {
                        throw new CycleCountStaleBalanceException(
                            lineIndex,
                            systemQuantity,
                            product.BaseUnitOfMeasure);
                    }

                    var countedQuantityInBase = ToCountedBaseQuantity(product, inputLine);
                    var line = CycleCountLine.Create(
                        count.Id,
                        lineIndex + 1,
                        product.Id,
                        systemQuantity,
                        systemVersion,
                        inputLine.CountedUnitOfMeasure,
                        inputLine.CountedQuantityInUnit,
                        countedQuantityInBase,
                        now,
                        currentUser.UserId);
                    dbContext.CycleCountLines.Add(line);
                    var variance = line.VarianceQuantityInBase;
                    if (variance != 0m)
                    {
                        var balance = existingBalance ?? InventoryBalance.Create(product.Id, warehouse.Id, now, currentUser.UserId);
                        if (existingBalance is null)
                        {
                            dbContext.InventoryBalances.Add(balance);
                        }

                        balance.ApplyAdjustment(variance, now, currentUser.UserId);
                        var movement = InventoryMovement.CreateCycleCount(
                            count.Id,
                            product.Id,
                            warehouse.Id,
                            product.BaseUnitOfMeasure,
                            variance,
                            variance,
                            balance.Quantity,
                            now,
                            currentUser.UserId);
                        dbContext.InventoryMovements.Add(movement);
                        line.LinkInventoryMovement(movement.Id);
                    }

                    responseLines.Add(ToCycleCountLineResponse(line, product));
                }

                await dbContext.SaveChangesAsync(token);
                response = new CycleCountDetailResponse(
                    count.Id,
                    warehouse.Id,
                    warehouse.Code,
                    warehouse.Name,
                    count.Reference,
                    count.Note,
                    count.CountedAtUtc,
                    responseLines);
            }, cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new InventoryConcurrencyException(exception);
        }

        return response ?? throw new InvalidOperationException("Cycle count did not produce a result.");
    }

    public async Task<PagedResult<CycleCountListItemResponse>> GetCycleCountsAsync(
        CycleCountListQuery query,
        CancellationToken cancellationToken)
    {
        var counts = from count in dbContext.CycleCounts.AsNoTracking()
                     join warehouse in dbContext.Warehouses.AsNoTracking() on count.WarehouseId equals warehouse.Id
                     select new { count, warehouse };
        if (query.WarehouseId is { } warehouseId) counts = counts.Where(item => item.count.WarehouseId == warehouseId);
        if (!string.IsNullOrWhiteSpace(query.Reference))
        {
            var reference = query.Reference.Trim();
            counts = counts.Where(item => item.count.Reference != null && item.count.Reference.Contains(reference));
        }
        if (query.FromUtc is { } fromUtc) counts = counts.Where(item => item.count.CountedAtUtc >= fromUtc);
        if (query.ToUtc is { } toUtc) counts = counts.Where(item => item.count.CountedAtUtc <= toUtc);
        var totalCount = await counts.CountAsync(cancellationToken);
        var items = await counts
            .OrderByDescending(item => item.count.CountedAtUtc)
            .ThenByDescending(item => item.count.Id)
            .Skip((query.Page - PaginationConstants.DefaultPage) * query.PageSize)
            .Take(query.PageSize)
            .Select(item => new CycleCountListItemResponse(
                item.count.Id,
                item.warehouse.Id,
                item.warehouse.Code,
                item.warehouse.Name,
                item.count.Reference,
                item.count.CountedAtUtc,
                dbContext.CycleCountLines.Count(line => line.CycleCountId == item.count.Id),
                dbContext.CycleCountLines.Count(line => line.CycleCountId == item.count.Id && line.VarianceQuantityInBase != 0m)))
            .ToListAsync(cancellationToken);
        return new PagedResult<CycleCountListItemResponse>(items, query.Page, query.PageSize, totalCount);
    }

    public async Task<CycleCountDetailResponse> GetCycleCountByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var header = await (from count in dbContext.CycleCounts.AsNoTracking()
                            join warehouse in dbContext.Warehouses.AsNoTracking() on count.WarehouseId equals warehouse.Id
                            where count.Id == id
                            select new { count, warehouse })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new CycleCountNotFoundException(id);
        var lines = await (from line in dbContext.CycleCountLines.AsNoTracking()
                           join product in dbContext.Products.AsNoTracking() on line.ProductId equals product.Id
                           where line.CycleCountId == id
                           orderby line.LineNumber
                           select new CycleCountLineResponse(
                               line.Id,
                               line.LineNumber,
                               product.Id,
                               product.Sku,
                               product.Name,
                               line.SystemQuantityInBase,
                               line.SystemBalanceVersion,
                               product.BaseUnitOfMeasure,
                               line.CountedUnitOfMeasure,
                               line.CountedQuantityInUnit,
                               line.CountedQuantityInBase,
                               line.VarianceQuantityInBase,
                               line.InventoryMovementId))
            .ToListAsync(cancellationToken);
        return new CycleCountDetailResponse(
            header.count.Id,
            header.warehouse.Id,
            header.warehouse.Code,
            header.warehouse.Name,
            header.count.Reference,
            header.count.Note,
            header.count.CountedAtUtc,
            lines);
    }

    private DateTime UtcNow() => timeProvider.GetUtcNow().UtcDateTime;
    private static InventoryBalanceResponse ToResponse(InventoryBalance balance, string baseUnitOfMeasure) => new(balance.ProductId, balance.WarehouseId, balance.Quantity, balance.UpdatedAtUtc, baseUnitOfMeasure);

    private static decimal ToCountedBaseQuantity(Product product, CycleCountLineInput line)
    {
        if (line.CountedQuantityInUnit == 0m)
        {
            if (!product.TryConvertToBaseQuantity(line.CountedUnitOfMeasure, 1m, out _))
            {
                throw new InventoryInvalidUnitOfMeasureException(product.Id, line.CountedUnitOfMeasure);
            }

            return 0m;
        }

        if (!product.TryConvertToBaseQuantity(line.CountedUnitOfMeasure, line.CountedQuantityInUnit, out var quantityInBase))
        {
            throw new InventoryInvalidUnitOfMeasureException(product.Id, line.CountedUnitOfMeasure);
        }

        return quantityInBase;
    }

    private static CycleCountLineResponse ToCycleCountLineResponse(CycleCountLine line, Product product) => new(
        line.Id,
        line.LineNumber,
        product.Id,
        product.Sku,
        product.Name,
        line.SystemQuantityInBase,
        line.SystemBalanceVersion,
        product.BaseUnitOfMeasure,
        line.CountedUnitOfMeasure,
        line.CountedQuantityInUnit,
        line.CountedQuantityInBase,
        line.VarianceQuantityInBase,
        line.InventoryMovementId);
}
