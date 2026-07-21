using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Warehouse.Application.Common.Auditing;

namespace Warehouse.Infrastructure.Auditing;

public static class AuditingServiceCollectionExtensions
{
    public static IServiceCollection AddAuditing(this IServiceCollection services)
    {
        services.TryAddScoped<IAuditContext>(_ => new AuditContext(null, null));
        services.AddScoped<IAuditProfileProvider, AuditProfileProvider>();
        services.AddSingleton<IAuditValueSerializer, AuditValueSerializer>();
        services.AddScoped<IAuditDiffEngine, AuditDiffEngine>();
        services.AddScoped<IAuditEventFactory, AuditEventFactory>();
        services.AddScoped<IAuditTrailWriter, AuditTrailWriter>();
        services.AddScoped<AuditSavePipeline>();
        return services;
    }
}
