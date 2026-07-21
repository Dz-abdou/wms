using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Warehouse.Infrastructure.Identity;

namespace Warehouse.Api.Auth;

public sealed class JwtTokenService(IOptions<JwtOptions> options, UserManager<ApplicationUser> userManager, TimeProvider timeProvider)
{
    public async Task<string> CreateAccessTokenAsync(ApplicationUser user)
    {
        var jwt = options.Value;
        var roles = await userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? string.Empty)
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey));
        var token = new JwtSecurityToken(jwt.Issuer, jwt.Audience, claims, expires: timeProvider.GetUtcNow().AddMinutes(jwt.AccessTokenMinutes).UtcDateTime, signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}