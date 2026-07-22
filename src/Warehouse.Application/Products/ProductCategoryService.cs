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

    private static ProductCategoryResponse ToResponse(ProductCategory category) => new(
        category.Id,
        category.Code,
        category.Name,
        category.ParentCategoryId,
        category.CreatedAtUtc,
        category.UpdatedAtUtc);
}

