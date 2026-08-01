using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.Application.Inventory;
using Warehouse.Application.Products;
using Warehouse.Application.Warehouses;
using Warehouse.Application.Suppliers;
using Warehouse.Application.Purchasing;
using Warehouse.Application.Currencies;
using Warehouse.Application.Customers;
using Warehouse.Application.Sales;

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
        services.AddScoped<CustomerService>();
        services.AddScoped<SalesOrderService>();
        services.AddScoped<SupplierProductService>();
        services.AddScoped<PurchaseOrderService>();
        services.AddScoped<CurrencyService>();
        services.AddScoped<InventoryService>();
        services.AddScoped<Warehouse.Application.Receiving.GoodsReceiptService>();
        services.AddValidatorsFromAssemblyContaining<ProductService>();

        return services;
    }
}
