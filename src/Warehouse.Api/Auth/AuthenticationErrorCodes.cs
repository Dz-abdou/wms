namespace Warehouse.Api.Auth;

public static class AuthenticationErrorCodes
{
    public const string InvalidCredentials = "auth.invalid_credentials";
    public const string InvalidRefreshToken = "auth.invalid_refresh_token";
    public const string Unauthenticated = "auth.unauthenticated";
    public const string Forbidden = "auth.forbidden";
}
