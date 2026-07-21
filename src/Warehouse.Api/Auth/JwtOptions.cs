namespace Warehouse.Api.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; init; } = null!;
    public string Audience { get; init; } = null!;
    public string SigningKey { get; init; } = null!;
    public int AccessTokenMinutes { get; init; } = 15;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Issuer) || string.IsNullOrWhiteSpace(Audience))
        {
            throw new InvalidOperationException("JWT issuer and audience are required.");
        }

        if (string.IsNullOrWhiteSpace(SigningKey) || SigningKey.Length < 32)
        {
            throw new InvalidOperationException("JWT signing key must be at least 32 characters.");
        }

        if (AccessTokenMinutes is < 1 or > 60)
        {
            throw new InvalidOperationException("JWT access token lifetime must be between 1 and 60 minutes.");
        }
    }
}