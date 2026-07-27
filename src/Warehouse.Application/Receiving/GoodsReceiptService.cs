using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Common.Identity;
using Warehouse.Application.Common.Persistence;
using Warehouse.Domain.Inventory;
using Warehouse.Domain.Purchasing;
using Warehouse.Domain.Receiving;

namespace Warehouse.Application.Receiving;

public sealed class GoodsReceiptService(IWarehouseDbContext db, TimeProvider clock, ICurrentUser user)
{
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
                var existing = await db.GoodsReceipts.Where(receipt => receipt.PurchaseOrderId == order.Id).SelectMany(receipt => receipt.Lines).GroupBy(line => line.PurchaseOrderLineId).ToDictionaryAsync(group => group.Key, group => group.Sum(line => line.AcceptedQuantity), token);
                var now = clock.GetUtcNow().UtcDateTime;
                var sequence = GoodsReceiptNumberSequence.Create(now.Year); db.GoodsReceiptNumberSequences.Add(sequence); await db.SaveChangesAsync(token);
                var receipt = GoodsReceipt.Create(sequence.ToNumber(), order.Id, order.DestinationWarehouseId ?? throw new GoodsReceiptPurchaseOrderUnavailableException(), input.ReceivedAtUtc, input.SupplierDeliveryNote, input.Notes, actor);
                db.GoodsReceipts.Add(receipt);
                var receiptLines = new List<GoodsReceiptLine>();
                foreach (var (lineInput, index) in input.Lines.Select((line, index) => (line, index)))
                {
                    var orderLine = order.Lines.SingleOrDefault(line => line.Id == lineInput.PurchaseOrderLineId);
                    if (orderLine is null || lineInput.AcceptedQuantity <= 0m) throw new GoodsReceiptOverReceiptException(index);
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
