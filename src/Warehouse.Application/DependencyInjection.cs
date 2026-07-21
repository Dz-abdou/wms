using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.Application.Products;

namespace Warehouse.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<ProductService>();
        services.AddValidatorsFromAssemblyContaining<ProductService>();

        return services;
    }
}