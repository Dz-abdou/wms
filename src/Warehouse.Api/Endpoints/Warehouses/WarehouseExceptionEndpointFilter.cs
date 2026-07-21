using Microsoft.AspNetCore.Http;
using Warehouse.Application.Common.Errors;
using Warehouse.Application.Warehouses;

namespace Warehouse.Api.Endpoints.Warehouses;

public sealed class WarehouseExceptionEndpointFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        try
        {
            return await next(context);
        }
        catch (WarehouseNotFoundException exception)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Warehouse not found.",
                detail: exception.Message,
                extensions: new Dictionary<string, object?> { ["code"] = ApiErrorCodes.WarehouseNotFound });
        }
        catch (WarehouseCodeConflictException exception)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Warehouse code already exists.",
                detail: exception.Message,
                extensions: new Dictionary<string, object?> { ["code"] = ApiErrorCodes.WarehouseCodeConflict });
        }
    }
}