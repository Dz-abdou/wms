using FluentValidation;
using Warehouse.Api.Auth;
using Warehouse.Api.Endpoints;
using Warehouse.Application.Purchasing;

namespace Warehouse.Api.Endpoints.Purchasing;

public static class PurchaseOrderEndpoints
{
    public static IEndpointRouteBuilder MapPurchaseOrderEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(PurchasingApiRoutes.PurchaseOrdersBasePath).WithTags("Purchase orders").AddEndpointFilter<PurchasingExceptionEndpointFilter>();
        group.MapGet("", GetListAsync).RequireAuthorization(AuthorizationPolicies.ReadPurchasing).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ReadPurchasing));
        group.MapGet("/{id:guid}", GetByIdAsync).WithName(PurchasingApiRoutes.PurchaseOrderByIdRouteName).RequireAuthorization(AuthorizationPolicies.ReadPurchasing).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ReadPurchasing));
        group.MapPost("", CreateAsync).RequireAuthorization(AuthorizationPolicies.ManagePurchasing).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ManagePurchasing));
        group.MapPut("/{id:guid}", UpdateAsync).RequireAuthorization(AuthorizationPolicies.ManagePurchasing).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ManagePurchasing));
        group.MapPatch("/{id:guid}/submit", SubmitAsync).RequireAuthorization(AuthorizationPolicies.ManagePurchasing).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ManagePurchasing));
        return endpoints;
    }

    private static async Task<IResult> GetListAsync([AsParameters] PurchaseOrderListQuery query, IValidator<PurchaseOrderListQuery> validator, PurchaseOrderService service, CancellationToken cancellationToken)
    {
        var problem = await validator.ValidateRequestAsync(query, cancellationToken);
        return problem ?? Results.Ok(await service.GetListAsync(query, cancellationToken));
    }

    private static async Task<IResult> GetByIdAsync(Guid id, PurchaseOrderService service, CancellationToken cancellationToken) => Results.Ok(await service.GetByIdAsync(id, cancellationToken));

    private static async Task<IResult> CreateAsync(PurchaseOrderInput input, IValidator<PurchaseOrderInput> validator, PurchaseOrderService service, CancellationToken cancellationToken)
    {
        var problem = await validator.ValidateRequestAsync(input, cancellationToken);
        if (problem is not null) return problem;
        var purchaseOrder = await service.CreateAsync(input, cancellationToken);
        return Results.CreatedAtRoute(PurchasingApiRoutes.PurchaseOrderByIdRouteName, new { id = purchaseOrder.Id }, purchaseOrder);
    }

    private static async Task<IResult> UpdateAsync(Guid id, PurchaseOrderInput input, IValidator<PurchaseOrderInput> validator, PurchaseOrderService service, CancellationToken cancellationToken)
    {
        var problem = await validator.ValidateRequestAsync(input, cancellationToken);
        return problem ?? Results.Ok(await service.UpdateAsync(id, input, cancellationToken));
    }

    private static async Task<IResult> SubmitAsync(Guid id, PurchaseOrderService service, CancellationToken cancellationToken) => Results.Ok(await service.SubmitAsync(id, cancellationToken));
}
