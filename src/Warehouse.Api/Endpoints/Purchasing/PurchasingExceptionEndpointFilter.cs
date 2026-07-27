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
            return ValidationProblem(
                "PurchaseUnitOfMeasure",
                exception.Message,
                ApiErrorCodes.SupplierProductConflict,
                "A supplier catalogue item already exists for this supplier, product, and purchase unit.",
                StatusCodes.Status409Conflict);
        }
        catch (SupplierProductCurrencyNotSupportedException exception)
        {
            return ValidationProblem(
                "CurrencyCode",
                exception.Message,
                ApiErrorCodes.SupplierProductCurrencyNotSupported,
                "Supplier catalogue currency is not supported.");
        }
        catch (SupplierProductFieldValidationException exception)
        {
            return ValidationProblem(
                exception.PropertyName,
                exception.Message,
                exception.ErrorCode,
                "Supplier catalogue data is invalid.");
        }
        catch (PurchaseOrderNotFoundException exception)
        {
            return Problem(StatusCodes.Status404NotFound, "Purchase order not found.", exception.Message, ApiErrorCodes.PurchaseOrderNotFound);
        }
        catch (PurchaseOrderImmutableException exception)
        {
            return Problem(StatusCodes.Status409Conflict, "Purchase order cannot be changed.", exception.Message, ApiErrorCodes.PurchaseOrderImmutable);
        }
        catch (PurchaseOrderConcurrencyException exception)
        {
            return Problem(StatusCodes.Status409Conflict, "Purchase order was updated.", exception.Message, ApiErrorCodes.PurchaseOrderConcurrencyConflict);
        }
        catch (PurchaseOrderInvalidTransitionException exception)
        {
            return Problem(StatusCodes.Status409Conflict, "Purchase order status transition is invalid.", exception.Message, ApiErrorCodes.PurchaseOrderInvalidTransition);
        }
        catch (PurchaseOrderMinimumOrderQuantityException exception)
        {
            return ValidationProblem(
                exception.PropertyName,
                exception.Message,
                ApiErrorCodes.PurchaseOrderMinimumOrderQuantity);
        }
        catch (PurchaseOrderFieldValidationException exception)
        {
            return ValidationProblem(
                exception.PropertyName,
                exception.Message,
                exception.ErrorCode,
                "Purchase order data is invalid.");
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

    private static IResult ValidationProblem(
        string propertyName,
        string message,
        string errorCode,
        string title = "Purchase order minimum order quantity is not met.",
        int statusCode = StatusCodes.Status422UnprocessableEntity) => Results.ValidationProblem(
        errors: new Dictionary<string, string[]> { [propertyName] = [message] },
        statusCode: statusCode,
        title: title,
        extensions: new Dictionary<string, object?>
        {
            ["code"] = errorCode,
            ["errorCodes"] = new Dictionary<string, string[]> { [propertyName] = [errorCode] }
        });
}
