using FluentValidation;
using Warehouse.Api.Auth;
using Warehouse.Api.Endpoints;
using Warehouse.Application.Purchasing;

namespace Warehouse.Api.Endpoints.Purchasing;

public static class SupplierProductEndpoints
{
    public static IEndpointRouteBuilder MapSupplierProductEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(PurchasingApiRoutes.SupplierProductsBasePath).WithTags("Supplier catalogue").AddEndpointFilter<PurchasingExceptionEndpointFilter>();
        group.MapGet("", GetListAsync).RequireAuthorization(AuthorizationPolicies.ReadCatalog).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ReadCatalog));
        group.MapGet("/{id:guid}", GetByIdAsync).WithName(PurchasingApiRoutes.SupplierProductByIdRouteName).RequireAuthorization(AuthorizationPolicies.ReadCatalog).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ReadCatalog));
        group.MapPost("", CreateAsync).RequireAuthorization(AuthorizationPolicies.ManageCatalog).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ManageCatalog));
        group.MapPut("/{id:guid}", UpdateAsync).RequireAuthorization(AuthorizationPolicies.ManageCatalog).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ManageCatalog));
        group.MapPatch("/{id:guid}/status", SetStatusAsync).RequireAuthorization(AuthorizationPolicies.ManageCatalog).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ManageCatalog));
        return endpoints;
    }

    private static async Task<IResult> GetListAsync([AsParameters] SupplierProductListQuery query, IValidator<SupplierProductListQuery> validator, SupplierProductService service, CancellationToken cancellationToken)
    {
        var problem = await validator.ValidateRequestAsync(query, cancellationToken);
        return problem ?? Results.Ok(await service.GetListAsync(query, cancellationToken));
    }

    private static async Task<IResult> GetByIdAsync(Guid id, SupplierProductService service, CancellationToken cancellationToken) => Results.Ok(await service.GetByIdAsync(id, cancellationToken));

    private static async Task<IResult> CreateAsync(SupplierProductInput input, IValidator<SupplierProductInput> validator, SupplierProductService service, CancellationToken cancellationToken)
    {
        var problem = await validator.ValidateRequestAsync(input, cancellationToken);
        if (problem is not null) return problem;
        var catalogueItem = await service.CreateAsync(input, cancellationToken);
        return Results.CreatedAtRoute(PurchasingApiRoutes.SupplierProductByIdRouteName, new { id = catalogueItem.Id }, catalogueItem);
    }

    private static async Task<IResult> UpdateAsync(Guid id, UpdateSupplierProductInput input, IValidator<UpdateSupplierProductInput> validator, SupplierProductService service, CancellationToken cancellationToken)
    {
        var problem = await validator.ValidateRequestAsync(input, cancellationToken);
        return problem ?? Results.Ok(await service.UpdateAsync(id, input, cancellationToken));
    }

    private static async Task<IResult> SetStatusAsync(Guid id, SetSupplierProductStatusRequest request, SupplierProductService service, CancellationToken cancellationToken) => Results.Ok(await service.SetStatusAsync(id, request, cancellationToken));
}
