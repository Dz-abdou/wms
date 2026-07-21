using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Warehouse.Application.Common.Auditing;
using Warehouse.Domain.Auditing;
using Warehouse.Domain.Common;
using Warehouse.Infrastructure.Auditing;

namespace Warehouse.IntegrationTests.Auditing;

public sealed class AuditPipelineTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer postgreSql = PostgreSqlTestContainer.Create();

    public async Task InitializeAsync() => await postgreSql.StartAsync();

    public async Task DisposeAsync() => await postgreSql.DisposeAsync();

    [Fact]
    public async Task SaveChangesAsync_creates_a_snapshot_with_the_final_parent_id_and_context_metadata()
    {
        var actorId = Guid.NewGuid();
        await using var scope = CreateScope(new AuditContext(actorId, "trace-123", "initial import"));
        var dbContext = scope.ServiceProvider.GetRequiredService<AuditTestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        var probe = AuditProbe.Create("first", "ignored", "refresh-secret");

        await dbContext.AuditProbes.AddAsync(probe);
        await dbContext.SaveChangesAsync();

        var trail = await dbContext.AuditHistory<AuditProbe>(probe.Id).ToListAsync();
        var idSnapshot = Assert.Single(trail.Where(entry => entry.PropertyPath == nameof(PersistentEntity.Id)));
        Assert.Equal(AuditAction.Create, idSnapshot.Action);
        Assert.Contains(probe.Id.ToString(), idSnapshot.NewValue);
        Assert.All(trail, entry =>
        {
            Assert.Equal(actorId, entry.ActorUserId);
            Assert.Equal("trace-123", entry.CorrelationId);
            Assert.Equal("initial import", entry.Reason);
            Assert.Equal(DateTimeKind.Utc, entry.ChangedAtUtc.Kind);
        });
    }

    [Fact]
    public async Task Update_writes_only_actual_nonignored_nonsecret_property_changes()
    {
        await using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuditTestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        var probe = AuditProbe.Create("before", "ignored", "refresh-secret");
        dbContext.Add(probe);
        await dbContext.SaveChangesAsync();

        probe.Update("after", "changed but ignored", "rotated-secret");
        await dbContext.SaveChangesAsync();

        var updates = await dbContext.AuditHistory<AuditProbe>(probe.Id)
            .Where(entry => entry.Action == AuditAction.Update)
            .ToListAsync();
        var update = Assert.Single(updates);
        Assert.Equal(nameof(AuditProbe.Name), update.PropertyPath);
        Assert.Equal("\"before\"", update.OldValue);
        Assert.Equal("\"after\"", update.NewValue);
    }

    [Fact]
    public async Task Delete_writes_one_deleted_marker()
    {
        await using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuditTestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        var probe = AuditProbe.Create("delete", "ignored", "refresh-secret");
        dbContext.Add(probe);
        await dbContext.SaveChangesAsync();

        dbContext.Remove(probe);
        await dbContext.SaveChangesAsync();

        var deletion = await dbContext.Set<AuditTrail<AuditProbe>>()
            .SingleAsync(entry => entry.EntityId == probe.Id && entry.Action == AuditAction.Delete);
        Assert.Equal("__deleted__", deletion.PropertyPath);
    }

    [Fact]
    public async Task Non_opted_in_and_disabled_entities_do_not_receive_audit_mappings_or_records()
    {
        await using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuditTestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        dbContext.Add(new NonAuditedProbe());
        dbContext.Add(new DisabledAuditProbe());
        await dbContext.SaveChangesAsync();

        Assert.Null(dbContext.Model.FindEntityType(typeof(AuditTrail<NonAuditedProbe>)));
        Assert.Null(dbContext.Model.FindEntityType(typeof(AuditTrail<DisabledAuditProbe>)));
    }

    [Fact]
    public async Task Caller_rollback_removes_parent_and_audit_rows_together()
    {
        await using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuditTestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        var probe = AuditProbe.Create("rollback", "ignored", "refresh-secret");

        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        dbContext.Add(probe);
        await dbContext.SaveChangesAsync();
        await transaction.RollbackAsync();
        dbContext.ChangeTracker.Clear();

        Assert.False(await dbContext.AuditProbes.AnyAsync(entry => entry.Id == probe.Id));
        Assert.False(await dbContext.Set<AuditTrail<AuditProbe>>().AnyAsync(entry => entry.EntityId == probe.Id));
    }

    [Fact]
    public async Task SaveChanges_writes_audit_rows_and_preserves_accept_all_changes_false()
    {
        await using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuditTestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        var probe = AuditProbe.Create("synchronous", "ignored", "refresh-secret");
        dbContext.Add(probe);

        var changes = dbContext.SaveChanges(acceptAllChangesOnSuccess: false);

        Assert.Equal(1, changes);
        Assert.Equal(EntityState.Added, dbContext.Entry(probe).State);
        Assert.True(await dbContext.Set<AuditTrail<AuditProbe>>().AnyAsync(entry => entry.EntityId == probe.Id));
        dbContext.ChangeTracker.AcceptAllChanges();
        Assert.Equal(EntityState.Unchanged, dbContext.Entry(probe).State);
    }

    private AsyncServiceScope CreateScope(IAuditContext? auditContext = null)
    {
        var services = new ServiceCollection();
        services.AddScoped<IAuditContext>(_ => auditContext ?? new AuditContext(null, null));
        services.AddAuditing();
        services.AddDbContext<AuditTestDbContext>(options => options.UseNpgsql(postgreSql.GetConnectionString()));
        return services.BuildServiceProvider().CreateAsyncScope();
    }

    private sealed class AuditTestDbContext(DbContextOptions<AuditTestDbContext> options, AuditSavePipeline auditSavePipeline) : DbContext(options)
    {
        public DbSet<AuditProbe> AuditProbes => Set<AuditProbe>();

        public DbSet<NonAuditedProbe> NonAuditedProbes => Set<NonAuditedProbe>();

        public DbSet<DisabledAuditProbe> DisabledAuditProbes => Set<DisabledAuditProbe>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuditProbe>(builder =>
            {
                builder.ToTable("AuditProbes");
                builder.HasKey(entity => entity.Id);
                builder.Property(entity => entity.Name).HasMaxLength(128).IsRequired();
                builder.Property(entity => entity.Ignored).HasMaxLength(128).IsRequired();
                builder.Property(entity => entity.RefreshToken).HasMaxLength(128).IsRequired();
            });
            modelBuilder.Entity<NonAuditedProbe>().HasKey(entity => entity.Id);
            modelBuilder.Entity<DisabledAuditProbe>().HasKey(entity => entity.Id);
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

    [AuditEntity]
    private sealed class AuditProbe : PersistentEntity
    {
        private AuditProbe(Guid id, string name, string ignored, string refreshToken) : base(id, DateTime.UtcNow, DateTime.UtcNow)
        {
            Name = name;
            Ignored = ignored;
            RefreshToken = refreshToken;
        }

        public string Name { get; private set; }

        [AuditIgnore]
        public string Ignored { get; private set; }

        public string RefreshToken { get; private set; }

        public static AuditProbe Create(string name, string ignored, string refreshToken) => new(Guid.NewGuid(), name, ignored, refreshToken);

        public void Update(string name, string ignored, string refreshToken)
        {
            Name = name;
            Ignored = ignored;
            RefreshToken = refreshToken;
        }
    }

    private sealed class NonAuditedProbe : PersistentEntity
    {
        public NonAuditedProbe() : base(Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow) { }
    }

    [AuditEntity]
    [AuditDisabled]
    private sealed class DisabledAuditProbe : PersistentEntity
    {
        public DisabledAuditProbe() : base(Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow) { }
    }
}
