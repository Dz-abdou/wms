using Warehouse.Application.Common.Pagination;
using Warehouse.Application.Products;
using Warehouse.Application.Warehouses;

namespace Warehouse.UnitTests.Pagination;

public sealed class PagedRequestValidatorTests
{
    [Fact]
    public void Feature_queries_use_the_shared_defaults()
    {
        var productQuery = new ProductListQuery();
        var warehouseQuery = new WarehouseListQuery();

        Assert.Equal(PaginationConstants.DefaultPage, productQuery.Page);
        Assert.Equal(PaginationConstants.DefaultPageSize, productQuery.PageSize);
        Assert.Equal(productQuery.Page, warehouseQuery.Page);
        Assert.Equal(productQuery.PageSize, warehouseQuery.PageSize);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    [InlineData(1_000_001, 20)]
    public void Feature_queries_reject_values_outside_shared_limits(
        int page,
        int pageSize)
    {
        var productResult = new ProductListQueryValidator()
            .Validate(new ProductListQuery(page, pageSize));
        var warehouseResult = new WarehouseListQueryValidator()
            .Validate(new WarehouseListQuery(page, pageSize));

        Assert.False(productResult.IsValid);
        Assert.False(warehouseResult.IsValid);
    }
}
