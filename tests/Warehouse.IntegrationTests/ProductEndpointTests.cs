using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.Application.Common.Errors;
using Warehouse.Application.Common.Models;
using Warehouse.Application.Products;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.IntegrationTests;

[Collection(ProductApiCollection.Name)]
public sealed class ProductEndpointTests(ProductApiFixture fixture)
{
    [Fact]
    public async Task Create_normalizes_sku_and_duplicate_sku_returns_conflict()
    {
        var createResponse = await fixture.Client.PostAsJsonAsync("/api/products", new
        {
            sku = " sku-001 ",
            name = "Sample product",
            description = "Optional description",
            baseUnitOfMeasure = "EA",
            unitConversions = new[]
            {
                new { unitOfMeasure = "CTN", quantityInBaseUnit = 24m }
            },
            measurements = new
            {
                netWeight = 1.2m,
                grossWeight = 1.5m,
                weightUnitOfMeasure = "KG",
                length = 20m,
                width = 10m,
                height = 5m,
                dimensionUnitOfMeasure = "CM"
            }
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var product = await createResponse.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(product);
        Assert.Equal("SKU-001", product.Sku);
        Assert.Equal("EA", product.BaseUnitOfMeasure);
        Assert.Equal("CTN", Assert.Single(product.UnitConversions).UnitOfMeasure);
        Assert.Equal(24m, product.UnitConversions.Single().QuantityInBaseUnit);
        Assert.NotNull(product.Measurements);
        Assert.Equal(0.001m, product.Measurements.VolumeCubicMetres);

        var duplicateResponse = await fixture.Client.PostAsJsonAsync("/api/products", new
        {
            sku = "sku-001",
            name = "Duplicate product"
        });

        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
        await AssertErrorCodeAsync(duplicateResponse, ApiErrorCodes.ProductSkuConflict);

        var secondProduct = await CreateProductAsync("SKU-002", "Second product");
        var updateResponse = await fixture.Client.PutAsJsonAsync($"/api/products/{secondProduct.Id}", new
        {
            sku = "sku-001",
            name = "Second product"
        });

        Assert.Equal(HttpStatusCode.Conflict, updateResponse.StatusCode);
        await AssertErrorCodeAsync(updateResponse, ApiErrorCodes.ProductSkuConflict);
    }

    [Fact]
    public async Task Invalid_product_input_returns_field_error_codes()
    {
        var response = await fixture.Client.PostAsJsonAsync("/api/products", new
        {
            sku = " ",
            name = ""
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemResponse>();
        Assert.NotNull(problem);
        Assert.Equal(ApiErrorCodes.ValidationFailed, problem.Code);
        Assert.Equal(ApiErrorCodes.ValidationRequired, Assert.Single(problem.ErrorCodes["Sku"]));
        Assert.Equal(ApiErrorCodes.ValidationRequired, Assert.Single(problem.ErrorCodes["Name"]));
    }

    [Fact]
    public async Task List_searches_case_insensitively_and_paginates()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        var first = await CreateProductAsync($"ALPHA-{suffix}", "Alpha product");
        var second = await CreateProductAsync($"BETA-{suffix}", "Beta product");
        var third = await CreateProductAsync($"GAMMA-{suffix}", "Gamma product");

        var searchResponse = await fixture.Client.GetFromJsonAsync<PagedResult<ProductResponse>>(
            $"/api/products?search={suffix.ToLowerInvariant()}&page=1&pageSize=2");

        Assert.NotNull(searchResponse);
        Assert.Equal(3, searchResponse.TotalCount);
        Assert.Equal(2, searchResponse.Items.Count);
        Assert.Contains(searchResponse.Items, item => item.Id == first.Id);
        Assert.DoesNotContain(searchResponse.Items, item => item.Id == third.Id);

        var secondPageResponse = await fixture.Client.GetFromJsonAsync<PagedResult<ProductResponse>>(
            $"/api/products?search={suffix}&page=2&pageSize=2");

        Assert.NotNull(secondPageResponse);
        Assert.Single(secondPageResponse.Items);
        Assert.Contains(secondPageResponse.Items, item => item.Id == third.Id);
    }

    [Fact]
    public async Task Unknown_product_returns_not_found_and_status_change_is_persisted()
    {
        var missingId = Guid.NewGuid();
        var missingResponse = await fixture.Client.GetAsync($"/api/products/{missingId}");
        Assert.Equal(HttpStatusCode.NotFound, missingResponse.StatusCode);
        await AssertErrorCodeAsync(missingResponse, ApiErrorCodes.ProductNotFound);

        var missingUpdateResponse = await fixture.Client.PutAsJsonAsync($"/api/products/{missingId}", new
        {
            sku = "MISSING-001",
            name = "Missing product"
        });
        Assert.Equal(HttpStatusCode.NotFound, missingUpdateResponse.StatusCode);
        await AssertErrorCodeAsync(missingUpdateResponse, ApiErrorCodes.ProductNotFound);

        var product = await CreateProductAsync($"STATUS-{Guid.NewGuid():N}"[..14], "Status product");
        var statusResponse = await fixture.Client.PatchAsJsonAsync(
            $"/api/products/{product.Id}/status",
            new { isActive = false });

        Assert.Equal(HttpStatusCode.OK, statusResponse.StatusCode);
        var updated = await statusResponse.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(updated);
        Assert.False(updated.IsActive);

        var detail = await fixture.Client.GetFromJsonAsync<ProductResponse>($"/api/products/{product.Id}");
        Assert.NotNull(detail);
        Assert.False(detail.IsActive);
    }

    [Fact]
    public async Task PostgreSql_unique_sku_index_rejects_a_duplicate_sku()
    {
        var sku = $"DATABASE-{Guid.NewGuid():N}"[..20];
        await CreateProductAsync(sku, "Database constraint product");

        using var scope = fixture.Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        var duplicate = Warehouse.Domain.Products.Product.Create(sku, "Duplicate", null, DateTime.UtcNow);
        dbContext.Products.Add(duplicate);

        await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task Categories_can_be_created_and_assigned_to_a_product()
    {
        var parentResponse = await fixture.Client.PostAsJsonAsync("/api/product-categories", new
        {
            code = "CONSUMABLES",
            name = "Consumables"
        });
        parentResponse.EnsureSuccessStatusCode();
        var parent = await parentResponse.Content.ReadFromJsonAsync<ProductCategoryResponse>();
        Assert.NotNull(parent);

        var childResponse = await fixture.Client.PostAsJsonAsync("/api/product-categories", new
        {
            code = "PACKAGING",
            name = "Packaging",
            parentCategoryId = parent.Id
        });
        childResponse.EnsureSuccessStatusCode();
        var child = await childResponse.Content.ReadFromJsonAsync<ProductCategoryResponse>();
        Assert.NotNull(child);
        Assert.Equal(parent.Id, child.ParentCategoryId);

        var productResponse = await fixture.Client.PostAsJsonAsync("/api/products", new
        {
            sku = ("CAT-" + Guid.NewGuid().ToString("N"))[..14],
            name = "Categorized product",
            categoryId = child.Id
        });
        productResponse.EnsureSuccessStatusCode();
        var product = await productResponse.Content.ReadFromJsonAsync<ProductResponse>();

        Assert.NotNull(product);
        Assert.Equal(child.Id, product.CategoryId);
    }

    private async Task<ProductResponse> CreateProductAsync(string sku, string name)
    {
        var response = await fixture.Client.PostAsJsonAsync("/api/products", new { sku, name });
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<ProductResponse>())
            ?? throw new InvalidOperationException("Product response body was empty.");
    }

    private static async Task AssertErrorCodeAsync(HttpResponseMessage response, string expectedCode)
    {
        var problem = await response.Content.ReadFromJsonAsync<ProblemResponse>();
        Assert.NotNull(problem);
        Assert.Equal(expectedCode, problem.Code);
    }

    private sealed record ProblemResponse(string? Code);

    private sealed record ValidationProblemResponse(
        string? Code,
        Dictionary<string, string[]> ErrorCodes);
}
