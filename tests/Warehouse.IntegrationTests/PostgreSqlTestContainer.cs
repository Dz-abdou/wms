using Testcontainers.PostgreSql;

namespace Warehouse.IntegrationTests;

public static class PostgreSqlTestContainer
{
    public const string Image = "postgres:17-alpine";

    public static PostgreSqlContainer Create() => new PostgreSqlBuilder(Image).Build();
}