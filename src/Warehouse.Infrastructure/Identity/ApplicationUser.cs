using Microsoft.AspNetCore.Identity;

namespace Warehouse.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public ICollection<RefreshToken> RefreshTokens { get; } = new List<RefreshToken>();
}