using Warehouse.Application.Common.Errors;
using Warehouse.Application.Products;
using Warehouse.Application.Purchasing;
using Warehouse.Application.Suppliers;

namespace Warehouse.Api.Endpoints.Purchasing;

public sealed class PurchasingExceptionEndpointFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        try
        {
            return await next(context);
        }
        catch (SupplierNotFoundException exception)
        {
            return Problem(StatusCodes.Status404NotFound, "Supplier not found.", exception.Message, ApiErrorCodes.SupplierNotFound);
        }
        catch (ProductNotFoundException exception)
        {
            return Problem(StatusCodes.Status404NotFound, "Product not found.", exception.Message, ApiErrorCodes.ProductNotFound);
        }
        catch (SupplierProductNotFoundException exception)
        {
            return Problem(StatusCodes.Status404NotFound, "Supplier catalogue item not found.", exception.Message, ApiErrorCodes.SupplierProductNotFound);
        }
        catch (SupplierProductConflictException exception)
        {
            return Problem(StatusCodes.Status409Conflict, "Supplier catalogue item already exists.", exception.Message, ApiErrorCodes.SupplierProductConflict);
        }
        catch (SupplierProductCurrencyNotSupportedException exception)
        {
            return Problem(StatusCodes.Status422UnprocessableEntity, "Supplier catalogue currency is not supported.", exception.Message, ApiErrorCodes.SupplierProductCurrencyNotSupported);
        }
        catch (PurchaseOrderNotFoundException exception)
        {
            return Problem(StatusCodes.Status404NotFound, "Purchase order not found.", exception.Message, ApiErrorCodes.PurchaseOrderNotFound);
        }
        catch (PurchaseOrderImmutableException exception)
        {
            return Problem(StatusCodes.Status409Conflict, "Purchase order cannot be changed.", exception.Message, ApiErrorCodes.PurchaseOrderImmutable);
        }
        catch (PurchaseOrderMinimumOrderQuantityException exception)
        {
            return ValidationProblem(
                exception.PropertyName,
                exception.Message,
                ApiErrorCodes.PurchaseOrderMinimumOrderQuantity);
        }
        catch (PurchaseOrderCatalogueInvalidException exception)
        {
            return Problem(StatusCodes.Status422UnprocessableEntity, "Purchase order catalogue data is invalid.", exception.Message, ApiErrorCodes.PurchaseOrderCatalogueInvalid);
        }
        catch (PurchaseOrderSubmissionInvalidException exception)
        {
            return Problem(StatusCodes.Status422UnprocessableEntity, "Purchase order cannot be submitted.", exception.Message, ApiErrorCodes.PurchaseOrderSubmissionInvalid);
        }
    }

    private static IResult Problem(int statusCode, string title, string detail, string code) => Results.Problem(
        statusCode: statusCode,
        title: title,
        detail: detail,
        extensions: new Dictionary<string, object?> { ["code"] = code });

    private static IResult ValidationProblem(string propertyName, string message, string errorCode) => Results.ValidationProblem(
        errors: new Dictionary<string, string[]> { [propertyName] = [message] },
        statusCode: StatusCodes.Status422UnprocessableEntity,
        title: "Purchase order minimum order quantity is not met.",
        extensions: new Dictionary<string, object?>
        {
            ["code"] = errorCode,
            ["errorCodes"] = new Dictionary<string, string[]> { [propertyName] = [errorCode] }
        });
}
