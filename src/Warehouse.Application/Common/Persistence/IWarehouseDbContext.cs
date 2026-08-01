using Microsoft.EntityFrameworkCore;
using Warehouse.Domain.Inventory;
using Warehouse.Domain.Products;
using WarehouseEntity = Warehouse.Domain.Warehouses.Warehouse;
using Warehouse.Domain.Suppliers;
using Warehouse.Domain.Purchasing;
using Warehouse.Domain.Currencies;
using Warehouse.Domain.Receiving;
using Warehouse.Domain.Customers;
using Warehouse.Domain.Numbering;
using Warehouse.Domain.Sales;

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

    DbSet<InventoryTransfer> InventoryTransfers { get; }

    DbSet<InventoryTransferLine> InventoryTransferLines { get; }
    DbSet<Supplier> Suppliers { get; }

    DbSet<SupplierProduct> SupplierProducts { get; }

    DbSet<Customer> Customers { get; }

    DbSet<CustomerContact> CustomerContacts { get; }

    DbSet<CustomerAddress> CustomerAddresses { get; }

    DbSet<SalesOrder> SalesOrders { get; }

    DbSet<DocumentNumberDefinition> DocumentNumberDefinitions { get; }

    DbSet<DocumentNumberSeries> DocumentNumberSeries { get; }

    DbSet<PurchaseOrder> PurchaseOrders { get; }

    DbSet<GoodsReceipt> GoodsReceipts { get; }

    DbSet<Currency> Currencies { get; }


    DbSet<InventoryMovement> InventoryMovements { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken);
}
