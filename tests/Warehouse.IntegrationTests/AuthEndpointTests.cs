using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Warehouse.IntegrationTests;

[Collection(ProductApiCollection.Name)]
public sealed class AuthEndpointTests(ProductApiFixture fixture)
{
    [Fact]
    public async Task Login_and_refresh_use_the_versioned_refresh_cookie()
    {
        using var client = fixture.Factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });

        var login = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin@warehouse.test",
            password = "TestPassword123!"
        });

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        Assert.Contains("wms_refresh_token_v1=", login.Headers.GetValues("Set-Cookie").Single());

        var refresh = await client.PostAsync("/api/auth/refresh", null);

        Assert.Equal(HttpStatusCode.OK, refresh.StatusCode);
        Assert.Contains("wms_refresh_token_v1=", refresh.Headers.GetValues("Set-Cookie").Single());
    }
}
