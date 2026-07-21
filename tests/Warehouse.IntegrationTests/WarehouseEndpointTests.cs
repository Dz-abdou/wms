using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.Application.Common.Errors;
using Warehouse.Application.Common.Models;
using Warehouse.Application.Warehouses;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.IntegrationTests;

[Collection(ProductApiCollection.Name)]
public sealed class WarehouseEndpointTests(ProductApiFixture fixture)
{
    [Fact]
    public async Task Create_normalizes_code_and_duplicate_code_returns_conflict()
    {
        var code = $"MAIN-{Guid.NewGuid():N}"[..13].ToUpperInvariant();
        var created = await CreateAsync($" {code.ToLowerInvariant()} ", "Main warehouse");
        Assert.Equal(code, created.Code);

        var duplicate = await fixture.Client.PostAsJsonAsync("/api/warehouses", new { code = code.ToLowerInvariant(), name = "Duplicate" });
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        await AssertCodeAsync(duplicate, ApiErrorCodes.WarehouseCodeConflict);
    }

    [Fact]
    public async Task Invalid_input_and_unknown_warehouse_return_stable_codes()
    {
        var invalid = await fixture.Client.PostAsJsonAsync("/api/warehouses", new { code = " ", name = "" });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        var validation = await invalid.Content.ReadFromJsonAsync<ValidationProblem>();
        Assert.NotNull(validation);
        Assert.Equal(ApiErrorCodes.ValidationFailed, validation.Code);
        Assert.Equal(ApiErrorCodes.ValidationRequired, Assert.Single(validation.ErrorCodes["Code"]));

        var missing = await fixture.Client.GetAsync($"/api/warehouses/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        await AssertCodeAsync(missing, ApiErrorCodes.WarehouseNotFound);
    }

    [Fact]
    public async Task List_paginates_and_database_rejects_duplicate_code()
    {
        await CreateAsync($"A-{Guid.NewGuid():N}"[..12], "First");
        await CreateAsync($"B-{Guid.NewGuid():N}"[..12], "Second");
        var list = await fixture.Client.GetFromJsonAsync<PagedResult<WarehouseResponse>>("/api/warehouses?page=1&pageSize=1");
        Assert.NotNull(list);
        Assert.Single(list.Items);
        Assert.True(list.TotalCount >= 2);

        var duplicateCode = $"DATABASE-{Guid.NewGuid():N}"[..18];
        await CreateAsync(duplicateCode, "Database original");
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        db.Warehouses.Add(Warehouse.Domain.Warehouses.Warehouse.Create(duplicateCode, "Database duplicate", null, DateTime.UtcNow));
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    private async Task<WarehouseResponse> CreateAsync(string code, string name)
    {
        var response = await fixture.Client.PostAsJsonAsync("/api/warehouses", new { code, name });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<WarehouseResponse>())!;
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