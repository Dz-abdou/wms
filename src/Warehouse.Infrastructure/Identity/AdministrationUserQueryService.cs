using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Administration;
using Warehouse.Application.Common.Models;
using Warehouse.Application.Common.Pagination;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.Infrastructure.Identity;

public sealed class AdministrationUserQueryService(
    WarehouseDbContext dbContext,
    UserManager<ApplicationUser> userManager) : IAdministrationUserQueryService
{
    public async Task<PagedResult<AdministrationUserResponse>> GetListAsync(
        AdministrationUserListQuery query,
        CancellationToken cancellationToken)
    {
        var users = dbContext.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Email))
        {
            var email = query.Email.Trim().ToUpperInvariant();
            users = users.Where(user => user.Email != null && user.Email.ToUpper().Contains(email));
        }

        if (!string.IsNullOrWhiteSpace(query.Role))
        {
            var role = query.Role.Trim();
            users = users.Where(user => dbContext.UserRoles.Any(userRole =>
                userRole.UserId == user.Id &&
                dbContext.Roles.Any(identityRole => identityRole.Id == userRole.RoleId && identityRole.Name == role)));
        }

        var totalCount = await users.CountAsync(cancellationToken);
        var page = await users
            .OrderBy(user => user.Email)
            .Skip((query.Page - PaginationConstants.DefaultPage) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var items = new List<AdministrationUserResponse>(page.Count);
        foreach (var user in page)
        {
            items.Add(await ToResponseAsync(user, cancellationToken));
        }

        return new PagedResult<AdministrationUserResponse>(items, query.Page, query.PageSize, totalCount);
    }

    public async Task<AdministrationUserResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
        return user is null ? null : await ToResponseAsync(user, cancellationToken);
    }

    private async Task<AdministrationUserResponse> ToResponseAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        var roles = await userManager.GetRolesAsync(user);
        cancellationToken.ThrowIfCancellationRequested();
        return new AdministrationUserResponse(
            user.Id,
            user.Email ?? string.Empty,
            roles.OrderBy(role => role).ToArray());
    }
}
