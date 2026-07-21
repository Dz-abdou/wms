using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Common.Persistence;
using Warehouse.Domain.Products;
using WarehouseEntity = Warehouse.Domain.Warehouses.Warehouse;

namespace Warehouse.Infrastructure.Persistence;

public sealed class WarehouseDbContext(DbContextOptions<WarehouseDbContext> options)
    : DbContext(options), IWarehouseDbContext
{
    public DbSet<Product> Products => Set<Product>();

    public DbSet<WarehouseEntity> Warehouses => Set<WarehouseEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WarehouseDbContext).Assembly);
    }
}