using Warehouse.Application.Common.Errors;
using Warehouse.Application.Suppliers;

namespace Warehouse.Api.Endpoints.Suppliers;

public sealed class SupplierExceptionEndpointFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        try { return await next(context); }
        catch (SupplierNotFoundException exception)
        {
            return Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Supplier not found.", detail: exception.Message, extensions: new Dictionary<string, object?> { ["code"] = ApiErrorCodes.SupplierNotFound });
        }
        catch (SupplierCodeConflictException exception)
        {
            return Results.Problem(statusCode: StatusCodes.Status409Conflict, title: "Supplier code already exists.", detail: exception.Message, extensions: new Dictionary<string, object?> { ["code"] = ApiErrorCodes.SupplierCodeConflict });
        }
    }
}
