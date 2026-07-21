namespace Warehouse.Infrastructure.Identity;

public static class ApplicationRoles
{
    public const string Admin = "admin";
    public const string Manager = "manager";
    public const string Operator = "operator";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(
        StringComparer.Ordinal)
    {
        Admin,
        Manager,
        Operator
    };
}