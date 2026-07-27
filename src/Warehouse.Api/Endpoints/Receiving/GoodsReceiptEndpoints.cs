using Warehouse.Api.Auth;
using Warehouse.Application.Common.Errors;
using Warehouse.Application.Receiving;

namespace Warehouse.Api.Endpoints.Receiving;

public static class GoodsReceiptEndpoints
{
    public static IEndpointRouteBuilder MapGoodsReceiptEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/goods-receipts").WithTags("Goods receipts").AddEndpointFilter<GoodsReceiptExceptionEndpointFilter>();
        group.MapPost("", CreateAsync).RequireAuthorization(AuthorizationPolicies.AdjustInventory).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.AdjustInventory));
        return endpoints;
    }
    private static async Task<IResult> CreateAsync(GoodsReceiptInput input, GoodsReceiptService service, CancellationToken cancellationToken)
    {
        var receipt = await service.CreateAsync(input, cancellationToken);
        return Results.Created($"/api/goods-receipts/{receipt.Id}", receipt);
    }
}

public sealed class GoodsReceiptExceptionEndpointFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        try { return await next(context); }
        catch (GoodsReceiptPurchaseOrderUnavailableException exception) { return Problem(422, "Purchase order cannot be received.", exception.Message, ApiErrorCodes.GoodsReceiptPurchaseOrderUnavailable); }
        catch (GoodsReceiptConcurrencyException exception) { return Problem(409, "Purchase order was updated.", exception.Message, ApiErrorCodes.GoodsReceiptPurchaseOrderConcurrencyConflict); }
        catch (GoodsReceiptOverReceiptException exception)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]> { [exception.PropertyName] = [exception.Message] }, statusCode: 422, extensions: new Dictionary<string, object?> { ["code"] = ApiErrorCodes.GoodsReceiptOverReceipt, ["errorCodes"] = new Dictionary<string, string[]> { [exception.PropertyName] = [ApiErrorCodes.GoodsReceiptOverReceipt] } });
        }
    }
    private static IResult Problem(int status, string title, string detail, string code) => Results.Problem(statusCode: status, title: title, detail: detail, extensions: new Dictionary<string, object?> { ["code"] = code });
}
