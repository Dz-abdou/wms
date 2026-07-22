using Warehouse.Infrastructure.Identity;

namespace Warehouse.Api.Auth;

public static class AuthorizationPolicies
{
    public const string ManageAdministration = "admin.manage";
    public const string ManageCatalog = "catalog.manage";
    public const string ReadCatalog = "catalog.read";
    public const string ReadInventory = "inventory.read";
    public const string AdjustInventory = "inventory.adjust";
    public const string AdminRole = ApplicationRoles.Admin;
    public const string ManagerRole = ApplicationRoles.Manager;
    public const string OperatorRole = ApplicationRoles.Operator;
}