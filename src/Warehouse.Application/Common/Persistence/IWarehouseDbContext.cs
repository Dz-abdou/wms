using Microsoft.EntityFrameworkCore;
using Warehouse.Domain.Inventory;
using Warehouse.Domain.Products;
using WarehouseEntity = Warehouse.Domain.Warehouses.Warehouse;

namespace Warehouse.Application.Common.Persistence;

public interface IWarehouseDbContext
{
    DbSet<Product> Products { get; }

    DbSet<WarehouseEntity> Warehouses { get; }

    DbSet<InventoryBalance> InventoryBalances { get; }

    DbSet<InventoryMovement> InventoryMovements { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken);
}