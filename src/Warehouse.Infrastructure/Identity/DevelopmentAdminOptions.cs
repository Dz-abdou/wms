namespace Warehouse.Infrastructure.Identity;

public sealed class DevelopmentAdminOptions
{
    public const string SectionName = "DevelopmentAdmin";
    public string? Email { get; init; }
    public string? Password { get; init; }
}