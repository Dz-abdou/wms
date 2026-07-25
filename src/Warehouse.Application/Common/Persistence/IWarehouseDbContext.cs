using Microsoft.EntityFrameworkCore;
using Warehouse.Domain.Inventory;
using Warehouse.Domain.Products;
using WarehouseEntity = Warehouse.Domain.Warehouses.Warehouse;
using Warehouse.Domain.Suppliers;
using Warehouse.Domain.Purchasing;
using Warehouse.Domain.Currencies;

namespace Warehouse.Application.Common.Persistence;

public interface IWarehouseDbContext
{
    DbSet<Product> Products { get; }

    DbSet<ProductCategory> ProductCategories { get; }

    DbSet<WarehouseEntity> Warehouses { get; }

    DbSet<InventoryBalance> InventoryBalances { get; }

    DbSet<InventoryAdjustment> InventoryAdjustments { get; }
    DbSet<Supplier> Suppliers { get; }

    DbSet<SupplierProduct> SupplierProducts { get; }

    DbSet<PurchaseOrder> PurchaseOrders { get; }

    DbSet<Currency> Currencies { get; }


    DbSet<InventoryMovement> InventoryMovements { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken);
}
