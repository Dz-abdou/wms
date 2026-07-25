using Warehouse.Api.Auth;
using Warehouse.Application.Purchasing;

namespace Warehouse.Api.Endpoints.Purchasing;

public static class PurchasingReferenceEndpoints
{
    public static IEndpointRouteBuilder MapPurchasingReferenceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/purchasing/currencies", (ICurrencyCatalogue currencyCatalogue) =>
                Results.Ok(currencyCatalogue.GetOptions()))
            .WithTags("Purchasing")
            .RequireAuthorization(AuthorizationPolicies.ReadPurchasing)
            .AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ReadPurchasing));

        return endpoints;
    }
}
