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
        group.MapGet(InventoryApiRoutes.CycleCountPath, GetCycleCountsAsync)
            .RequireAuthorization(AuthorizationPolicies.ReadInventory)
            .AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ReadInventory));
        group.MapGet(InventoryApiRoutes.CycleCountCandidatePath, GetCycleCountCandidateAsync)
            .RequireAuthorization(AuthorizationPolicies.AdjustInventory)
            .AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.AdjustInventory));
        group.MapGet(InventoryApiRoutes.CycleCountByIdPath, GetCycleCountByIdAsync)
            .RequireAuthorization(AuthorizationPolicies.ReadInventory)
            .AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ReadInventory));
        group.MapPost(InventoryApiRoutes.CycleCountPath, CreateCycleCountAsync)
            .RequireAuthorization(AuthorizationPolicies.AdjustInventory)
            .AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.AdjustInventory));
        group.MapGet(InventoryApiRoutes.TransferPath, GetTransfersAsync)
            .RequireAuthorization(AuthorizationPolicies.ReadInventory)
            .AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ReadInventory));
        group.MapGet(InventoryApiRoutes.TransferCandidatePath, GetTransferCandidateAsync)
            .RequireAuthorization(AuthorizationPolicies.AdjustInventory)
            .AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.AdjustInventory));
        group.MapGet(InventoryApiRoutes.TransferByIdPath, GetTransferByIdAsync)
            .RequireAuthorization(AuthorizationPolicies.ReadInventory)
            .AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ReadInventory));
        group.MapPost(InventoryApiRoutes.TransferPath, CreateTransferAsync)
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

    private static async Task<IResult> GetCycleCountsAsync(
        [AsParameters] CycleCountListQuery query,
        IValidator<CycleCountListQuery> validator,
        InventoryService inventoryService,
        CancellationToken cancellationToken)
    {
        var validationProblem = await validator.ValidateRequestAsync(query, cancellationToken);
        return validationProblem ?? Results.Ok(await inventoryService.GetCycleCountsAsync(query, cancellationToken));
    }

    private static async Task<IResult> GetCycleCountCandidateAsync(
        [AsParameters] CycleCountCandidateQuery query,
        IValidator<CycleCountCandidateQuery> validator,
        InventoryService inventoryService,
        CancellationToken cancellationToken)
    {
        var validationProblem = await validator.ValidateRequestAsync(query, cancellationToken);
        return validationProblem ?? Results.Ok(await inventoryService.GetCycleCountCandidateAsync(query, cancellationToken));
    }

    private static async Task<IResult> GetCycleCountByIdAsync(
        Guid id,
        InventoryService inventoryService,
        CancellationToken cancellationToken) =>
        Results.Ok(await inventoryService.GetCycleCountByIdAsync(id, cancellationToken));

    private static async Task<IResult> CreateCycleCountAsync(
        CycleCountInput input,
        IValidator<CycleCountInput> validator,
        InventoryService inventoryService,
        CancellationToken cancellationToken)
    {
        var validationProblem = await validator.ValidateRequestAsync(input, cancellationToken);
        if (validationProblem is not null) return validationProblem;
        var cycleCount = await inventoryService.CreateCycleCountAsync(input, cancellationToken);
        return Results.Created($"{InventoryApiRoutes.CycleCountPath}/{cycleCount.Id}", cycleCount);
    }

    private static async Task<IResult> GetTransfersAsync(
        [AsParameters] InventoryTransferListQuery query,
        IValidator<InventoryTransferListQuery> validator,
        InventoryService inventoryService,
        CancellationToken cancellationToken)
    {
        var validationProblem = await validator.ValidateRequestAsync(query, cancellationToken);
        return validationProblem ?? Results.Ok(await inventoryService.GetTransfersAsync(query, cancellationToken));
    }

    private static async Task<IResult> GetTransferCandidateAsync(
        [AsParameters] InventoryTransferCandidateQuery query,
        IValidator<InventoryTransferCandidateQuery> validator,
        InventoryService inventoryService,
        CancellationToken cancellationToken)
    {
        var validationProblem = await validator.ValidateRequestAsync(query, cancellationToken);
        return validationProblem ?? Results.Ok(await inventoryService.GetTransferCandidateAsync(query, cancellationToken));
    }

    private static async Task<IResult> GetTransferByIdAsync(
        Guid id,
        InventoryService inventoryService,
        CancellationToken cancellationToken) =>
        Results.Ok(await inventoryService.GetTransferByIdAsync(id, cancellationToken));

    private static async Task<IResult> CreateTransferAsync(
        InventoryTransferInput input,
        IValidator<InventoryTransferInput> validator,
        InventoryService inventoryService,
        CancellationToken cancellationToken)
    {
        var validationProblem = await validator.ValidateRequestAsync(input, cancellationToken);
        if (validationProblem is not null) return validationProblem;
        var transfer = await inventoryService.CreateTransferAsync(input, cancellationToken);
        return Results.Created($"{InventoryApiRoutes.TransferPath}/{transfer.Id}", transfer);
    }
}
