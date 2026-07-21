using FluentValidation;
using Microsoft.AspNetCore.Http;
using Warehouse.Api.Auth;
using Warehouse.Api.Endpoints;
using Warehouse.Application.Warehouses;

namespace Warehouse.Api.Endpoints.Warehouses;

public static class WarehouseEndpoints
{
    public static IEndpointRouteBuilder MapWarehouseEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(WarehouseApiRoutes.BasePath)
            .WithTags("Warehouses")
            .AddEndpointFilter<WarehouseExceptionEndpointFilter>();

        group.MapGet("", GetListAsync).RequireAuthorization(AuthorizationPolicies.ReadCatalog).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ReadCatalog));
        group.MapGet("/{id:guid}", GetByIdAsync).WithName(WarehouseApiRoutes.GetByIdRouteName).RequireAuthorization(AuthorizationPolicies.ReadCatalog).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ReadCatalog));
        group.MapPost("", CreateAsync).RequireAuthorization(AuthorizationPolicies.ManageCatalog).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ManageCatalog));
        group.MapPut("/{id:guid}", UpdateAsync).RequireAuthorization(AuthorizationPolicies.ManageCatalog).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ManageCatalog));
        group.MapPatch("/{id:guid}/status", SetStatusAsync).RequireAuthorization(AuthorizationPolicies.ManageCatalog).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ManageCatalog));

        return endpoints;
    }

    private static async Task<IResult> GetListAsync(
        [AsParameters] WarehouseListQuery query,
        IValidator<WarehouseListQuery> validator,
        WarehouseService warehouseService,
        CancellationToken cancellationToken)
    {
        var validationProblem = await validator.ValidateRequestAsync(query, cancellationToken);
        return validationProblem ?? Results.Ok(await warehouseService.GetListAsync(query, cancellationToken));
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        WarehouseService warehouseService,
        CancellationToken cancellationToken) =>
        Results.Ok(await warehouseService.GetByIdAsync(id, cancellationToken));

    private static async Task<IResult> CreateAsync(
        WarehouseInput input,
        IValidator<WarehouseInput> validator,
        WarehouseService warehouseService,
        CancellationToken cancellationToken)
    {
        var validationProblem = await validator.ValidateRequestAsync(input, cancellationToken);
        if (validationProblem is not null)
        {
            return validationProblem;
        }

        var warehouse = await warehouseService.CreateAsync(input, cancellationToken);
        return Results.CreatedAtRoute(WarehouseApiRoutes.GetByIdRouteName, new { id = warehouse.Id }, warehouse);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        WarehouseInput input,
        IValidator<WarehouseInput> validator,
        WarehouseService warehouseService,
        CancellationToken cancellationToken)
    {
        var validationProblem = await validator.ValidateRequestAsync(input, cancellationToken);
        if (validationProblem is not null)
        {
            return validationProblem;
        }

        return Results.Ok(await warehouseService.UpdateAsync(id, input, cancellationToken));
    }

    private static async Task<IResult> SetStatusAsync(
        Guid id,
        SetWarehouseStatusRequest request,
        WarehouseService warehouseService,
        CancellationToken cancellationToken) =>
        Results.Ok(await warehouseService.SetStatusAsync(id, request, cancellationToken));
}