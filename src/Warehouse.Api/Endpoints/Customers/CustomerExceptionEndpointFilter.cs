using Warehouse.Application.Common.Errors;
using Warehouse.Application.Customers;

namespace Warehouse.Api.Endpoints.Customers;

public sealed class CustomerExceptionEndpointFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        try
        {
            return await next(context);
        }
        catch (CustomerNotFoundException exception)
        {
            return Problem(StatusCodes.Status404NotFound, "Customer not found.", exception.Message, ApiErrorCodes.CustomerNotFound);
        }
        catch (CustomerContactNotFoundException exception)
        {
            return Problem(StatusCodes.Status404NotFound, "Customer contact not found.", exception.Message, ApiErrorCodes.CustomerContactNotFound);
        }
        catch (CustomerAddressNotFoundException exception)
        {
            return Problem(StatusCodes.Status404NotFound, "Customer address not found.", exception.Message, ApiErrorCodes.CustomerAddressNotFound);
        }
        catch (CustomerCodeConflictException exception)
        {
            return Problem(StatusCodes.Status409Conflict, "Customer code already exists.", exception.Message, ApiErrorCodes.CustomerCodeConflict);
        }
        catch (CustomerDefaultCurrencyNotSupportedException exception)
        {
            return Results.ValidationProblem(
                errors: new Dictionary<string, string[]> { ["DefaultCurrencyCode"] = [exception.Message] },
                statusCode: StatusCodes.Status422UnprocessableEntity,
                title: "Customer default currency is not available.",
                extensions: new Dictionary<string, object?>
                {
                    ["code"] = ApiErrorCodes.CustomerDefaultCurrencyNotSupported,
                    ["errorCodes"] = new Dictionary<string, string[]>
                    {
                        ["DefaultCurrencyCode"] = [ApiErrorCodes.CustomerDefaultCurrencyNotSupported]
                    }
                });
        }
    }

    private static IResult Problem(int statusCode, string title, string detail, string code) =>
        Results.Problem(statusCode: statusCode, title: title, detail: detail, extensions: new Dictionary<string, object?> { ["code"] = code });
}
