namespace Warehouse.Application.Common.Identity;

public interface ICurrentUser
{
    Guid? UserId { get; }

    bool IsAuthenticated { get; }

    IReadOnlySet<string> Roles { get; }

    bool IsInRole(string role);
}

public sealed class CurrentUser : ICurrentUser
{
    private readonly HashSet<string> roles;

    public CurrentUser(
        Guid? userId = null,
        bool isAuthenticated = false,
        IEnumerable<string>? roles = null)
    {
        UserId = userId;
        IsAuthenticated = isAuthenticated;
        this.roles = roles is null
            ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(roles, StringComparer.OrdinalIgnoreCase);
    }

    public Guid? UserId { get; }

    public bool IsAuthenticated { get; }

    public IReadOnlySet<string> Roles => roles;

    public bool IsInRole(string role) => !string.IsNullOrWhiteSpace(role) && roles.Contains(role);
}
