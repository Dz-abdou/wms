using FluentValidation;
using Warehouse.Api.Auth;
using Warehouse.Api.Endpoints;
using Warehouse.Application.Customers;

namespace Warehouse.Api.Endpoints.Customers;

public static class CustomerEndpoints
{
    public static IEndpointRouteBuilder MapCustomerEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(CustomerApiRoutes.BasePath)
            .WithTags("Customers")
            .AddEndpointFilter<CustomerExceptionEndpointFilter>();

        group.MapGet("", GetListAsync).RequireAuthorization(AuthorizationPolicies.ReadCatalog).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ReadCatalog));
        group.MapGet("/{id:guid}", GetByIdAsync).WithName(CustomerApiRoutes.GetByIdRouteName).RequireAuthorization(AuthorizationPolicies.ReadCatalog).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ReadCatalog));
        group.MapPost("", CreateAsync).RequireAuthorization(AuthorizationPolicies.ManageCatalog).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ManageCatalog));
        group.MapPut("/{id:guid}", UpdateAsync).RequireAuthorization(AuthorizationPolicies.ManageCatalog).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ManageCatalog));
        group.MapPatch("/{id:guid}/status", SetStatusAsync).RequireAuthorization(AuthorizationPolicies.ManageCatalog).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ManageCatalog));
        group.MapPost("/{id:guid}/contacts", CreateContactAsync).RequireAuthorization(AuthorizationPolicies.ManageCatalog).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ManageCatalog));
        group.MapPut("/{id:guid}/contacts/{contactId:guid}", UpdateContactAsync).RequireAuthorization(AuthorizationPolicies.ManageCatalog).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ManageCatalog));
        group.MapDelete("/{id:guid}/contacts/{contactId:guid}", DeleteContactAsync).RequireAuthorization(AuthorizationPolicies.ManageCatalog).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ManageCatalog));
        group.MapPost("/{id:guid}/addresses", CreateAddressAsync).RequireAuthorization(AuthorizationPolicies.ManageCatalog).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ManageCatalog));
        group.MapPut("/{id:guid}/addresses/{addressId:guid}", UpdateAddressAsync).RequireAuthorization(AuthorizationPolicies.ManageCatalog).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ManageCatalog));
        group.MapDelete("/{id:guid}/addresses/{addressId:guid}", DeleteAddressAsync).RequireAuthorization(AuthorizationPolicies.ManageCatalog).AddEndpointFilter(new CatalogAuthorizationEndpointFilter(AuthorizationPolicies.ManageCatalog));
        return endpoints;
    }

    private static async Task<IResult> GetListAsync([AsParameters] CustomerListQuery query, IValidator<CustomerListQuery> validator, CustomerService service, CancellationToken cancellationToken)
    {
        var problem = await validator.ValidateRequestAsync(query, cancellationToken);
        return problem ?? Results.Ok(await service.GetListAsync(query, cancellationToken));
    }

    private static async Task<IResult> GetByIdAsync(Guid id, CustomerService service, CancellationToken cancellationToken) => Results.Ok(await service.GetByIdAsync(id, cancellationToken));

    private static async Task<IResult> CreateAsync(CustomerInput input, IValidator<CustomerInput> validator, CustomerService service, CancellationToken cancellationToken)
    {
        var problem = await validator.ValidateRequestAsync(input, cancellationToken);
        if (problem is not null) return problem;
        var customer = await service.CreateAsync(input, cancellationToken);
        return Results.CreatedAtRoute(CustomerApiRoutes.GetByIdRouteName, new { id = customer.Id }, customer);
    }

    private static async Task<IResult> UpdateAsync(Guid id, CustomerInput input, IValidator<CustomerInput> validator, CustomerService service, CancellationToken cancellationToken)
    {
        var problem = await validator.ValidateRequestAsync(input, cancellationToken);
        return problem ?? Results.Ok(await service.UpdateAsync(id, input, cancellationToken));
    }

    private static async Task<IResult> SetStatusAsync(Guid id, SetCustomerStatusRequest request, CustomerService service, CancellationToken cancellationToken) => Results.Ok(await service.SetStatusAsync(id, request, cancellationToken));

    private static async Task<IResult> CreateContactAsync(Guid id, CustomerContactInput input, IValidator<CustomerContactInput> validator, CustomerService service, CancellationToken cancellationToken)
    {
        var problem = await validator.ValidateRequestAsync(input, cancellationToken);
        return problem ?? Results.Ok(await service.CreateContactAsync(id, input, cancellationToken));
    }

    private static async Task<IResult> UpdateContactAsync(Guid id, Guid contactId, CustomerContactInput input, IValidator<CustomerContactInput> validator, CustomerService service, CancellationToken cancellationToken)
    {
        var problem = await validator.ValidateRequestAsync(input, cancellationToken);
        return problem ?? Results.Ok(await service.UpdateContactAsync(id, contactId, input, cancellationToken));
    }

    private static async Task<IResult> DeleteContactAsync(Guid id, Guid contactId, CustomerService service, CancellationToken cancellationToken)
    {
        await service.DeleteContactAsync(id, contactId, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> CreateAddressAsync(Guid id, CustomerAddressInput input, IValidator<CustomerAddressInput> validator, CustomerService service, CancellationToken cancellationToken)
    {
        var problem = await validator.ValidateRequestAsync(input, cancellationToken);
        return problem ?? Results.Ok(await service.CreateAddressAsync(id, input, cancellationToken));
    }

    private static async Task<IResult> UpdateAddressAsync(Guid id, Guid addressId, CustomerAddressInput input, IValidator<CustomerAddressInput> validator, CustomerService service, CancellationToken cancellationToken)
    {
        var problem = await validator.ValidateRequestAsync(input, cancellationToken);
        return problem ?? Results.Ok(await service.UpdateAddressAsync(id, addressId, input, cancellationToken));
    }

    private static async Task<IResult> DeleteAddressAsync(Guid id, Guid addressId, CustomerService service, CancellationToken cancellationToken)
    {
        await service.DeleteAddressAsync(id, addressId, cancellationToken);
        return Results.NoContent();
    }
}
