using FluentValidation;
using Warehouse.Api.Auth;
using Warehouse.Api.Endpoints;
using Warehouse.Application.Common.Errors;
using Warehouse.Application.Common.Numbering;
using Warehouse.Application.Sales;

namespace Warehouse.Api.Endpoints.Sales;

public static class SalesOrderEndpoints
{
    public static IEndpointRouteBuilder MapSalesOrderEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/sales-orders").WithTags("Sales orders").AddEndpointFilter<SalesOrderExceptionEndpointFilter>();
        group.MapGet("", GetListAsync).RequireAuthorization(AuthorizationPolicies.ReadPurchasing).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ReadPurchasing));
        group.MapGet("/{id:guid}", GetByIdAsync).WithName("GetSalesOrderById").RequireAuthorization(AuthorizationPolicies.ReadPurchasing).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ReadPurchasing));
        group.MapGet("/availability", GetAvailabilityAsync).RequireAuthorization(AuthorizationPolicies.ReadPurchasing).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ReadPurchasing));
        group.MapPost("", CreateAsync).RequireAuthorization(AuthorizationPolicies.ManagePurchasing).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ManagePurchasing));
        group.MapPut("/{id:guid}", UpdateAsync).RequireAuthorization(AuthorizationPolicies.ManagePurchasing).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ManagePurchasing));
        group.MapPatch("/{id:guid}/submit", SubmitAsync).RequireAuthorization(AuthorizationPolicies.ManagePurchasing).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ManagePurchasing));
        group.MapPatch("/{id:guid}/cancel", CancelAsync).RequireAuthorization(AuthorizationPolicies.ManagePurchasing).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ManagePurchasing));
        return endpoints;
    }
    private static async Task<IResult> GetListAsync([AsParameters] SalesOrderListQuery query, IValidator<SalesOrderListQuery> validator, SalesOrderService service, CancellationToken token) { var problem = await validator.ValidateRequestAsync(query, token); return problem ?? Results.Ok(await service.GetListAsync(query, token)); }
    private static async Task<IResult> GetByIdAsync(Guid id, SalesOrderService service, CancellationToken token) => Results.Ok(await service.GetByIdAsync(id, token));
    private static async Task<IResult> GetAvailabilityAsync([AsParameters] SalesOrderAvailabilityQuery query, SalesOrderService service, CancellationToken token) => Results.Ok(await service.GetAvailabilityAsync(query, token));
    private static async Task<IResult> CreateAsync(SalesOrderInput input, IValidator<SalesOrderInput> validator, SalesOrderService service, CancellationToken token) { var problem = await validator.ValidateRequestAsync(input, token); if (problem is not null) return problem; var order = await service.CreateAsync(input, token); return Results.CreatedAtRoute("GetSalesOrderById", new { id = order.Id }, order); }
    private static async Task<IResult> UpdateAsync(Guid id, SalesOrderInput input, IValidator<SalesOrderInput> validator, SalesOrderService service, CancellationToken token) { var problem = await validator.ValidateRequestAsync(input, token); return problem ?? Results.Ok(await service.UpdateAsync(id, input, token)); }
    private static async Task<IResult> SubmitAsync(Guid id, SalesOrderVersionInput input, IValidator<SalesOrderVersionInput> validator, SalesOrderService service, CancellationToken token) { var problem = await validator.ValidateRequestAsync(input, token); return problem ?? Results.Ok(await service.SubmitAsync(id, input, token)); }
    private static async Task<IResult> CancelAsync(Guid id, SalesOrderCancelInput input, IValidator<SalesOrderCancelInput> validator, SalesOrderService service, CancellationToken token) { var problem = await validator.ValidateRequestAsync(input, token); return problem ?? Results.Ok(await service.CancelAsync(id, input, token)); }
}

public sealed class SalesOrderExceptionEndpointFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        try { return await next(context); }
        catch (DocumentNumberDefinitionUnavailableException exception) { return Problem(422, "Document number is unavailable.", exception.Message, ApiErrorCodes.DocumentNumberDefinitionUnavailable); }
        catch (DocumentNumberCapacityExceededException exception) { return Problem(409, "Document number capacity is exhausted.", exception.Message, ApiErrorCodes.DocumentNumberCapacityExceeded); }
        catch (SalesOrderNotFoundException exception) { return Problem(404, "Sales order not found.", exception.Message, ApiErrorCodes.SalesOrderNotFound); }
        catch (SalesOrderConcurrencyException exception) { return Problem(409, "Sales order was updated.", exception.Message, ApiErrorCodes.SalesOrderConcurrencyConflict); }
        catch (SalesOrderImmutableException exception) { return Problem(409, "Sales order cannot be changed.", exception.Message, ApiErrorCodes.SalesOrderImmutable); }
        catch (SalesOrderInvalidTransitionException exception) { return Problem(409, "Sales order status transition is invalid.", exception.Message, ApiErrorCodes.SalesOrderInvalidTransition); }
        catch (SalesOrderFieldValidationException exception) { return Results.ValidationProblem(new Dictionary<string, string[]> { [exception.PropertyName] = [exception.Message] }, statusCode: 422, title: "Sales order data is invalid.", extensions: new Dictionary<string, object?> { ["code"] = exception.ErrorCode, ["errorCodes"] = new Dictionary<string, string[]> { [exception.PropertyName] = [exception.ErrorCode] } }); }
    }
    private static IResult Problem(int status, string title, string detail, string code) => Results.Problem(statusCode: status, title: title, detail: detail, extensions: new Dictionary<string, object?> { ["code"] = code });
}
