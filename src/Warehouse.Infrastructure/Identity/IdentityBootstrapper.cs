using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Warehouse.Infrastructure.Identity;

public sealed class IdentityBootstrapper(UserManager<ApplicationUser> users, RoleManager<IdentityRole<Guid>> roles, IOptions<DevelopmentAdminOptions> options)
{
    public async Task SeedDevelopmentAdminAsync()
    {
        var admin = options.Value;
        if (string.IsNullOrWhiteSpace(admin.Email) || string.IsNullOrWhiteSpace(admin.Password)) return;
        foreach (var role in new[] { ApplicationRoles.Admin, ApplicationRoles.Manager, ApplicationRoles.Operator }) if (!await roles.RoleExistsAsync(role)) await roles.CreateAsync(new IdentityRole<Guid>(role));
        var user = await users.FindByEmailAsync(admin.Email);
        if (user is null) { user = new ApplicationUser { UserName = admin.Email, Email = admin.Email, EmailConfirmed = true }; var result = await users.CreateAsync(user, admin.Password); if (!result.Succeeded) throw new InvalidOperationException("Development administrator could not be created."); }
        if (!await users.IsInRoleAsync(user, ApplicationRoles.Admin)) await users.AddToRoleAsync(user, ApplicationRoles.Admin);
    }
}