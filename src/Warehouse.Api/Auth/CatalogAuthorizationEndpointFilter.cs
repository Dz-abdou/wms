using Microsoft.AspNetCore.Authorization;

namespace Warehouse.Api.Auth;

public sealed class CatalogAuthorizationEndpointFilter(string policyName) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var authorization = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();
        var result = await authorization.AuthorizeAsync(context.HttpContext.User, null, policyName);

        if (result.Succeeded)
        {
            return await next(context);
        }

        var authenticated = context.HttpContext.User.Identity?.IsAuthenticated == true;
        return Results.Problem(
            statusCode: authenticated ? StatusCodes.Status403Forbidden : StatusCodes.Status401Unauthorized,
            title: authenticated ? "You do not have permission to perform this action." : "Authentication required.",
            extensions: new Dictionary<string, object?>
            {
                ["code"] = authenticated ? AuthenticationErrorCodes.Forbidden : AuthenticationErrorCodes.Unauthenticated
            });
    }
}
