using FluentValidation;
using Warehouse.Api.Auth;
using Warehouse.Api.Endpoints;
using Warehouse.Application.Suppliers;

namespace Warehouse.Api.Endpoints.Suppliers;

public static class SupplierEndpoints
{
    public static IEndpointRouteBuilder MapSupplierEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(SupplierApiRoutes.BasePath).WithTags("Suppliers").AddEndpointFilter<SupplierExceptionEndpointFilter>();
        group.MapGet("", GetListAsync).RequireAuthorization(AuthorizationPolicies.ReadCatalog).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ReadCatalog));
        group.MapGet("/{id:guid}", GetByIdAsync).WithName(SupplierApiRoutes.GetByIdRouteName).RequireAuthorization(AuthorizationPolicies.ReadCatalog).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ReadCatalog));
        group.MapPost("", CreateAsync).RequireAuthorization(AuthorizationPolicies.ManageCatalog).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ManageCatalog));
        group.MapPut("/{id:guid}", UpdateAsync).RequireAuthorization(AuthorizationPolicies.ManageCatalog).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ManageCatalog));
        group.MapPatch("/{id:guid}/status", SetStatusAsync).RequireAuthorization(AuthorizationPolicies.ManageCatalog).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ManageCatalog));
        return endpoints;
    }

    private static async Task<IResult> GetListAsync([AsParameters] SupplierListQuery query, IValidator<SupplierListQuery> validator, SupplierService service, CancellationToken cancellationToken)
    {
        var problem = await validator.ValidateRequestAsync(query, cancellationToken);
        return problem ?? Results.Ok(await service.GetListAsync(query, cancellationToken));
    }
    private static async Task<IResult> GetByIdAsync(Guid id, SupplierService service, CancellationToken cancellationToken) => Results.Ok(await service.GetByIdAsync(id, cancellationToken));
    private static async Task<IResult> CreateAsync(SupplierInput input, IValidator<SupplierInput> validator, SupplierService service, CancellationToken cancellationToken)
    {
        var problem = await validator.ValidateRequestAsync(input, cancellationToken);
        if (problem is not null) return problem;
        var supplier = await service.CreateAsync(input, cancellationToken);
        return Results.CreatedAtRoute(SupplierApiRoutes.GetByIdRouteName, new { id = supplier.Id }, supplier);
    }
    private static async Task<IResult> UpdateAsync(Guid id, SupplierInput input, IValidator<SupplierInput> validator, SupplierService service, CancellationToken cancellationToken)
    {
        var problem = await validator.ValidateRequestAsync(input, cancellationToken);
        return problem ?? Results.Ok(await service.UpdateAsync(id, input, cancellationToken));
    }
    private static async Task<IResult> SetStatusAsync(Guid id, SetSupplierStatusRequest request, SupplierService service, CancellationToken cancellationToken) => Results.Ok(await service.SetStatusAsync(id, request, cancellationToken));
}
