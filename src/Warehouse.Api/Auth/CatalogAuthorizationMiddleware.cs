using Microsoft.AspNetCore.Authorization;

namespace Warehouse.Api.Auth;

public sealed class CatalogAuthorizationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IAuthorizationService authorization)
    {
        var policy = GetPolicy(context.Request);
        if (policy is not null && !(await authorization.AuthorizeAsync(context.User, null, policy)).Succeeded)
        {
            var authenticated = context.User.Identity?.IsAuthenticated == true;
            await Results.Problem(
                statusCode: authenticated ? StatusCodes.Status403Forbidden : StatusCodes.Status401Unauthorized,
                title: authenticated ? "You do not have permission to perform this action." : "Authentication required.",
                extensions: new Dictionary<string, object?> { ["code"] = authenticated ? AuthenticationErrorCodes.Forbidden : AuthenticationErrorCodes.Unauthenticated })
                .ExecuteAsync(context);
            return;
        }

        await next(context);
    }

    private static string? GetPolicy(HttpRequest request) => request.Path.StartsWithSegments("/api/admin") ? AuthorizationPolicies.ManageAdministration : request.Path.StartsWithSegments("/api/inventory") ? request.Method == HttpMethods.Get ? AuthorizationPolicies.ReadInventory : AuthorizationPolicies.AdjustInventory : request.Path.StartsWithSegments("/api/products") || request.Path.StartsWithSegments("/api/warehouses") ? request.Method == HttpMethods.Get ? AuthorizationPolicies.ReadCatalog : AuthorizationPolicies.ManageCatalog : null;
}
