using System.Text.Json;

namespace Warehouse.Infrastructure.Auditing;

public interface IAuditValueSerializer
{
    string? Serialize(object? value);
}

public sealed class AuditValueSerializer : IAuditValueSerializer
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    public string? Serialize(object? value) => value is null
        ? null
        : JsonSerializer.Serialize(value, value.GetType(), Options);
}
