using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Warehouse.Api.Auth;
using Warehouse.Infrastructure.Identity;

namespace Warehouse.Api.Endpoints.Administration;

public static class AdministrationEndpoints
{
    public static IEndpointRouteBuilder MapAdministrationEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/admin")
            .WithTags("Administration")
            .RequireAuthorization(AuthorizationPolicies.ManageAdministration);

        group.MapGet("/users", GetUsersAsync);
        group.MapPost("/users", CreateUserAsync);
        group.MapPut("/users/{id:guid}", UpdateUserAsync);
        group.MapDelete("/users/{id:guid}", DeleteUserAsync);
        group.MapGet("/roles", GetRolesAsync);
        group.MapPut("/users/{id:guid}/roles", SetUserRolesAsync);

        return endpoints;
    }

    private static async Task<IResult> GetUsersAsync(
        UserManager<ApplicationUser> users,
        CancellationToken cancellationToken)
    {
        var userEntities = await users.Users
            .OrderBy(user => user.Email)
            .ToListAsync(cancellationToken);

        var result = new List<UserResponse>(userEntities.Count);
        foreach (var user in userEntities)
        {
            result.Add(ToResponse(user, await users.GetRolesAsync(user)));
        }

        return Results.Ok(result);
    }

    private static async Task<IResult> CreateUserAsync(
        CreateUserRequest request,
        UserManager<ApplicationUser> users)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return ValidationProblem();
        }

        var email = request.Email.Trim();
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var result = await users.CreateAsync(user, request.Password);
        return result.Succeeded
            ? Results.Created($"/api/admin/users/{user.Id}", ToResponse(user, []))
            : IdentityProblem(result);
    }

    private static async Task<IResult> UpdateUserAsync(
        Guid id,
        UpdateUserRequest request,
        UserManager<ApplicationUser> users)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return ValidationProblem();
        }

        var user = await users.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return NotFound();
        }

        var email = request.Email.Trim();
        user.Email = email;
        user.UserName = email;

        var result = await users.UpdateAsync(user);
        return result.Succeeded
            ? Results.Ok(ToResponse(user, await users.GetRolesAsync(user)))
            : IdentityProblem(result);
    }

    private static async Task<IResult> DeleteUserAsync(
        Guid id,
        ClaimsPrincipal principal,
        UserManager<ApplicationUser> users)
    {
        if (IsCurrentUser(id, principal))
        {
            return SelfManagementProblem();
        }

        var user = await users.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return NotFound();
        }

        var result = await users.DeleteAsync(user);
        return result.Succeeded ? Results.NoContent() : IdentityProblem(result);
    }

    private static IResult GetRolesAsync() =>
        Results.Ok(ApplicationRoles.All.OrderBy(role => role).ToArray());

    private static async Task<IResult> SetUserRolesAsync(
        Guid id,
        SetUserRolesRequest request,
        ClaimsPrincipal principal,
        UserManager<ApplicationUser> users)
    {
        if (request.Roles is null ||
            request.Roles.Any(role => !ApplicationRoles.All.Contains(role)))
        {
            return ValidationProblem();
        }

        if (IsCurrentUser(id, principal))
        {
            return SelfManagementProblem();
        }

        var user = await users.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return NotFound();
        }

        var currentRoles = await users.GetRolesAsync(user);
        var desiredRoles = request.Roles.Distinct(StringComparer.Ordinal).ToArray();
        var removeResult = await users.RemoveFromRolesAsync(
            user,
            currentRoles.Except(desiredRoles, StringComparer.Ordinal));

        if (!removeResult.Succeeded)
        {
            return IdentityProblem(removeResult);
        }

        var addResult = await users.AddToRolesAsync(
            user,
            desiredRoles.Except(currentRoles, StringComparer.Ordinal));

        return addResult.Succeeded
            ? Results.Ok(ToResponse(user, await users.GetRolesAsync(user)))
            : IdentityProblem(addResult);
    }

    private static bool IsCurrentUser(Guid id, ClaimsPrincipal principal) =>
        string.Equals(
            principal.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            id.ToString(),
            StringComparison.OrdinalIgnoreCase);

    private static IResult ValidationProblem() =>
        Results.Problem(
            statusCode: StatusCodes.Status400BadRequest,
            title: "The request is invalid.",
            extensions: new Dictionary<string, object?> { ["code"] = "validation.failed" });

    private static IResult NotFound() =>
        Results.Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "User not found.",
            extensions: new Dictionary<string, object?> { ["code"] = "admin.user_not_found" });

    private static IResult SelfManagementProblem() =>
        Results.Problem(
            statusCode: StatusCodes.Status400BadRequest,
            title: "The current user cannot be changed here.",
            extensions: new Dictionary<string, object?> { ["code"] = "admin.self_management_forbidden" });

    private static IResult IdentityProblem(IdentityResult result) =>
        Results.Problem(
            statusCode: StatusCodes.Status400BadRequest,
            title: "The request could not be completed.",
            extensions: new Dictionary<string, object?>
            {
                ["code"] = result.Errors.Any(error =>
                    error.Code.Contains("Duplicate", StringComparison.OrdinalIgnoreCase))
                    ? "admin.email_conflict"
                    : "admin.user_invalid"
            });

    private static UserResponse ToResponse(
        ApplicationUser user,
        IEnumerable<string> roles) =>
        new(user.Id, user.Email ?? string.Empty, roles.OrderBy(role => role).ToArray());

    private sealed record CreateUserRequest(string? Email, string? Password);
    private sealed record UpdateUserRequest(string? Email);
    private sealed record SetUserRolesRequest(string[]? Roles);
    private sealed record UserResponse(Guid Id, string Email, string[] Roles);
}
