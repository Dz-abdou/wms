using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Warehouse.Api.Middleware;

internal static class ProblemDetailsResponseWriter
{
    public static async Task WriteAsync(
        IProblemDetailsService problemDetailsService,
        HttpContext httpContext,
        Exception exception,
        ProblemDetails problemDetails,
        CancellationToken cancellationToken)
    {
        var wasWritten = await problemDetailsService.TryWriteAsync(new()
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails
        });

        if (wasWritten)
        {
            return;
        }

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
    }
}