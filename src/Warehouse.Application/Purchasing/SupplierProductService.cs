using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Common.Identity;
using Warehouse.Application.Common.Errors;
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
        var catalogue = BuildCatalogueQuery(query.SupplierId, query.ProductId, query.IsActive, query.CurrencyCode);

        var totalCount = await catalogue.CountAsync(cancellationToken);
        var page = from catalogueItem in catalogue
                   join supplier in dbContext.Suppliers.AsNoTracking() on catalogueItem.SupplierId equals supplier.Id
                   join product in dbContext.Products.AsNoTracking() on catalogueItem.ProductId equals product.Id
                   orderby supplier.Code, product.Sku, catalogueItem.PurchaseUnitOfMeasure
                   select new { catalogueItem, supplier, product };
        var items = await page
            .Skip((query.Page - PaginationConstants.DefaultPage) * query.PageSize)
            .Take(query.PageSize)
            .Select(item => new SupplierProductResponse(
                item.catalogueItem.Id, item.catalogueItem.SupplierId, item.supplier.Code, item.supplier.Name,
                item.catalogueItem.ProductId, item.product.Sku, item.product.Name, item.catalogueItem.SupplierSku,
                item.catalogueItem.PurchaseUnitOfMeasure, item.catalogueItem.MinimumOrderQuantity, item.catalogueItem.UnitPrice,
                item.catalogueItem.CurrencyCode, item.catalogueItem.IsActive, item.catalogueItem.CreatedAtUtc, item.catalogueItem.UpdatedAtUtc))
            .ToListAsync(cancellationToken);
        return new PagedResult<SupplierProductResponse>(items, query.Page, query.PageSize, totalCount);
    }

    public async Task<SupplierProductResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await (from catalogueItem in BuildCatalogueQuery(id: id)
               join supplier in dbContext.Suppliers.AsNoTracking() on catalogueItem.SupplierId equals supplier.Id
               join product in dbContext.Products.AsNoTracking() on catalogueItem.ProductId equals product.Id
               select new SupplierProductResponse(
                   catalogueItem.Id, catalogueItem.SupplierId, supplier.Code, supplier.Name,
                   catalogueItem.ProductId, product.Sku, product.Name, catalogueItem.SupplierSku,
                   catalogueItem.PurchaseUnitOfMeasure, catalogueItem.MinimumOrderQuantity, catalogueItem.UnitPrice,
                   catalogueItem.CurrencyCode, catalogueItem.IsActive, catalogueItem.CreatedAtUtc, catalogueItem.UpdatedAtUtc))
        .SingleOrDefaultAsync(cancellationToken)
        ?? throw new SupplierProductNotFoundException(id);

    public async Task<SupplierProductResponse> CreateAsync(SupplierProductInput input, CancellationToken cancellationToken)
    {
        var supplier = await FindSupplierAsync(input.SupplierId, cancellationToken);
        var product = await FindProductAsync(input.ProductId, cancellationToken);
        var currencyCode = SupplierProduct.NormalizeCurrencyCode(input.CurrencyCode);
        await EnsurePurchasableAsync(supplier.IsActive, product, input.PurchaseUnitOfMeasure, input.MinimumOrderQuantity, currencyCode, cancellationToken);

        var catalogueItem = SupplierProduct.Create(
            input.SupplierId,
            input.ProductId,
            input.SupplierSku,
            input.PurchaseUnitOfMeasure,
            input.MinimumOrderQuantity,
            input.UnitPrice,
            currencyCode,
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
        var currencyCode = SupplierProduct.NormalizeCurrencyCode(input.CurrencyCode);
        await EnsurePurchasableAsync(supplier.IsActive, product, input.PurchaseUnitOfMeasure, input.MinimumOrderQuantity, currencyCode, cancellationToken);
        var unitOfMeasure = ProductUnitOfMeasure.NormalizeUnitOfMeasure(input.PurchaseUnitOfMeasure);
        await EnsureUniqueAsync(catalogueItem.SupplierId, catalogueItem.ProductId, unitOfMeasure, id, cancellationToken);
        catalogueItem.Update(input.SupplierSku, unitOfMeasure, input.MinimumOrderQuantity, input.UnitPrice, currencyCode, UtcNow(), currentUser.UserId);
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

    private IQueryable<SupplierProduct> BuildCatalogueQuery(
        Guid? supplierId = null,
        Guid? productId = null,
        bool? isActive = null,
        string? currencyCode = null,
        Guid? id = null)
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

        if (isActive is { } activeFilter)
        {
            catalogue = catalogue.Where(item => item.IsActive == activeFilter);
        }

        if (!string.IsNullOrWhiteSpace(currencyCode))
        {
            var normalizedCurrencyCode = currencyCode.Trim().ToUpperInvariant();
            catalogue = catalogue.Where(item => item.CurrencyCode == normalizedCurrencyCode);
        }

        if (id is { } itemId)
        {
            catalogue = catalogue.Where(item => item.Id == itemId);
        }

        return catalogue;
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

    private async Task EnsurePurchasableAsync(bool supplierIsActive, Product product, string? unitOfMeasure, decimal minimumOrderQuantity, string currencyCode, CancellationToken cancellationToken)
    {
        if (!supplierIsActive)
        {
            throw new SupplierProductFieldValidationException(
                "SupplierId",
                ApiErrorCodes.SupplierProductSupplierUnavailable,
                "The selected supplier is inactive.");
        }

        if (!product.IsActive)
        {
            throw new SupplierProductFieldValidationException(
                "ProductId",
                ApiErrorCodes.SupplierProductProductUnavailable,
                "The selected product is inactive.");
        }

        if (!IsPurchaseUnitAvailable(product, unitOfMeasure))
        {
            throw new SupplierProductFieldValidationException(
                "PurchaseUnitOfMeasure",
                ApiErrorCodes.SupplierProductPurchaseUnitUnavailable,
                "The selected purchase unit is not configured for this product.");
        }

        if (!product.TryConvertToBaseQuantity(unitOfMeasure, minimumOrderQuantity, out _))
        {
            throw new SupplierProductFieldValidationException(
                "MinimumOrderQuantity",
                ApiErrorCodes.SupplierProductMinimumOrderQuantityInvalid,
                "The minimum order quantity is not valid for the selected purchase unit.");
        }

        if (!await dbContext.Currencies.AnyAsync(currency => currency.Code == currencyCode && currency.IsActive, cancellationToken))
        {
            throw new SupplierProductCurrencyNotSupportedException(currencyCode);
        }
    }

    private static bool IsPurchaseUnitAvailable(Product product, string? unitOfMeasure)
    {
        try
        {
            var normalizedUnit = ProductUnitOfMeasure.NormalizeUnitOfMeasure(unitOfMeasure);
            return normalizedUnit == product.BaseUnitOfMeasure
                || product.UnitConversions.Any(conversion => conversion.UnitOfMeasure == normalizedUnit);
        }
        catch (ArgumentException)
        {
            return false;
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
