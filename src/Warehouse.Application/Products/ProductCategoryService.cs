using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Common.Identity;
using Warehouse.Application.Common.Models;
using Warehouse.Application.Common.Pagination;
using Warehouse.Application.Common.Persistence;
using Warehouse.Domain.Products;

namespace Warehouse.Application.Products;

public sealed class ProductCategoryService(
    IWarehouseDbContext dbContext,
    TimeProvider timeProvider,
    ICurrentUser currentUser)
{
    public async Task<PagedResult<ProductCategoryResponse>> GetListAsync(
        ProductCategoryListQuery query,
        CancellationToken cancellationToken)
    {
        var categories = dbContext.ProductCategories.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search)) { var search = query.Search.Trim().ToUpper(); categories = categories.Where(x => x.Code.ToUpper().Contains(search) || x.Name.ToUpper().Contains(search)); }
        var totalCount = await categories.CountAsync(cancellationToken);
        var skip = (query.Page - PaginationConstants.DefaultPage) * query.PageSize;
        var items = await categories
            .OrderBy(category => category.Code)
            .Skip(skip)
            .Take(query.PageSize)
            .Select(category => ToResponse(category))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductCategoryResponse>(items, query.Page, query.PageSize, totalCount);
    }

    public async Task<ProductCategoryResponse> CreateAsync(
        ProductCategoryInput input,
        CancellationToken cancellationToken)
    {
        var category = ProductCategory.Create(
            input.Code,
            input.Name,
            input.ParentCategoryId,
            timeProvider.GetUtcNow().UtcDateTime,
            currentUser.UserId);

        if (input.ParentCategoryId is { } parentCategoryId &&
            !await dbContext.ProductCategories.AnyAsync(candidate => candidate.Id == parentCategoryId, cancellationToken))
        {
            throw new ProductCategoryNotFoundException(parentCategoryId);
        }

        if (await dbContext.ProductCategories.AnyAsync(candidate => candidate.Code == category.Code, cancellationToken))
        {
            throw new ProductCategoryCodeConflictException(category.Code);
        }

        dbContext.ProductCategories.Add(category);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            throw new ProductCategoryCodeConflictException(category.Code, exception);
        }

        return ToResponse(category);
    }

    public async Task<ProductCategoryResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        ToResponse(await FindAsync(id, cancellationToken));

    public async Task<ProductCategoryResponse> UpdateAsync(Guid id, ProductCategoryInput input, CancellationToken cancellationToken)
    {
        var category = await FindAsync(id, cancellationToken);
        if (input.ParentCategoryId == id) throw new ProductCategoryInvalidParentException(id);
        if (input.ParentCategoryId is { } parentCategoryId && !await dbContext.ProductCategories.AnyAsync(candidate => candidate.Id == parentCategoryId, cancellationToken)) throw new ProductCategoryNotFoundException(parentCategoryId);
        var normalizedCode = ProductCategory.NormalizeCode(input.Code);
        if (await dbContext.ProductCategories.AnyAsync(candidate => candidate.Code == normalizedCode && candidate.Id != id, cancellationToken)) throw new ProductCategoryCodeConflictException(normalizedCode);
        category.Update(input.Code, input.Name, input.ParentCategoryId, timeProvider.GetUtcNow().UtcDateTime, currentUser.UserId);
        try { await dbContext.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException exception) { throw new ProductCategoryCodeConflictException(normalizedCode, exception); }
        return ToResponse(category);
    }

    private async Task<ProductCategory> FindAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.ProductCategories.SingleOrDefaultAsync(category => category.Id == id, cancellationToken)
        ?? throw new ProductCategoryNotFoundException(id);

    private static ProductCategoryResponse ToResponse(ProductCategory category) => new(
        category.Id,
        category.Code,
        category.Name,
        category.ParentCategoryId,
        category.CreatedAtUtc,
        category.UpdatedAtUtc);
}
