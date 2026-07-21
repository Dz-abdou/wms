using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Warehouse.Api.Auth;
using Warehouse.Infrastructure.Identity;
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
                builder.UseSetting("Jwt:Issuer", "warehouse-tests");
                builder.UseSetting("Jwt:Audience", "warehouse-tests");
                builder.UseSetting("Jwt:SigningKey", "test-signing-key-must-be-at-least-32-characters-long");
            });

        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        await dbContext.Database.MigrateAsync();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        foreach (var role in new[] { AuthorizationPolicies.AdminRole, AuthorizationPolicies.ManagerRole, AuthorizationPolicies.OperatorRole }) await roles.CreateAsync(new IdentityRole<Guid>(role));
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser { UserName = "admin@warehouse.test", Email = "admin@warehouse.test" };
        await users.CreateAsync(user, "TestPassword123!");
        await users.AddToRoleAsync(user, AuthorizationPolicies.AdminRole);
        var token = await scope.ServiceProvider.GetRequiredService<JwtTokenService>().CreateAccessTokenAsync(user);
        Client = Factory.CreateClient();
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<HttpClient> CreateClientForRoleAsync(string role)
    {
        using var scope = Factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var identity = Guid.NewGuid().ToString("N");
        var user = new ApplicationUser { UserName = $"{identity}@warehouse.test", Email = $"{identity}@warehouse.test" };
        var result = await users.CreateAsync(user, "TestPassword123!");
        if (!result.Succeeded) throw new InvalidOperationException("Test user could not be created.");
        await users.AddToRoleAsync(user, role);
        var token = await scope.ServiceProvider.GetRequiredService<JwtTokenService>().CreateAccessTokenAsync(user);
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await Factory.DisposeAsync();
        await postgreSql.DisposeAsync();
    }
}