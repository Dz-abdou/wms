using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Application.Common.Errors;

namespace Warehouse.Api.Middleware;

public sealed class UnexpectedExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<UnexpectedExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is BadHttpRequestException)
        {
            logger.LogWarning(exception, "An invalid HTTP request was received.");

            await ProblemDetailsResponseWriter.WriteAsync(
                problemDetailsService,
                httpContext,
                exception,
                new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Request data is invalid.",
                    Extensions = { ["code"] = ApiErrorCodes.ValidationFailed }
                },
                cancellationToken);

            return true;
        }

        logger.LogError(exception, "An unhandled exception occurred while processing the request.");

        await ProblemDetailsResponseWriter.WriteAsync(
            problemDetailsService,
            httpContext,
            exception,
            new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred.",
                Extensions = { ["code"] = ApiErrorCodes.SystemUnexpected }
            },
            cancellationToken);

        return true;
    }
}
