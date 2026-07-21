using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.Application.Common.Persistence;
using Warehouse.Infrastructure.Auditing;
using Warehouse.Infrastructure.Identity;
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
        services.AddAuditing();
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredLength = 12;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Lockout.AllowedForNewUsers = true;
        })
        .AddRoles<IdentityRole<Guid>>()
        .AddEntityFrameworkStores<WarehouseDbContext>()
        .AddSignInManager();
        services.Configure<DevelopmentAdminOptions>(configuration.GetSection(DevelopmentAdminOptions.SectionName));
        services.AddScoped<IdentityBootstrapper>();
        services.AddScoped<IWarehouseDbContext>(provider => provider.GetRequiredService<WarehouseDbContext>());

        return services;
    }
}