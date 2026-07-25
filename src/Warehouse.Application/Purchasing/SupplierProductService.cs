using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Common.Identity;
using Warehouse.Application.Common.Models;
using Warehouse.Application.Common.Pagination;
using Warehouse.Application.Common.Persistence;
using Warehouse.Application.Products;
using Warehouse.Application.Suppliers;
using Warehouse.Domain.Products;
using Warehouse.Domain.Purchasing;

namespace Warehouse.Application.Purchasing;

public sealed class SupplierProductService(
    IWarehouseDbContext dbContext,
    TimeProvider timeProvider,
    ICurrentUser currentUser)
{
    public async Task<PagedResult<SupplierProductResponse>> GetListAsync(SupplierProductListQuery query, CancellationToken cancellationToken)
    {
        var catalogue = BuildResponseQuery(query.SupplierId, query.ProductId);

        var totalCount = await catalogue.CountAsync(cancellationToken);
        var items = await catalogue
            .OrderBy(item => item.SupplierCode)
            .ThenBy(item => item.ProductSku)
            .ThenBy(item => item.PurchaseUnitOfMeasure)
            .Skip((query.Page - PaginationConstants.DefaultPage) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);
        return new PagedResult<SupplierProductResponse>(items, query.Page, query.PageSize, totalCount);
    }

    public async Task<SupplierProductResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await BuildResponseQuery(id: id).SingleOrDefaultAsync(cancellationToken)
        ?? throw new SupplierProductNotFoundException(id);

    public async Task<SupplierProductResponse> CreateAsync(SupplierProductInput input, CancellationToken cancellationToken)
    {
        var supplier = await FindSupplierAsync(input.SupplierId, cancellationToken);
        var product = await FindProductAsync(input.ProductId, cancellationToken);
        EnsurePurchasable(supplier.IsActive, product, input.PurchaseUnitOfMeasure, input.MinimumOrderQuantity);

        var catalogueItem = SupplierProduct.Create(
            input.SupplierId,
            input.ProductId,
            input.SupplierSku,
            input.PurchaseUnitOfMeasure,
            input.MinimumOrderQuantity,
            input.UnitPrice,
            input.CurrencyCode,
            UtcNow(),
            currentUser.UserId);
        await EnsureUniqueAsync(catalogueItem.SupplierId, catalogueItem.ProductId, catalogueItem.PurchaseUnitOfMeasure, null, cancellationToken);
        dbContext.SupplierProducts.Add(catalogueItem);
        await SaveAsync(catalogueItem, cancellationToken);
        return await GetByIdAsync(catalogueItem.Id, cancellationToken);
    }

    public async Task<SupplierProductResponse> UpdateAsync(Guid id, UpdateSupplierProductInput input, CancellationToken cancellationToken)
    {
        var catalogueItem = await FindAsync(id, cancellationToken);
        var supplier = await FindSupplierAsync(catalogueItem.SupplierId, cancellationToken);
        var product = await FindProductAsync(catalogueItem.ProductId, cancellationToken);
        EnsurePurchasable(supplier.IsActive, product, input.PurchaseUnitOfMeasure, input.MinimumOrderQuantity);
        var unitOfMeasure = ProductUnitOfMeasure.NormalizeUnitOfMeasure(input.PurchaseUnitOfMeasure);
        await EnsureUniqueAsync(catalogueItem.SupplierId, catalogueItem.ProductId, unitOfMeasure, id, cancellationToken);
        catalogueItem.Update(input.SupplierSku, unitOfMeasure, input.MinimumOrderQuantity, input.UnitPrice, input.CurrencyCode, UtcNow(), currentUser.UserId);
        await SaveAsync(catalogueItem, cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<SupplierProductResponse> SetStatusAsync(Guid id, SetSupplierProductStatusRequest request, CancellationToken cancellationToken)
    {
        var catalogueItem = await FindAsync(id, cancellationToken);
        catalogueItem.SetStatus(request.IsActive, UtcNow(), currentUser.UserId);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    private IQueryable<SupplierProductResponse> BuildResponseQuery(Guid? supplierId = null, Guid? productId = null, Guid? id = null)
    {
        var catalogue = dbContext.SupplierProducts.AsNoTracking();
        if (supplierId is { } supplierFilter)
        {
            catalogue = catalogue.Where(item => item.SupplierId == supplierFilter);
        }

        if (productId is { } productFilter)
        {
            catalogue = catalogue.Where(item => item.ProductId == productFilter);
        }

        if (id is { } itemId)
        {
            catalogue = catalogue.Where(item => item.Id == itemId);
        }

        return from catalogueItem in catalogue
        join supplier in dbContext.Suppliers.AsNoTracking() on catalogueItem.SupplierId equals supplier.Id
        join product in dbContext.Products.AsNoTracking() on catalogueItem.ProductId equals product.Id
        select new SupplierProductResponse(
            catalogueItem.Id,
            catalogueItem.SupplierId,
            supplier.Code,
            supplier.Name,
            catalogueItem.ProductId,
            product.Sku,
            product.Name,
            catalogueItem.SupplierSku,
            catalogueItem.PurchaseUnitOfMeasure,
            catalogueItem.MinimumOrderQuantity,
            catalogueItem.UnitPrice,
            catalogueItem.CurrencyCode,
            catalogueItem.IsActive,
            catalogueItem.CreatedAtUtc,
            catalogueItem.UpdatedAtUtc);
    }

    private async Task<Warehouse.Domain.Suppliers.Supplier> FindSupplierAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.Suppliers.SingleOrDefaultAsync(supplier => supplier.Id == id, cancellationToken)
        ?? throw new SupplierNotFoundException(id);

    private async Task<Product> FindProductAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.Products.SingleOrDefaultAsync(product => product.Id == id, cancellationToken)
        ?? throw new ProductNotFoundException(id);

    private async Task<SupplierProduct> FindAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.SupplierProducts.SingleOrDefaultAsync(item => item.Id == id, cancellationToken)
        ?? throw new SupplierProductNotFoundException(id);

    private async Task EnsureUniqueAsync(Guid supplierId, Guid productId, string unitOfMeasure, Guid? excludedId, CancellationToken cancellationToken)
    {
        if (await dbContext.SupplierProducts.AnyAsync(item => item.SupplierId == supplierId && item.ProductId == productId && item.PurchaseUnitOfMeasure == unitOfMeasure && item.Id != excludedId, cancellationToken))
        {
            throw new SupplierProductConflictException(supplierId, productId, unitOfMeasure);
        }
    }

    private static void EnsurePurchasable(bool supplierIsActive, Product product, string? unitOfMeasure, decimal minimumOrderQuantity)
    {
        if (!supplierIsActive || !product.IsActive || !product.TryConvertToBaseQuantity(unitOfMeasure, minimumOrderQuantity, out _))
        {
            throw new PurchaseOrderCatalogueInvalidException("The supplier, product, purchase unit, or minimum order quantity is not valid for purchasing.");
        }
    }

    private async Task SaveAsync(SupplierProduct catalogueItem, CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            throw new SupplierProductConflictException(catalogueItem.SupplierId, catalogueItem.ProductId, catalogueItem.PurchaseUnitOfMeasure, exception);
        }
    }

    private DateTime UtcNow() => timeProvider.GetUtcNow().UtcDateTime;
}
