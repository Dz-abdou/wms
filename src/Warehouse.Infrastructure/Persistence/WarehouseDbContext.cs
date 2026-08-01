using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Warehouse.Infrastructure.Auditing;
using Warehouse.Application.Common.Persistence;
using Warehouse.Domain.Inventory;
using Warehouse.Domain.Products;
using Warehouse.Domain.Purchasing;
using Warehouse.Domain.Currencies;
using Warehouse.Domain.Suppliers;
using Warehouse.Domain.Receiving;
using Warehouse.Domain.Customers;
using Warehouse.Domain.Numbering;
using WarehouseEntity = Warehouse.Domain.Warehouses.Warehouse;
using Warehouse.Infrastructure.Identity;

namespace Warehouse.Infrastructure.Persistence;

public sealed class WarehouseDbContext(DbContextOptions<WarehouseDbContext> options, AuditSavePipeline auditSavePipeline)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options), IWarehouseDbContext
{
    public DbSet<Product> Products => Set<Product>();

    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();

    public DbSet<WarehouseEntity> Warehouses => Set<WarehouseEntity>();

    public DbSet<Supplier> Suppliers => Set<Supplier>();

    public DbSet<SupplierProduct> SupplierProducts => Set<SupplierProduct>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<CustomerContact> CustomerContacts => Set<CustomerContact>();

    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();

    public DbSet<DocumentNumberDefinition> DocumentNumberDefinitions => Set<DocumentNumberDefinition>();

    public DbSet<DocumentNumberSeries> DocumentNumberSeries => Set<DocumentNumberSeries>();

    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();

    public DbSet<GoodsReceipt> GoodsReceipts => Set<GoodsReceipt>();

    public DbSet<Currency> Currencies => Set<Currency>();

    public DbSet<InventoryBalance> InventoryBalances => Set<InventoryBalance>();

    public DbSet<InventoryAdjustment> InventoryAdjustments => Set<InventoryAdjustment>();

    public DbSet<CycleCount> CycleCounts => Set<CycleCount>();

    public DbSet<CycleCountLine> CycleCountLines => Set<CycleCountLine>();

    public DbSet<InventoryTransfer> InventoryTransfers => Set<InventoryTransfer>();

    public DbSet<InventoryTransferLine> InventoryTransferLines => Set<InventoryTransferLine>();

    public DbSet<InventoryMovement> InventoryMovements => Set<InventoryMovement>();

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

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken)
    {
        await using var transaction = await Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await operation(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
