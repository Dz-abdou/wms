using FluentValidation;
using Warehouse.Api.Auth;
using Warehouse.Api.Endpoints;
using Warehouse.Application.Inventory;

namespace Warehouse.Api.Endpoints.Inventory;

public static class InventoryEndpoints
{
    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(InventoryApiRoutes.BasePath)
            .WithTags("Inventory")
            .AddEndpointFilter<InventoryExceptionEndpointFilter>();

        group.MapGet(InventoryApiRoutes.MovementHistoryPath, GetMovementHistoryAsync)
            .RequireAuthorization(AuthorizationPolicies.ReadInventory)
            .AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ReadInventory));
        group.MapGet(InventoryApiRoutes.OverviewPath, GetOverviewAsync)
            .RequireAuthorization(AuthorizationPolicies.ReadInventory)
            .AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ReadInventory));
        group.MapGet(InventoryApiRoutes.AdjustmentPath, GetAdjustmentsAsync)
            .RequireAuthorization(AuthorizationPolicies.ReadInventory)
            .AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ReadInventory));
        group.MapGet(InventoryApiRoutes.AdjustmentByIdPath, GetAdjustmentByIdAsync)
            .RequireAuthorization(AuthorizationPolicies.ReadInventory)
            .AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ReadInventory));
        group.MapPost(InventoryApiRoutes.AdjustmentPath, AdjustAsync)
            .RequireAuthorization(AuthorizationPolicies.AdjustInventory)
            .AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.AdjustInventory));

        return endpoints;
    }

    private static async Task<IResult> GetMovementHistoryAsync(
        [AsParameters] InventoryMovementListQuery query,
        IValidator<InventoryMovementListQuery> validator,
        InventoryService inventoryService,
        CancellationToken cancellationToken)
    {
        var validationProblem = await validator.ValidateRequestAsync(query, cancellationToken);
        return validationProblem ?? Results.Ok(await inventoryService.GetMovementHistoryAsync(query, cancellationToken));
    }

    private static async Task<IResult> GetOverviewAsync(
        [AsParameters] InventoryOverviewQuery query,
        IValidator<InventoryOverviewQuery> validator,
        InventoryService inventoryService,
        CancellationToken cancellationToken)
    {
        var validationProblem = await validator.ValidateRequestAsync(query, cancellationToken);
        return validationProblem ?? Results.Ok(await inventoryService.GetOverviewAsync(query, cancellationToken));
    }

    private static async Task<IResult> AdjustAsync(
        InventoryAdjustmentInput input,
        IValidator<InventoryAdjustmentInput> validator,
        InventoryService inventoryService,
        CancellationToken cancellationToken)
    {
        var validationProblem = await validator.ValidateRequestAsync(input, cancellationToken);
        return validationProblem ?? Results.Ok(await inventoryService.AdjustAsync(input, cancellationToken));
    }

    private static async Task<IResult> GetAdjustmentsAsync(
        [AsParameters] InventoryAdjustmentListQuery query,
        IValidator<InventoryAdjustmentListQuery> validator,
        InventoryService inventoryService,
        CancellationToken cancellationToken)
    {
        var validationProblem = await validator.ValidateRequestAsync(query, cancellationToken);
        return validationProblem ?? Results.Ok(await inventoryService.GetAdjustmentsAsync(query, cancellationToken));
    }

    private static async Task<IResult> GetAdjustmentByIdAsync(
        Guid id,
        InventoryService inventoryService,
        CancellationToken cancellationToken) =>
        Results.Ok(await inventoryService.GetAdjustmentByIdAsync(id, cancellationToken));
}
