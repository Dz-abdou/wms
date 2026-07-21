using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.Application.Common.Persistence;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("WarehouseDb")
            ?? throw new InvalidOperationException("Connection string 'WarehouseDb' is required.");

        services.AddDbContext<WarehouseDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IWarehouseDbContext>(provider => provider.GetRequiredService<WarehouseDbContext>());

        return services;
    }
}