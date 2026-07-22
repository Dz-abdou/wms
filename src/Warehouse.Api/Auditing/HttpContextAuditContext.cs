using Warehouse.Application.Common.Auditing;

namespace Warehouse.Api.Auditing;

public sealed class HttpContextAuditContext(IHttpContextAccessor accessor) : IAuditContext
{
    private HttpContext? HttpContext => accessor.HttpContext;


    public string? CorrelationId => HttpContext?.TraceIdentifier;

    public string? Reason => null;
}
