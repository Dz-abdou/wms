using FluentValidation;
using Microsoft.AspNetCore.Http;
using Warehouse.Api.Endpoints;
using Warehouse.Application.Products;

namespace Warehouse.Api.Endpoints.Products;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(ProductApiRoutes.BasePath)
            .WithTags("Products")
            .AddEndpointFilter<ProductExceptionEndpointFilter>();

        group.MapGet("", GetListAsync);
        group.MapGet("/{id:guid}", GetByIdAsync).WithName(ProductApiRoutes.GetByIdRouteName);
        group.MapPost("", CreateAsync);
        group.MapPut("/{id:guid}", UpdateAsync);
        group.MapPatch("/{id:guid}/status", SetStatusAsync);

        return endpoints;
    }

    private static async Task<IResult> GetListAsync(
        [AsParameters] ProductListQuery query,
        IValidator<ProductListQuery> validator,
        ProductService productService,
        CancellationToken cancellationToken)
    {
        var validationProblem = await validator.ValidateRequestAsync(query, cancellationToken);
        return validationProblem ?? Results.Ok(await productService.GetListAsync(query, cancellationToken));
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        ProductService productService,
        CancellationToken cancellationToken) =>
        Results.Ok(await productService.GetByIdAsync(id, cancellationToken));

    private static async Task<IResult> CreateAsync(
        ProductInput input,
        IValidator<ProductInput> validator,
        ProductService productService,
        CancellationToken cancellationToken)
    {
        var validationProblem = await validator.ValidateRequestAsync(input, cancellationToken);
        if (validationProblem is not null)
        {
            return validationProblem;
        }

        var product = await productService.CreateAsync(input, cancellationToken);
        return Results.CreatedAtRoute(ProductApiRoutes.GetByIdRouteName, new { id = product.Id }, product);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        ProductInput input,
        IValidator<ProductInput> validator,
        ProductService productService,
        CancellationToken cancellationToken)
    {
        var validationProblem = await validator.ValidateRequestAsync(input, cancellationToken);
        if (validationProblem is not null)
        {
            return validationProblem;
        }

        return Results.Ok(await productService.UpdateAsync(id, input, cancellationToken));
    }

    private static async Task<IResult> SetStatusAsync(
        Guid id,
        SetProductStatusRequest request,
        ProductService productService,
        CancellationToken cancellationToken) =>
        Results.Ok(await productService.SetStatusAsync(id, request, cancellationToken));
}