using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.Application.Inventory;
using Warehouse.Application.Products;
using Warehouse.Application.Warehouses;
using Warehouse.Application.Suppliers;

namespace Warehouse.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<ProductService>();
        services.AddScoped<WarehouseService>();
        services.AddScoped<ProductCategoryService>();
        services.AddScoped<SupplierService>();
        services.AddScoped<InventoryService>();
        services.AddValidatorsFromAssemblyContaining<ProductService>();

        return services;
    }
}