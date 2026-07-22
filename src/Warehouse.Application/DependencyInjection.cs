using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.Application.Inventory;
using Warehouse.Application.Products;
using Warehouse.Application.Warehouses;

namespace Warehouse.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<ProductService>();
        services.AddScoped<WarehouseService>();
        services.AddScoped<InventoryService>();
        services.AddValidatorsFromAssemblyContaining<ProductService>();

        return services;
    }
}