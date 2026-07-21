using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Warehouse.Infrastructure.Auditing;
using Warehouse.Application.Common.Persistence;
using Warehouse.Domain.Products;
using WarehouseEntity = Warehouse.Domain.Warehouses.Warehouse;
using Warehouse.Infrastructure.Identity;

namespace Warehouse.Infrastructure.Persistence;

public sealed class WarehouseDbContext(DbContextOptions<WarehouseDbContext> options, AuditSavePipeline auditSavePipeline)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options), IWarehouseDbContext
{
    public DbSet<Product> Products => Set<Product>();

    public DbSet<WarehouseEntity> Warehouses => Set<WarehouseEntity>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WarehouseDbContext).Assembly);
        modelBuilder.ApplyAuditTrailMappings();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess) => auditSavePipeline.Save(
        this,
        acceptAllChangesOnSuccess,
        acceptAllChanges => base.SaveChanges(acceptAllChanges));

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default) => auditSavePipeline.SaveAsync(
        this,
        acceptAllChangesOnSuccess,
        (acceptAllChanges, token) => base.SaveChangesAsync(acceptAllChanges, token),
        cancellationToken);
}
