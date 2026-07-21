using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Warehouse.Api.Auth;
using Warehouse.Infrastructure.Identity;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.Api.Endpoints.Auth;

public static class AuthEndpoints
{
    private const string RefreshCookieName = "refresh_token";

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth").WithTags("Authentication");
        group.MapPost("/login", LoginAsync).AllowAnonymous();
        group.MapPost("/refresh", RefreshAsync).AllowAnonymous();
        group.MapPost("/logout", LogoutAsync).AllowAnonymous();
        group.MapGet("/me", GetCurrentUserAsync).RequireAuthorization();
        return endpoints;
    }

    private static async Task<IResult> LoginAsync(LoginRequest request, HttpContext context, UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signInManager, WarehouseDbContext db, JwtTokenService tokens, TimeProvider timeProvider, IWebHostEnvironment environment)
    {
        var user = await users.FindByEmailAsync(request.Email);
        if (user is null || !request.IsValid() || !(await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true)).Succeeded)
        {
            return AuthenticationProblem(StatusCodes.Status401Unauthorized, "Login failed.", AuthenticationErrorCodes.InvalidCredentials);
        }

        var refresh = await CreateRefreshTokenAsync(user.Id, db, timeProvider);
        WriteRefreshCookie(context, refresh.RawToken, refresh.ExpiresAtUtc, environment);
        return Results.Ok(new AccessTokenResponse(await tokens.CreateAccessTokenAsync(user)));
    }

    private static async Task<IResult> RefreshAsync(HttpContext context, WarehouseDbContext db, UserManager<ApplicationUser> users, JwtTokenService tokens, TimeProvider timeProvider, IWebHostEnvironment environment)
    {
        if (!context.Request.Cookies.TryGetValue(RefreshCookieName, out var rawToken)) return InvalidRefreshToken(context, environment);
        var token = await db.RefreshTokens.SingleOrDefaultAsync(item => item.TokenHash == Hash(rawToken));
        if (token is null || token.RevokedAtUtc is not null || token.ExpiresAtUtc <= timeProvider.GetUtcNow().UtcDateTime)
        {
            if (token is not null)
            {
                await RevokeActiveTokensAsync(token.UserId, db, timeProvider);
            }

            return InvalidRefreshToken(context, environment);
        }

        token.RevokedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        var replacement = await CreateRefreshTokenAsync(token.UserId, db, timeProvider, saveChanges: false);
        token.ReplacedByTokenId = replacement.Id;
        await db.SaveChangesAsync();
        WriteRefreshCookie(context, replacement.RawToken, replacement.ExpiresAtUtc, environment);
        var user = await users.FindByIdAsync(token.UserId.ToString());
        return user is null
            ? InvalidRefreshToken(context, environment)
            : Results.Ok(new AccessTokenResponse(await tokens.CreateAccessTokenAsync(user)));
    }

    private static async Task<IResult> LogoutAsync(HttpContext context, WarehouseDbContext db, TimeProvider timeProvider, IWebHostEnvironment environment)
    {
        if (context.Request.Cookies.TryGetValue(RefreshCookieName, out var rawToken))
        {
            var token = await db.RefreshTokens.SingleOrDefaultAsync(item => item.TokenHash == Hash(rawToken));
            if (token is not null && token.RevokedAtUtc is null) { token.RevokedAtUtc = timeProvider.GetUtcNow().UtcDateTime; await db.SaveChangesAsync(); }
        }
        context.Response.Cookies.Delete(RefreshCookieName, CookieOptions(context, environment));
        return Results.NoContent();
    }

    private static async Task<IResult> GetCurrentUserAsync(ClaimsPrincipal principal, UserManager<ApplicationUser> users)
    {
        var user = await users.GetUserAsync(principal);
        return user is null
            ? AuthenticationProblem(StatusCodes.Status401Unauthorized, "Authentication required.", AuthenticationErrorCodes.Unauthenticated)
            : Results.Ok(new CurrentUserResponse(user.Id, user.Email!, await users.GetRolesAsync(user)));
    }

    private static async Task<CreatedRefreshToken> CreateRefreshTokenAsync(Guid userId, WarehouseDbContext db, TimeProvider timeProvider, bool saveChanges = true)
    {
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var created = timeProvider.GetUtcNow().UtcDateTime;
        var token = new RefreshToken { Id = Guid.NewGuid(), UserId = userId, TokenHash = Hash(raw), CreatedAtUtc = created, ExpiresAtUtc = created.AddDays(7) };
        db.RefreshTokens.Add(token);
        if (saveChanges) await db.SaveChangesAsync();
        return new CreatedRefreshToken(token.Id, raw, token.ExpiresAtUtc);
    }

    private static async Task RevokeActiveTokensAsync(Guid userId, WarehouseDbContext db, TimeProvider timeProvider)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var activeTokens = await db.RefreshTokens.Where(item => item.UserId == userId && item.RevokedAtUtc == null).ToListAsync();
        foreach (var activeToken in activeTokens)
        {
            activeToken.RevokedAtUtc = now;
        }

        await db.SaveChangesAsync();
    }

    private static IResult InvalidRefreshToken(HttpContext context, IWebHostEnvironment environment)
    {
        context.Response.Cookies.Delete(RefreshCookieName, CookieOptions(context, environment));
        return AuthenticationProblem(StatusCodes.Status401Unauthorized, "Refresh token is invalid.", AuthenticationErrorCodes.InvalidRefreshToken);
    }

    private static IResult AuthenticationProblem(int status, string title, string code) =>
        Results.Problem(statusCode: status, title: title, extensions: new Dictionary<string, object?> { ["code"] = code });

    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(value)));
    private static void WriteRefreshCookie(HttpContext context, string value, DateTime expires, IWebHostEnvironment environment) => context.Response.Cookies.Append(RefreshCookieName, value, CookieOptions(context, environment, expires));
    private static CookieOptions CookieOptions(HttpContext context, IWebHostEnvironment environment, DateTime? expires = null) => new() { HttpOnly = true, Secure = environment.IsProduction() || context.Request.IsHttps, SameSite = environment.IsProduction() ? SameSiteMode.None : SameSiteMode.Lax, Path = "/api/auth", Expires = expires };
    private sealed record LoginRequest(string Email, string Password)
    {
        public bool IsValid() => !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);
    }
    private sealed record AccessTokenResponse(string AccessToken);
    private sealed record CurrentUserResponse(Guid Id, string Email, IList<string> Roles);
    private sealed record CreatedRefreshToken(Guid Id, string RawToken, DateTime ExpiresAtUtc);
}