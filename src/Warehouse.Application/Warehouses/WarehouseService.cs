using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Common.Models;
using Warehouse.Application.Common.Pagination;
using Warehouse.Application.Common.Persistence;
using Warehouse.Domain.Warehouses;
using WarehouseEntity = Warehouse.Domain.Warehouses.Warehouse;

namespace Warehouse.Application.Warehouses;

public sealed class WarehouseService(IWarehouseDbContext dbContext, TimeProvider timeProvider)
{
    public async Task<PagedResult<WarehouseResponse>> GetListAsync(
        WarehouseListQuery query,
        CancellationToken cancellationToken)
    {
        var warehouses = dbContext.Warehouses.AsNoTracking();
        var totalCount = await warehouses.CountAsync(cancellationToken);
        var skip = (query.Page - PaginationConstants.DefaultPage) * query.PageSize;

        var items = await warehouses
            .OrderBy(warehouse => warehouse.Code)
            .ThenBy(warehouse => warehouse.Name)
            .Skip(skip)
            .Take(query.PageSize)
            .Select(warehouse => ToResponse(warehouse))
            .ToListAsync(cancellationToken);

        return new PagedResult<WarehouseResponse>(items, query.Page, query.PageSize, totalCount);
    }

    public async Task<WarehouseResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var warehouse = await FindWarehouseAsync(id, asNoTracking: true, cancellationToken);
        return ToResponse(warehouse);
    }

    public async Task<WarehouseResponse> CreateAsync(WarehouseInput input, CancellationToken cancellationToken)
    {
        var warehouse = WarehouseEntity.Create(input.Code, input.Name, input.Description, UtcNow());

        await EnsureCodeIsAvailableAsync(warehouse.Code, null, cancellationToken);
        dbContext.Warehouses.Add(warehouse);
        await SaveWarehouseAsync(warehouse.Code, cancellationToken);

        return ToResponse(warehouse);
    }

    public async Task<WarehouseResponse> UpdateAsync(
        Guid id,
        WarehouseInput input,
        CancellationToken cancellationToken)
    {
        var warehouse = await FindWarehouseAsync(id, asNoTracking: false, cancellationToken);
        var normalizedCode = WarehouseEntity.NormalizeCode(input.Code);

        await EnsureCodeIsAvailableAsync(normalizedCode, id, cancellationToken);
        warehouse.Update(normalizedCode, input.Name, input.Description, UtcNow());
        await SaveWarehouseAsync(warehouse.Code, cancellationToken);

        return ToResponse(warehouse);
    }

    public async Task<WarehouseResponse> SetStatusAsync(
        Guid id,
        SetWarehouseStatusRequest request,
        CancellationToken cancellationToken)
    {
        var warehouse = await FindWarehouseAsync(id, asNoTracking: false, cancellationToken);
        if (warehouse.IsActive == request.IsActive)
        {
            return ToResponse(warehouse);
        }

        warehouse.SetStatus(request.IsActive, UtcNow());
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(warehouse);
    }

    private async Task<WarehouseEntity> FindWarehouseAsync(Guid id, bool asNoTracking, CancellationToken cancellationToken)
    {
        var warehouses = asNoTracking ? dbContext.Warehouses.AsNoTracking() : dbContext.Warehouses.AsQueryable();

        return await warehouses.SingleOrDefaultAsync(warehouse => warehouse.Id == id, cancellationToken)
            ?? throw new WarehouseNotFoundException(id);
    }

    private async Task EnsureCodeIsAvailableAsync(string code, Guid? excludedWarehouseId, CancellationToken cancellationToken)
    {
        var exists = await dbContext.Warehouses.AnyAsync(
            warehouse => warehouse.Code == code && warehouse.Id != excludedWarehouseId,
            cancellationToken);

        if (exists)
        {
            throw new WarehouseCodeConflictException(code);
        }
    }

    private async Task SaveWarehouseAsync(string code, CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            throw new WarehouseCodeConflictException(code, exception);
        }
    }

    private DateTime UtcNow() => timeProvider.GetUtcNow().UtcDateTime;

    private static WarehouseResponse ToResponse(WarehouseEntity warehouse) => new(
        warehouse.Id,
        warehouse.Code,
        warehouse.Name,
        warehouse.Description,
        warehouse.IsActive,
        warehouse.CreatedAtUtc,
        warehouse.UpdatedAtUtc);
}