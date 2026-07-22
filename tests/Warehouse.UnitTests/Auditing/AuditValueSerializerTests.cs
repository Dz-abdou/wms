using Warehouse.Infrastructure.Auditing;

namespace Warehouse.UnitTests.Auditing;

public sealed class AuditValueSerializerTests
{
    [Fact]
    public void Serialize_uses_json_that_preserves_primitive_types()
    {
        var serializer = new AuditValueSerializer();

        Assert.Equal("42", serializer.Serialize(42));
        Assert.Equal("true", serializer.Serialize(true));
        Assert.Equal("\"quoted value\"", serializer.Serialize("quoted value"));
        Assert.Null(serializer.Serialize(null));
    }

    [Fact]
    public void Profile_identifies_sensitive_property_names()
    {
        Assert.True(AuditProfile.IsSensitivePropertyName("RefreshToken"));
        Assert.True(AuditProfile.IsSensitivePropertyName("PasswordHash"));
        Assert.False(AuditProfile.IsSensitivePropertyName("DisplayName"));
    }
}
