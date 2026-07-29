using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Common.Identity;
using Warehouse.Application.Common.Models;
using Warehouse.Application.Common.Pagination;
using Warehouse.Application.Common.Persistence;
using Warehouse.Domain.Inventory;
using Warehouse.Domain.Purchasing;
using Warehouse.Domain.Receiving;

namespace Warehouse.Application.Receiving;

public sealed class GoodsReceiptService(IWarehouseDbContext db, TimeProvider clock, ICurrentUser user)
{
    public async Task<PagedResult<GoodsReceiptListItemResponse>> GetListAsync(
        GoodsReceiptListQuery query,
        CancellationToken cancellationToken)
    {
        var receipts = from receipt in db.GoodsReceipts.AsNoTracking()
                       join order in db.PurchaseOrders.AsNoTracking() on receipt.PurchaseOrderId equals order.Id
                       join warehouse in db.Warehouses.AsNoTracking() on receipt.WarehouseId equals warehouse.Id
                       select new { receipt, order, warehouse };

        if (query.PurchaseOrderId is { } purchaseOrderId)
        {
            receipts = receipts.Where(item => item.receipt.PurchaseOrderId == purchaseOrderId);
        }

        if (!string.IsNullOrWhiteSpace(query.PurchaseOrderNumber))
        {
            var purchaseOrderNumber = query.PurchaseOrderNumber.Trim();
            receipts = receipts.Where(item => item.order.Number != null && item.order.Number.Contains(purchaseOrderNumber));
        }

        if (query.WarehouseId is { } warehouseId)
        {
            receipts = receipts.Where(item => item.receipt.WarehouseId == warehouseId);
        }

        var totalCount = await receipts.CountAsync(cancellationToken);
        var items = await receipts
            .OrderByDescending(item => item.receipt.ReceivedAtUtc)
            .ThenByDescending(item => item.receipt.Id)
            .Skip((query.Page - PaginationConstants.DefaultPage) * query.PageSize)
            .Take(query.PageSize)
            .Select(item => new GoodsReceiptListItemResponse(
                item.receipt.Id,
                item.receipt.Number,
                item.order.Id,
                item.order.Number!,
                item.warehouse.Id,
                item.warehouse.Code,
                item.warehouse.Name,
                item.receipt.ReceivedAtUtc,
                item.receipt.Lines.Count))
            .ToListAsync(cancellationToken);

        return new PagedResult<GoodsReceiptListItemResponse>(items, query.Page, query.PageSize, totalCount);
    }

