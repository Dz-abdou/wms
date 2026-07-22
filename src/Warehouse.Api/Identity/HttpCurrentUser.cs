using System.Security.Claims;
using Warehouse.Application.Common.Identity;

namespace Warehouse.Api.Identity;

public sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public Guid? UserId => Guid.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
        ? userId
        : null;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public IReadOnlySet<string> Roles => Principal?
        .FindAll(ClaimTypes.Role)
        .Select(claim => claim.Value)
        .ToHashSet(StringComparer.OrdinalIgnoreCase)
        ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public bool IsInRole(string role) => !string.IsNullOrWhiteSpace(role) && Roles.Contains(role);
}
