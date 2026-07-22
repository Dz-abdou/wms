using FluentValidation;
using Warehouse.Api.Auth;
using Warehouse.Api.Endpoints;
using Warehouse.Application.Products;

namespace Warehouse.Api.Endpoints.Products;

public static class ProductCategoryEndpoints
{
    private const string BasePath = "/api/product-categories";

    public static IEndpointRouteBuilder MapProductCategoryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(BasePath)
            .WithTags("Product categories")
            .AddEndpointFilter<ProductExceptionEndpointFilter>();

        group.MapGet("", GetListAsync)
            .RequireAuthorization(AuthorizationPolicies.ReadCatalog)
            .AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ReadCatalog));
        group.MapPost("", CreateAsync)
            .RequireAuthorization(AuthorizationPolicies.ManageCatalog)
            .AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ManageCatalog));

        return endpoints;
    }

    private static async Task<IResult> GetListAsync(
        [AsParameters] ProductCategoryListQuery query,
        IValidator<ProductCategoryListQuery> validator,
        ProductCategoryService categoryService,
        CancellationToken cancellationToken)
    {
        var validationProblem = await validator.ValidateRequestAsync(query, cancellationToken);
        return validationProblem ?? Results.Ok(await categoryService.GetListAsync(query, cancellationToken));
    }

    private static async Task<IResult> CreateAsync(
        ProductCategoryInput input,
        IValidator<ProductCategoryInput> validator,
        ProductCategoryService categoryService,
        CancellationToken cancellationToken)
    {
        var validationProblem = await validator.ValidateRequestAsync(input, cancellationToken);
        return validationProblem ?? Results.Created(
            $"{BasePath}",
            await categoryService.CreateAsync(input, cancellationToken));
    }
}

