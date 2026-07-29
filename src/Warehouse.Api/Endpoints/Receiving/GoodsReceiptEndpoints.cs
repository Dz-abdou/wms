using FluentValidation;
using Warehouse.Api.Auth;
using Warehouse.Api.Endpoints;
using Warehouse.Application.Common.Errors;
using Warehouse.Application.Receiving;

namespace Warehouse.Api.Endpoints.Receiving;

public static class GoodsReceiptEndpoints
{
    public static IEndpointRouteBuilder MapGoodsReceiptEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/goods-receipts").WithTags("Goods receipts").AddEndpointFilter<GoodsReceiptExceptionEndpointFilter>();
        endpoints.MapGet("/api/purchase-orders/{id:guid}/receipt-candidate", GetCandidateAsync).WithTags("Goods receipts").AddEndpointFilter<GoodsReceiptExceptionEndpointFilter>().RequireAuthorization(AuthorizationPolicies.ReadPurchasing).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ReadPurchasing));
        group.MapGet("", GetListAsync).RequireAuthorization(AuthorizationPolicies.ReadPurchasing).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ReadPurchasing));
        group.MapGet("/{id:guid}", GetByIdAsync).RequireAuthorization(AuthorizationPolicies.ReadPurchasing).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ReadPurchasing));
        group.MapPost("", CreateAsync).RequireAuthorization(AuthorizationPolicies.AdjustInventory).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.AdjustInventory));
        return endpoints;
    }
    private static async Task<IResult> GetCandidateAsync(Guid id, GoodsReceiptService service, CancellationToken cancellationToken) => Results.Ok(await service.GetCandidateAsync(id, cancellationToken));
    private static async Task<IResult> GetListAsync([AsParameters] GoodsReceiptListQuery query, IValidator<GoodsReceiptListQuery> validator, GoodsReceiptService service, CancellationToken cancellationToken)
    {
        var problem = await validator.ValidateRequestAsync(query, cancellationToken);
        return problem ?? Results.Ok(await service.GetListAsync(query, cancellationToken));
    }
    private static async Task<IResult> GetByIdAsync(Guid id, GoodsReceiptService service, CancellationToken cancellationToken) => Results.Ok(await service.GetByIdAsync(id, cancellationToken));
    private static async Task<IResult> CreateAsync(GoodsReceiptInput input, IValidator<GoodsReceiptInput> validator, GoodsReceiptService service, CancellationToken cancellationToken)
    {
        var problem = await validator.ValidateRequestAsync(input, cancellationToken);
        if (problem is not null) return problem;
        var receipt = await service.CreateAsync(input, cancellationToken);
        return Results.Created($"/api/goods-receipts/{receipt.Id}", receipt);
    }
}

public sealed class GoodsReceiptExceptionEndpointFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        try { return await next(context); }
        catch (GoodsReceiptNotFoundException exception) { return Problem(404, "Goods receipt not found.", exception.Message, ApiErrorCodes.GoodsReceiptNotFound); }
        catch (GoodsReceiptPurchaseOrderUnavailableException exception) { return Problem(422, "Purchase order cannot be received.", exception.Message, ApiErrorCodes.GoodsReceiptPurchaseOrderUnavailable); }
        catch (GoodsReceiptConcurrencyException exception) { return Problem(409, "Purchase order was updated.", exception.Message, ApiErrorCodes.GoodsReceiptPurchaseOrderConcurrencyConflict); }
        catch (GoodsReceiptOverReceiptException exception)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]> { [exception.PropertyName] = [exception.Message] }, statusCode: 422, extensions: new Dictionary<string, object?> { ["code"] = ApiErrorCodes.GoodsReceiptOverReceipt, ["errorCodes"] = new Dictionary<string, string[]> { [exception.PropertyName] = [ApiErrorCodes.GoodsReceiptOverReceipt] } });
        }
        catch (GoodsReceiptPurchaseOrderLineUnavailableException exception)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]> { [exception.PropertyName] = [exception.Message] }, statusCode: 422, extensions: new Dictionary<string, object?> { ["code"] = ApiErrorCodes.GoodsReceiptPurchaseOrderLineUnavailable, ["errorCodes"] = new Dictionary<string, string[]> { [exception.PropertyName] = [ApiErrorCodes.GoodsReceiptPurchaseOrderLineUnavailable] } });
        }
    }
    private static IResult Problem(int status, string title, string detail, string code) => Results.Problem(statusCode: status, title: title, detail: detail, extensions: new Dictionary<string, object?> { ["code"] = code });
}
