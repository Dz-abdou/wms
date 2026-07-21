using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Warehouse.IntegrationTests;

public sealed class HealthEndpointTests
{
    [Fact]
    public async Task Readiness_endpoint_returns_ok_when_postgresql_is_available()
    {
        await using var postgreSql = PostgreSqlTestContainer.Create();
        await postgreSql.StartAsync();

        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.UseSetting("ConnectionStrings:WarehouseDb", postgreSql.GetConnectionString());
                builder.UseSetting("Jwt:Issuer", "warehouse-tests");
                builder.UseSetting("Jwt:Audience", "warehouse-tests");
                builder.UseSetting("Jwt:SigningKey", "test-signing-key-must-be-at-least-32-characters-long");
            });

        using var client = factory.CreateClient();
        var response = await client.GetAsync("/health/ready");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }
}