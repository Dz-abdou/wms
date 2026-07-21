using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Api.Middleware;

namespace Warehouse.Api.Auth;

public sealed class AuthorizationProblemDetailsMiddlewareResultHandler(
    IProblemDetailsService problemDetailsService) : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler fallback = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Challenged)
        {
            await WriteAsync(context, StatusCodes.Status401Unauthorized, "Authentication required.", AuthenticationErrorCodes.Unauthenticated);
            return;
        }

        if (authorizeResult.Forbidden)
        {
            await WriteAsync(context, StatusCodes.Status403Forbidden, "You do not have permission to perform this action.", AuthenticationErrorCodes.Forbidden);
            return;
        }

        await fallback.HandleAsync(next, context, policy, authorizeResult);
    }

    private Task WriteAsync(HttpContext context, int status, string title, string code) =>
        ProblemDetailsResponseWriter.WriteAsync(
            problemDetailsService,
            context,
            new InvalidOperationException(title),
            new ProblemDetails
            {
                Status = status,
                Title = title,
                Extensions = { ["code"] = code }
            },
            context.RequestAborted);
}
