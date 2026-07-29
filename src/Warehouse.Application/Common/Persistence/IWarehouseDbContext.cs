using Microsoft.EntityFrameworkCore;
using Warehouse.Domain.Inventory;
using Warehouse.Domain.Products;
using WarehouseEntity = Warehouse.Domain.Warehouses.Warehouse;
using Warehouse.Domain.Suppliers;
using Warehouse.Domain.Purchasing;
using Warehouse.Domain.Currencies;
using Warehouse.Domain.Receiving;

namespace Warehouse.Application.Common.Persistence;

public interface IWarehouseDbContext
{
    DbSet<Product> Products { get; }

    DbSet<ProductCategory> ProductCategories { get; }

    DbSet<WarehouseEntity> Warehouses { get; }

    DbSet<InventoryBalance> InventoryBalances { get; }

    DbSet<InventoryAdjustment> InventoryAdjustments { get; }

    DbSet<CycleCount> CycleCounts { get; }

    DbSet<CycleCountLine> CycleCountLines { get; }
    DbSet<Supplier> Suppliers { get; }

    DbSet<SupplierProduct> SupplierProducts { get; }

    DbSet<PurchaseOrder> PurchaseOrders { get; }

    DbSet<PurchaseOrderNumberSequence> PurchaseOrderNumberSequences { get; }
    DbSet<GoodsReceipt> GoodsReceipts { get; }
    DbSet<GoodsReceiptNumberSequence> GoodsReceiptNumberSequences { get; }

    DbSet<Currency> Currencies { get; }


    DbSet<InventoryMovement> InventoryMovements { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken);
}
