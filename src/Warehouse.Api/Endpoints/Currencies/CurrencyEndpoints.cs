using FluentValidation;
using Warehouse.Api.Auth;
using Warehouse.Api.Endpoints;
using Warehouse.Application.Currencies;

namespace Warehouse.Api.Endpoints.Currencies;

public static class CurrencyEndpoints
{
    private const string BasePath = "/api/currencies";
    public static IEndpointRouteBuilder MapCurrencyEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(BasePath).WithTags("Currencies").AddEndpointFilter<CurrencyExceptionEndpointFilter>();
        group.MapGet("", GetListAsync).RequireAuthorization(AuthorizationPolicies.ReadCatalog).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ReadCatalog));
        group.MapGet("/{id:guid}", GetByIdAsync).RequireAuthorization(AuthorizationPolicies.ReadCatalog).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ReadCatalog));
        group.MapPost("", CreateAsync).RequireAuthorization(AuthorizationPolicies.ManageCatalog).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ManageCatalog));
        group.MapPut("/{id:guid}", UpdateAsync).RequireAuthorization(AuthorizationPolicies.ManageCatalog).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ManageCatalog));
        group.MapPatch("/{id:guid}/status", SetStatusAsync).RequireAuthorization(AuthorizationPolicies.ManageCatalog).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ManageCatalog));
        group.MapPatch("/{id:guid}/default", SetDefaultAsync).RequireAuthorization(AuthorizationPolicies.ManageCatalog).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ManageCatalog));
        return endpoints;
    }
    private static async Task<IResult> GetListAsync([AsParameters] CurrencyListQuery query, IValidator<CurrencyListQuery> validator, CurrencyService service, CancellationToken token) { var problem = await validator.ValidateRequestAsync(query, token); return problem ?? Results.Ok(await service.GetListAsync(query, token)); }
    private static async Task<IResult> GetByIdAsync(Guid id, CurrencyService service, CancellationToken token) => Results.Ok(await service.GetByIdAsync(id, token));
    private static async Task<IResult> CreateAsync(CurrencyInput input, IValidator<CurrencyInput> validator, CurrencyService service, CancellationToken token) { var problem = await validator.ValidateRequestAsync(input, token); return problem ?? Results.Created(BasePath, await service.CreateAsync(input, token)); }
    private static async Task<IResult> UpdateAsync(Guid id, UpdateCurrencyInput input, IValidator<UpdateCurrencyInput> validator, CurrencyService service, CancellationToken token) { var problem = await validator.ValidateRequestAsync(input, token); return problem ?? Results.Ok(await service.UpdateAsync(id, input, token)); }
    private static async Task<IResult> SetStatusAsync(Guid id, SetCurrencyStatusRequest request, CurrencyService service, CancellationToken token) => Results.Ok(await service.SetStatusAsync(id, request, token));
    private static async Task<IResult> SetDefaultAsync(Guid id, CurrencyService service, CancellationToken token) => Results.Ok(await service.SetDefaultAsync(id, token));
}
