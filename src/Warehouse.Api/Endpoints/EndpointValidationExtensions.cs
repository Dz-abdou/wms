using FluentValidation;
using Microsoft.AspNetCore.Http;
using Warehouse.Application.Common.Errors;

namespace Warehouse.Api.Endpoints;

public static class EndpointValidationExtensions
{
    public static async Task<IResult?> ValidateRequestAsync<TRequest>(
        this IValidator<TRequest> validator,
        TRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (validationResult.IsValid)
        {
            return null;
        }

        var errors = validationResult.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray());

        var errorCodes = validationResult.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorCode).Distinct().ToArray());

        return Results.ValidationProblem(
            errors,
            extensions: new Dictionary<string, object?>
            {
                ["code"] = ApiErrorCodes.ValidationFailed,
                ["errorCodes"] = errorCodes
            });
    }
}