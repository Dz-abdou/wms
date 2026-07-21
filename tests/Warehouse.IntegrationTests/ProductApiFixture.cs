using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class ProductApiCollection : ICollectionFixture<ProductApiFixture>
{
    public const string Name = "Product API";
}

public sealed class ProductApiFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer postgreSql = PostgreSqlTestContainer.Create();

    public WebApplicationFactory<Program> Factory { get; private set; } = null!;

    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await postgreSql.StartAsync();

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.UseSetting("ConnectionStrings:WarehouseDb", postgreSql.GetConnectionString());
            });

        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        await dbContext.Database.MigrateAsync();

        Client = Factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await Factory.DisposeAsync();
        await postgreSql.DisposeAsync();
    }
}