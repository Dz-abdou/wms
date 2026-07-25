using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.Application.Common.Errors;
using Warehouse.Application.Common.Models;
using Warehouse.Application.Suppliers;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.IntegrationTests;

[Collection(ProductApiCollection.Name)]
public sealed class SupplierEndpointTests(ProductApiFixture fixture)
{
    [Fact]
    public async Task Create_normalizes_code_and_duplicate_code_returns_conflict()
    {
        var code = $"SUP-{Guid.NewGuid():N}"[..16].ToUpperInvariant();
        var created = await CreateAsync($" {code.ToLowerInvariant()} ", "Acme Supplies");
        Assert.Equal(code, created.Code);

        var duplicate = await fixture.Client.PostAsJsonAsync("/api/suppliers", new { code = code.ToLowerInvariant(), name = "Duplicate" });
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        await AssertCodeAsync(duplicate, ApiErrorCodes.SupplierCodeConflict);
    }

    [Fact]
    public async Task Invalid_input_and_unknown_supplier_return_stable_codes()
    {
        var invalid = await fixture.Client.PostAsJsonAsync("/api/suppliers", new { code = " ", name = "" });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        var validation = await invalid.Content.ReadFromJsonAsync<ValidationProblem>();
        Assert.NotNull(validation);
        Assert.Equal(ApiErrorCodes.ValidationFailed, validation.Code);
        Assert.Equal(ApiErrorCodes.ValidationRequired, Assert.Single(validation.ErrorCodes["Code"]));
        Assert.Equal(ApiErrorCodes.ValidationRequired, Assert.Single(validation.ErrorCodes["Name"]));

        var missing = await fixture.Client.GetAsync($"/api/suppliers/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        await AssertCodeAsync(missing, ApiErrorCodes.SupplierNotFound);
    }

    [Fact]
    public async Task List_paginates_and_status_change_is_persisted()
    {
        await CreateAsync($"A-{Guid.NewGuid():N}"[..12], "First");
        var second = await CreateAsync($"B-{Guid.NewGuid():N}"[..12], "Second");

        var list = await fixture.Client.GetFromJsonAsync<PagedResult<SupplierResponse>>("/api/suppliers?page=1&pageSize=1");
        Assert.NotNull(list);
        Assert.Single(list.Items);
        Assert.True(list.TotalCount >= 2);

        var status = await fixture.Client.PatchAsJsonAsync($"/api/suppliers/{second.Id}/status", new { isActive = false });
        status.EnsureSuccessStatusCode();
        var updated = await status.Content.ReadFromJsonAsync<SupplierResponse>();
        Assert.NotNull(updated);
        Assert.False(updated.IsActive);
    }

    [Fact]
    public async Task PostgreSql_unique_code_index_rejects_a_duplicate_code()
    {
        var code = $"DATABASE-{Guid.NewGuid():N}"[..20];
        await CreateAsync(code, "Database original");

        using var scope = fixture.Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        dbContext.Suppliers.Add(Warehouse.Domain.Suppliers.Supplier.Create(code, "Database duplicate", null, null, null, DateTime.UtcNow));

        await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
    }

    private async Task<SupplierResponse> CreateAsync(string code, string name)
    {
        var response = await fixture.Client.PostAsJsonAsync("/api/suppliers", new { code, name });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<SupplierResponse>())!;
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
