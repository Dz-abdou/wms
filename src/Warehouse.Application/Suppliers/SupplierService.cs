using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Common.Identity;
using Warehouse.Application.Common.Models;
using Warehouse.Application.Common.Pagination;
using Warehouse.Application.Common.Persistence;
using Warehouse.Domain.Suppliers;

namespace Warehouse.Application.Suppliers;

public sealed class SupplierService(IWarehouseDbContext dbContext, TimeProvider timeProvider, ICurrentUser currentUser)
{
    public async Task<PagedResult<SupplierResponse>> GetListAsync(SupplierListQuery query, CancellationToken cancellationToken)
    {
        var suppliers = dbContext.Suppliers.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search)) { var search = query.Search.Trim().ToUpper(); suppliers = suppliers.Where(x => x.Code.ToUpper().Contains(search) || x.Name.ToUpper().Contains(search)); }
        if (query.IsActive is { } isActive) suppliers = suppliers.Where(x => x.IsActive == isActive);
        var totalCount = await suppliers.CountAsync(cancellationToken);
        var items = await suppliers.OrderBy(supplier => supplier.Code).ThenBy(supplier => supplier.Name)
            .Skip((query.Page - PaginationConstants.DefaultPage) * query.PageSize).Take(query.PageSize)
            .Select(supplier => ToResponse(supplier)).ToListAsync(cancellationToken);
        return new PagedResult<SupplierResponse>(items, query.Page, query.PageSize, totalCount);
    }

    public async Task<SupplierResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken) => ToResponse(await FindAsync(id, true, cancellationToken));

    public async Task<SupplierResponse> CreateAsync(SupplierInput input, CancellationToken cancellationToken)
    {
        var supplier = Supplier.Create(input.Code, input.Name, input.Email, input.PhoneNumber, input.Address, UtcNow(), currentUser.UserId);
        await EnsureCodeAvailableAsync(supplier.Code, null, cancellationToken);
        dbContext.Suppliers.Add(supplier);
        await SaveAsync(supplier.Code, cancellationToken);
        return ToResponse(supplier);
    }

    public async Task<SupplierResponse> UpdateAsync(Guid id, SupplierInput input, CancellationToken cancellationToken)
    {
        var supplier = await FindAsync(id, false, cancellationToken);
        var code = Supplier.NormalizeCode(input.Code);
        await EnsureCodeAvailableAsync(code, id, cancellationToken);
        supplier.Update(code, input.Name, input.Email, input.PhoneNumber, input.Address, UtcNow(), currentUser.UserId);
        await SaveAsync(code, cancellationToken);
        return ToResponse(supplier);
    }

    public async Task<SupplierResponse> SetStatusAsync(Guid id, SetSupplierStatusRequest request, CancellationToken cancellationToken)
    {
        var supplier = await FindAsync(id, false, cancellationToken);
        if (supplier.IsActive == request.IsActive) return ToResponse(supplier);
        supplier.SetStatus(request.IsActive, UtcNow(), currentUser.UserId);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(supplier);
    }

    private async Task<Supplier> FindAsync(Guid id, bool asNoTracking, CancellationToken cancellationToken)
    {
        var suppliers = asNoTracking ? dbContext.Suppliers.AsNoTracking() : dbContext.Suppliers.AsQueryable();
        return await suppliers.SingleOrDefaultAsync(supplier => supplier.Id == id, cancellationToken) ?? throw new SupplierNotFoundException(id);
    }

    private async Task EnsureCodeAvailableAsync(string code, Guid? excludedId, CancellationToken cancellationToken)
    {
        if (await dbContext.Suppliers.AnyAsync(supplier => supplier.Code == code && supplier.Id != excludedId, cancellationToken)) throw new SupplierCodeConflictException(code);
    }

    private async Task SaveAsync(string code, CancellationToken cancellationToken)
    {
        try { await dbContext.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException exception) { throw new SupplierCodeConflictException(code, exception); }
    }

    private DateTime UtcNow() => timeProvider.GetUtcNow().UtcDateTime;
    private static SupplierResponse ToResponse(Supplier supplier) => new(supplier.Id, supplier.Code, supplier.Name, supplier.Email, supplier.PhoneNumber, supplier.Address, supplier.IsActive, supplier.CreatedAtUtc, supplier.UpdatedAtUtc);
}
