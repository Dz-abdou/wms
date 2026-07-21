using System.Security.Claims;
using Warehouse.Application.Common.Auditing;

namespace Warehouse.Api.Auditing;

public sealed class HttpContextAuditContext(IHttpContextAccessor accessor) : IAuditContext
{
    private HttpContext? HttpContext => accessor.HttpContext;

    public Guid? ActorUserId =>
        Guid.TryParse(HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
            ? userId
            : null;

    public string? CorrelationId => HttpContext?.TraceIdentifier;

    public string? Reason => null;
}
