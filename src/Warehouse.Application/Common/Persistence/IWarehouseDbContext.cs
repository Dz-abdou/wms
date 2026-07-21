using Microsoft.EntityFrameworkCore;
using Warehouse.Domain.Products;

namespace Warehouse.Application.Common.Persistence;

public interface IWarehouseDbContext
{
    DbSet<Product> Products { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}