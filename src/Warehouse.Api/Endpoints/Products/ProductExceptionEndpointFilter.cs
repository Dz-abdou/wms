using Microsoft.AspNetCore.Http;
using Warehouse.Application.Common.Errors;
using Warehouse.Application.Products;

namespace Warehouse.Api.Endpoints.Products;

public sealed class ProductExceptionEndpointFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        try
        {
            return await next(context);
        }
        catch (ProductNotFoundException exception)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Product not found.",
                detail: exception.Message,
                extensions: new Dictionary<string, object?> { ["code"] = ApiErrorCodes.ProductNotFound });
        }
        catch (ProductSkuConflictException exception)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "SKU already exists.",
                detail: exception.Message,
                extensions: new Dictionary<string, object?> { ["code"] = ApiErrorCodes.ProductSkuConflict });
        }
        catch (ProductCategoryNotFoundException exception)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Product category not found.",
                detail: exception.Message,
                extensions: new Dictionary<string, object?> { ["code"] = ApiErrorCodes.ProductCategoryNotFound });
        }

    }
}