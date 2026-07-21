using Microsoft.EntityFrameworkCore;
using Warehouse.Domain.Products;
using WarehouseEntity = Warehouse.Domain.Warehouses.Warehouse;

namespace Warehouse.Application.Common.Persistence;

public interface IWarehouseDbContext
{
    DbSet<Product> Products { get; }

    DbSet<WarehouseEntity> Warehouses { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}