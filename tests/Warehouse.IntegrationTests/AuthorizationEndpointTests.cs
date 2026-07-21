using System.Net;
using System.Net.Http.Json;
using Warehouse.Api.Auth;
namespace Warehouse.IntegrationTests;
[Collection(ProductApiCollection.Name)]
public sealed class AuthorizationEndpointTests(ProductApiFixture fixture)
{
    [Fact] public async Task Anonymous_catalog_request_returns_authentication_problem() { using var client = fixture.Factory.CreateClient(); var response = await client.GetAsync("/api/products"); Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode); Assert.Contains(AuthenticationErrorCodes.Unauthenticated, await response.Content.ReadAsStringAsync()); }
    [Fact] public async Task Operator_can_read_but_cannot_manage_catalog() { using var client = await fixture.CreateClientForRoleAsync(AuthorizationPolicies.OperatorRole); Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/products")).StatusCode); var response = await client.PostAsJsonAsync("/api/products", new { sku = "OPERATOR-1", name = "Operator" }); Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode); Assert.Contains(AuthenticationErrorCodes.Forbidden, await response.Content.ReadAsStringAsync()); }
}
