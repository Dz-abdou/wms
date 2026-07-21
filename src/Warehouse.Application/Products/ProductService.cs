using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Common.Models;
using Warehouse.Application.Common.Pagination;
using Warehouse.Application.Common.Persistence;
using Warehouse.Domain.Products;

namespace Warehouse.Application.Products;

public sealed class ProductService(IWarehouseDbContext dbContext, TimeProvider timeProvider)
{
    public async Task<PagedResult<ProductResponse>> GetListAsync(
        ProductListQuery query,
        CancellationToken cancellationToken)
    {
        var products = ApplySearch(dbContext.Products.AsNoTracking(), query.Search);
        var totalCount = await products.CountAsync(cancellationToken);
        var skip = (query.Page - PaginationConstants.DefaultPage) * query.PageSize;

        var items = await products
            .OrderBy(product => product.Sku)
            .ThenBy(product => product.Name)
            .Skip(skip)
            .Take(query.PageSize)
            .Select(product => ToResponse(product))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductResponse>(items, query.Page, query.PageSize, totalCount);
    }

    public async Task<ProductResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await FindProductAsync(id, asNoTracking: true, cancellationToken);
        return ToResponse(product);
    }

    public async Task<ProductResponse> CreateAsync(ProductInput input, CancellationToken cancellationToken)
    {
        var product = Product.Create(input.Sku, input.Name, input.Description, UtcNow());

        await EnsureSkuIsAvailableAsync(product.Sku, null, cancellationToken);
        dbContext.Products.Add(product);
        await SaveProductAsync(product.Sku, cancellationToken);

        return ToResponse(product);
    }

    public async Task<ProductResponse> UpdateAsync(
        Guid id,
        ProductInput input,
        CancellationToken cancellationToken)
    {
        var product = await FindProductAsync(id, asNoTracking: false, cancellationToken);
        var normalizedSku = Product.NormalizeSku(input.Sku);

        await EnsureSkuIsAvailableAsync(normalizedSku, id, cancellationToken);
        product.Update(normalizedSku, input.Name, input.Description, UtcNow());
        await SaveProductAsync(product.Sku, cancellationToken);

        return ToResponse(product);
    }

    public async Task<ProductResponse> SetStatusAsync(
        Guid id,
        SetProductStatusRequest request,
        CancellationToken cancellationToken)
    {
        var product = await FindProductAsync(id, asNoTracking: false, cancellationToken);
        if (product.IsActive == request.IsActive)
        {
            return ToResponse(product);
        }

        product.SetStatus(request.IsActive, UtcNow());
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(product);
    }

    private static IQueryable<Product> ApplySearch(IQueryable<Product> products, string? search)
    {
        var normalizedSearch = search?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalizedSearch))
        {
            return products;
        }

        return products.Where(product =>
            product.Sku.ToUpper().Contains(normalizedSearch) ||
            product.Name.ToUpper().Contains(normalizedSearch));
    }

    private async Task<Product> FindProductAsync(Guid id, bool asNoTracking, CancellationToken cancellationToken)
    {
        var products = asNoTracking ? dbContext.Products.AsNoTracking() : dbContext.Products.AsQueryable();

        return await products.SingleOrDefaultAsync(product => product.Id == id, cancellationToken)
            ?? throw new ProductNotFoundException(id);
    }

    private async Task EnsureSkuIsAvailableAsync(string sku, Guid? excludedProductId, CancellationToken cancellationToken)
    {
        var exists = await dbContext.Products.AnyAsync(
            product => product.Sku == sku && product.Id != excludedProductId,
            cancellationToken);

        if (exists)
        {
            throw new ProductSkuConflictException(sku);
        }
    }

    private async Task SaveProductAsync(string sku, CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            throw new ProductSkuConflictException(sku, exception);
        }
    }

    private DateTime UtcNow() => timeProvider.GetUtcNow().UtcDateTime;

    private static ProductResponse ToResponse(Product product) => new(
        product.Id,
        product.Sku,
        product.Name,
        product.Description,
        product.IsActive,
        product.CreatedAtUtc,
        product.UpdatedAtUtc);
}