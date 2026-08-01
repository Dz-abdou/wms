using System.Net;
using System.Net.Http.Json;
using Warehouse.Application.Common.Errors;
using Warehouse.Application.Common.Models;
using Warehouse.Application.Customers;

namespace Warehouse.IntegrationTests;

[Collection(ProductApiCollection.Name)]
public sealed class CustomerEndpointTests(ProductApiFixture fixture)
{
    [Fact]
    public async Task Create_normalizes_code_and_duplicate_code_returns_conflict()
    {
        var code = $"CUS-{Guid.NewGuid():N}"[..16].ToUpperInvariant();
        var created = await CreateAsync($" {code.ToLowerInvariant()} ", "Acme Customer");
        Assert.Equal(code, created.Code);

        var duplicate = await fixture.Client.PostAsJsonAsync("/api/customers", new { code = code.ToLowerInvariant(), legalName = "Duplicate customer" });
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        await AssertCodeAsync(duplicate, ApiErrorCodes.CustomerCodeConflict);
    }

    [Fact]
    public async Task Contact_and_address_are_maintained_on_the_customer_detail()
    {
        var customer = await CreateAsync($"CUS-{Guid.NewGuid():N}"[..16], "Contact customer");
        var contact = await fixture.Client.PostAsJsonAsync($"/api/customers/{customer.Id}/contacts", new { name = " Amina B. ", role = "Receiving", email = "amina@example.test", phoneNumber = "+213 555 000 000" });
        contact.EnsureSuccessStatusCode();

        var address = await fixture.Client.PostAsJsonAsync($"/api/customers/{customer.Id}/addresses", new { label = "Main warehouse", addressLine1 = "1 Main Street", city = "Algiers", countryCode = "dz", isShippingAddress = true, isBillingAddress = false });
        address.EnsureSuccessStatusCode();

        var detail = await fixture.Client.GetFromJsonAsync<CustomerResponse>($"/api/customers/{customer.Id}");
        Assert.NotNull(detail);
        var savedContact = Assert.Single(detail.Contacts);
        Assert.Equal("Amina B.", savedContact.Name);
        var savedAddress = Assert.Single(detail.Addresses);
        Assert.Equal("DZ", savedAddress.CountryCode);
        Assert.True(savedAddress.IsShippingAddress);
    }

    [Fact]
    public async Task Invalid_address_purpose_and_unknown_customer_return_stable_codes()
    {
        var customer = await CreateAsync($"CUS-{Guid.NewGuid():N}"[..16], "Validation customer");
        var invalid = await fixture.Client.PostAsJsonAsync($"/api/customers/{customer.Id}/addresses", new { label = "Main", addressLine1 = "1 Main Street", city = "Algiers", countryCode = "DZ", isShippingAddress = false, isBillingAddress = false });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        var validation = await invalid.Content.ReadFromJsonAsync<ValidationProblem>();
        Assert.NotNull(validation);
        Assert.Equal(ApiErrorCodes.ValidationFailed, validation.Code);
        Assert.Equal(ApiErrorCodes.ValidationRequired, Assert.Single(validation.ErrorCodes["IsShippingAddress"]));

        var missing = await fixture.Client.GetAsync($"/api/customers/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        await AssertCodeAsync(missing, ApiErrorCodes.CustomerNotFound);
    }

    [Fact]
    public async Task List_paginates_and_status_change_is_persisted()
    {
        await CreateAsync($"A-{Guid.NewGuid():N}"[..12], "First");
        var second = await CreateAsync($"B-{Guid.NewGuid():N}"[..12], "Second");

        var list = await fixture.Client.GetFromJsonAsync<PagedResult<CustomerListItemResponse>>("/api/customers?page=1&pageSize=1");
        Assert.NotNull(list);
        Assert.Single(list.Items);
        Assert.True(list.TotalCount >= 2);

        var status = await fixture.Client.PatchAsJsonAsync($"/api/customers/{second.Id}/status", new { isActive = false });
        status.EnsureSuccessStatusCode();
        var updated = await status.Content.ReadFromJsonAsync<CustomerResponse>();
        Assert.NotNull(updated);
        Assert.False(updated.IsActive);
    }

    private async Task<CustomerResponse> CreateAsync(string code, string legalName)
    {
        var response = await fixture.Client.PostAsJsonAsync("/api/customers", new { code, legalName });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CustomerResponse>())!;
    }

    private static async Task AssertCodeAsync(HttpResponseMessage response, string code)
    {
        var problem = await response.Content.ReadFromJsonAsync<Problem>();
        Assert.NotNull(problem);
        Assert.Equal(code, problem.Code);
    }

    private sealed record Problem(string? Code);
    private sealed record ValidationProblem(string? Code, Dictionary<string, string[]> ErrorCodes);
}