    public async Task<GoodsReceiptDetailResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var receipt = await db.GoodsReceipts.AsNoTracking()
            .Include(candidate => candidate.Lines)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken)
            ?? throw new GoodsReceiptNotFoundException();
        var order = await db.PurchaseOrders.AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == receipt.PurchaseOrderId, cancellationToken)
            ?? throw new GoodsReceiptNotFoundException();
        var warehouse = await db.Warehouses.AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == receipt.WarehouseId, cancellationToken)
            ?? throw new GoodsReceiptNotFoundException();

        return new GoodsReceiptDetailResponse(
            receipt.Id,
            receipt.Number,
            order.Id,
            order.Number ?? string.Empty,
            warehouse.Id,
            warehouse.Code,
            warehouse.Name,
            receipt.ReceivedAtUtc,
            receipt.SupplierDeliveryNote,
            receipt.Notes,
            receipt.ReceiverUserId,
            receipt.CreatedAtUtc,
            receipt.Lines
                .OrderBy(line => line.PurchaseOrderLineNumber)
                .Select(line => new GoodsReceiptLineResponse(
                    line.Id,
                    line.PurchaseOrderLineId,
                    line.PurchaseOrderLineNumber,
                    line.ProductId,
                    line.ProductSku,
                    line.ProductName,
                    line.UnitOfMeasure,
                    line.AcceptedQuantity,
                    line.AcceptedQuantityInBaseUnit,
                    line.ConversionFactorToBaseUnit,
                    line.InventoryMovementId))
                .ToList());
    }

    public async Task<GoodsReceiptCandidateResponse> GetCandidateAsync(Guid purchaseOrderId, CancellationToken cancellationToken)
    {
        var order = await db.PurchaseOrders.AsNoTracking().Include(order => order.Lines).SingleOrDefaultAsync(order => order.Id == purchaseOrderId, cancellationToken)
            ?? throw new GoodsReceiptPurchaseOrderUnavailableException();
        if (order.Status is not (PurchaseOrderStatus.Submitted or PurchaseOrderStatus.PartiallyReceived) || order.DestinationWarehouseId is null || order.Number is null)
            throw new GoodsReceiptPurchaseOrderUnavailableException();
        var warehouse = await db.Warehouses.AsNoTracking().SingleOrDefaultAsync(candidate => candidate.Id == order.DestinationWarehouseId, cancellationToken)
            ?? throw new GoodsReceiptPurchaseOrderUnavailableException();
        var received = await db.GoodsReceipts.AsNoTracking().Where(receipt => receipt.PurchaseOrderId == order.Id).SelectMany(receipt => receipt.Lines).GroupBy(line => line.PurchaseOrderLineId).ToDictionaryAsync(group => group.Key, group => group.Sum(line => line.AcceptedQuantity), cancellationToken);
        return new GoodsReceiptCandidateResponse(order.Id, order.Number, order.DestinationWarehouseId.Value, warehouse.Code, warehouse.Name, order.CurrencyCode, order.Version, order.Lines.Select(line => new GoodsReceiptCandidateLineResponse(line.Id, line.LineNumber, line.ProductSku, line.ProductName, line.PurchaseUnitOfMeasure, line.Quantity, received.GetValueOrDefault(line.Id), line.Quantity - received.GetValueOrDefault(line.Id), line.ConversionFactorToBaseUnit)).ToList());
    }

    public async Task<GoodsReceiptResponse> CreateAsync(GoodsReceiptInput input, CancellationToken cancellationToken)
    {
        GoodsReceiptResponse? response = null;
        try
        {
            await db.ExecuteInTransactionAsync(async token =>
            {
                var order = await db.PurchaseOrders.Include(order => order.Lines).SingleOrDefaultAsync(order => order.Id == input.PurchaseOrderId, token)
                    ?? throw new GoodsReceiptPurchaseOrderUnavailableException();
                if (order.Status is not (PurchaseOrderStatus.Submitted or PurchaseOrderStatus.PartiallyReceived)) throw new GoodsReceiptPurchaseOrderUnavailableException();
                if (order.Version != input.PurchaseOrderVersion) throw new GoodsReceiptConcurrencyException();
                var actor = user.UserId ?? throw new GoodsReceiptPurchaseOrderUnavailableException();
                var existing = await db.GoodsReceipts.AsNoTracking().Where(receipt => receipt.PurchaseOrderId == order.Id).SelectMany(receipt => receipt.Lines).GroupBy(line => line.PurchaseOrderLineId).ToDictionaryAsync(group => group.Key, group => group.Sum(line => line.AcceptedQuantity), token);
                var now = clock.GetUtcNow().UtcDateTime;
                var sequence = GoodsReceiptNumberSequence.Create(now.Year); db.GoodsReceiptNumberSequences.Add(sequence); await db.SaveChangesAsync(token);
                var receipt = GoodsReceipt.Create(sequence.ToNumber(), order.Id, order.DestinationWarehouseId ?? throw new GoodsReceiptPurchaseOrderUnavailableException(), input.ReceivedAtUtc, input.SupplierDeliveryNote, input.Notes, actor);
                db.GoodsReceipts.Add(receipt);
                var receiptLines = new List<GoodsReceiptLine>();
                foreach (var (lineInput, index) in input.Lines.Select((line, index) => (line, index)))
                {
                    var orderLine = order.Lines.SingleOrDefault(line => line.Id == lineInput.PurchaseOrderLineId);
                    if (orderLine is null) throw new GoodsReceiptPurchaseOrderLineUnavailableException(index);
                    var received = existing.GetValueOrDefault(orderLine.Id);
                    if (received + lineInput.AcceptedQuantity > orderLine.Quantity) throw new GoodsReceiptOverReceiptException(index);
                    var balance = await db.InventoryBalances.SingleOrDefaultAsync(balance => balance.ProductId == orderLine.ProductId && balance.WarehouseId == receipt.WarehouseId, token) ?? InventoryBalance.Create(orderLine.ProductId, receipt.WarehouseId, now, actor);
                    if (!db.InventoryBalances.Local.Contains(balance)) db.InventoryBalances.Add(balance);
                    var baseQuantity = lineInput.AcceptedQuantity * orderLine.ConversionFactorToBaseUnit;
                    balance.ApplyAdjustment(baseQuantity, now, actor);
                    var movement = InventoryMovement.CreateGoodsReceipt(receipt.Id, orderLine.ProductId, receipt.WarehouseId, orderLine.PurchaseUnitOfMeasure, lineInput.AcceptedQuantity, baseQuantity, balance.Quantity, now, actor);
                    db.InventoryMovements.Add(movement);
                    receiptLines.Add(GoodsReceiptLine.Create(orderLine.Id, orderLine.LineNumber, orderLine.ProductId, orderLine.ProductSku, orderLine.ProductName, orderLine.PurchaseUnitOfMeasure, lineInput.AcceptedQuantity, orderLine.ConversionFactorToBaseUnit, movement.Id));
                    existing[orderLine.Id] = received + lineInput.AcceptedQuantity;
                }
                receipt.AddLines(receiptLines);
                var complete = order.Lines.All(line => existing.GetValueOrDefault(line.Id) >= line.Quantity);
                order.ApplyReceiptProgress(complete, now, actor);
                await db.SaveChangesAsync(token);
                response = new GoodsReceiptResponse(receipt.Id, receipt.Number, order.Id, receipt.WarehouseId, receipt.ReceivedAtUtc, order.Version);
            }, cancellationToken);
        }
        catch (DbUpdateConcurrencyException) { throw new GoodsReceiptConcurrencyException(); }
        return response ?? throw new InvalidOperationException("Goods receipt did not produce a result.");
    }
}
