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
        catch (InventoryInvalidUnitOfMeasureException exception)
        {
            return Problem(StatusCodes.Status400BadRequest, "Invalid unit of measure.", exception.Message, ApiErrorCodes.InventoryInvalidUnitOfMeasure);
        }

        catch (InventoryWarehouseNotFoundException exception)
        {
            return Problem(StatusCodes.Status404NotFound, "Warehouse not found.", exception.Message, ApiErrorCodes.InventoryWarehouseNotFound);
        }
        catch (InsufficientInventoryException exception)
        {
            return Results.ValidationProblem(
                errors: new Dictionary<string, string[]> { [exception.PropertyName] = [exception.Message] },
                statusCode: StatusCodes.Status422UnprocessableEntity,
                title: "Inventory adjustment data is invalid.",
                extensions: new Dictionary<string, object?>
                {
                    ["code"] = ApiErrorCodes.InventoryInsufficientStock,
                    ["errorCodes"] = new Dictionary<string, string[]> { [exception.PropertyName] = [ApiErrorCodes.InventoryInsufficientStock] },
                    ["errorParameters"] = new Dictionary<string, object?[]>
                    {
                        [exception.PropertyName] =
                        [new { exception.AvailableQuantity, exception.BaseUnitOfMeasure, exception.Warehouse }]
                    }
                });
        }
        catch (InventoryConcurrencyException exception)
        {
            return Problem(StatusCodes.Status409Conflict, "Inventory changed.", exception.Message, ApiErrorCodes.InventoryConcurrencyConflict);
        }
        catch (InventoryAdjustmentNotFoundException exception)
        {
            return Problem(StatusCodes.Status404NotFound, "Inventory adjustment not found.", exception.Message, ApiErrorCodes.InventoryAdjustmentNotFound);
        }
        catch (CycleCountNotFoundException exception)
        {
            return Problem(StatusCodes.Status404NotFound, "Cycle count not found.", exception.Message, ApiErrorCodes.InventoryCycleCountNotFound);
        }
        catch (InventoryTransferNotFoundException exception)
        {
            return Problem(StatusCodes.Status404NotFound, "Inventory transfer not found.", exception.Message, ApiErrorCodes.InventoryTransferNotFound);
        }
        catch (CycleCountStaleBalanceException exception)
        {
            return Results.ValidationProblem(
                errors: new Dictionary<string, string[]> { [exception.PropertyName] = [exception.Message] },
                statusCode: StatusCodes.Status422UnprocessableEntity,
                title: "Cycle count data is invalid.",
                extensions: new Dictionary<string, object?>
                {
                    ["code"] = ApiErrorCodes.InventoryCycleCountStaleBalance,
                    ["errorCodes"] = new Dictionary<string, string[]> { [exception.PropertyName] = [ApiErrorCodes.InventoryCycleCountStaleBalance] },
                    ["errorParameters"] = new Dictionary<string, object?[]>
                    {
                        [exception.PropertyName] = [new { exception.CurrentQuantityInBase, exception.BaseUnitOfMeasure }]
                    }
                });
        }
    }

    private static IResult Problem(int statusCode, string title, string detail, string code) => Results.Problem(
        statusCode: statusCode,
        title: title,
        detail: detail,
        extensions: new Dictionary<string, object?> { ["code"] = code });
}
