using Microsoft.AspNetCore.Http;
using Warehouse.Application.Common.Errors;
using Warehouse.Application.Inventory;

namespace Warehouse.Api.Endpoints.Inventory;

public sealed class InventoryExceptionEndpointFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        try
        {
            return await next(context);
        }
        catch (InventoryProductNotFoundException exception)
        {
            return Problem(StatusCodes.Status404NotFound, "Product not found.", exception.Message, ApiErrorCodes.InventoryProductNotFound);
        }
        catch (InventoryWarehouseNotFoundException exception)
        {
            return Problem(StatusCodes.Status404NotFound, "Warehouse not found.", exception.Message, ApiErrorCodes.InventoryWarehouseNotFound);
        }
        catch (InsufficientInventoryException exception)
        {
            return Problem(StatusCodes.Status409Conflict, "Insufficient stock.", exception.Message, ApiErrorCodes.InventoryInsufficientStock);
        }
        catch (InventoryConcurrencyException exception)
        {
            return Problem(StatusCodes.Status409Conflict, "Inventory changed.", exception.Message, ApiErrorCodes.InventoryConcurrencyConflict);
        }
    }

    private static IResult Problem(int statusCode, string title, string detail, string code) => Results.Problem(
        statusCode: statusCode,
        title: title,
        detail: detail,
        extensions: new Dictionary<string, object?> { ["code"] = code });
}
