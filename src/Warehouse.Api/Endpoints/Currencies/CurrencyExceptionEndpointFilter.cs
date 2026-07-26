using Warehouse.Application.Common.Errors;
using Warehouse.Application.Currencies;

namespace Warehouse.Api.Endpoints.Currencies;

public sealed class CurrencyExceptionEndpointFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        try { return await next(context); }
        catch (CurrencyNotFoundException exception) { return Problem(StatusCodes.Status404NotFound, exception.Message, ApiErrorCodes.CurrencyNotFound); }
        catch (CurrencyCodeConflictException exception) { return Problem(StatusCodes.Status409Conflict, exception.Message, ApiErrorCodes.CurrencyCodeConflict); }
        catch (DefaultCurrencyRequiredException exception) { return Problem(StatusCodes.Status422UnprocessableEntity, exception.Message, ApiErrorCodes.CurrencyDefaultRequired); }
        catch (InactiveCurrencyCannotBeDefaultException exception) { return Problem(StatusCodes.Status422UnprocessableEntity, exception.Message, ApiErrorCodes.CurrencyInactive); }
    }
    private static IResult Problem(int status, string detail, string code) => Results.Problem(statusCode: status, title: "Currency request cannot be completed.", detail: detail, extensions: new Dictionary<string, object?> { ["code"] = code });
}
