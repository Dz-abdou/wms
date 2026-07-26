using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Common.Identity;
using Warehouse.Application.Common.Models;
using Warehouse.Application.Common.Pagination;
using Warehouse.Application.Common.Persistence;
using Warehouse.Domain.Currencies;

namespace Warehouse.Application.Currencies;

public sealed class CurrencyService(IWarehouseDbContext dbContext, TimeProvider timeProvider, ICurrentUser currentUser)
{
    public async Task<PagedResult<CurrencyResponse>> GetListAsync(CurrencyListQuery query, CancellationToken cancellationToken)
    {
        var currencies = dbContext.Currencies.AsNoTracking();
        if (query.ActiveOnly) currencies = currencies.Where(currency => currency.IsActive);
        var total = await currencies.CountAsync(cancellationToken);
        var items = await currencies.OrderBy(currency => currency.Code).Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).Select(currency => ToResponse(currency)).ToListAsync(cancellationToken);
        return new PagedResult<CurrencyResponse>(items, query.Page, query.PageSize, total);
    }

    public async Task<CurrencyResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken) => ToResponse(await FindAsync(id, cancellationToken));

    public async Task<CurrencyResponse> CreateAsync(CurrencyInput input, CancellationToken cancellationToken)
    {
        var currency = Currency.Create(input.Code, input.Name, input.Symbol, input.DecimalPlaces, false, UtcNow(), currentUser.UserId);
        if (await dbContext.Currencies.AnyAsync(candidate => candidate.Code == currency.Code, cancellationToken)) throw new CurrencyCodeConflictException(currency.Code);
        dbContext.Currencies.Add(currency);
        try { await dbContext.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException exception) { throw new CurrencyCodeConflictException(currency.Code, exception); }
        return ToResponse(currency);
    }

    public async Task<CurrencyResponse> UpdateAsync(Guid id, UpdateCurrencyInput input, CancellationToken cancellationToken)
    {
        var currency = await FindAsync(id, cancellationToken);
        currency.Update(input.Name, input.Symbol, input.DecimalPlaces, UtcNow(), currentUser.UserId);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(currency);
    }

    public async Task<CurrencyResponse> SetStatusAsync(Guid id, SetCurrencyStatusRequest request, CancellationToken cancellationToken)
    {
        var currency = await FindAsync(id, cancellationToken);
        if (currency.IsDefault && !request.IsActive) throw new DefaultCurrencyRequiredException();
        currency.SetStatus(request.IsActive, UtcNow(), currentUser.UserId);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(currency);
    }

    public async Task<CurrencyResponse> SetDefaultAsync(Guid id, CancellationToken cancellationToken)
    {
        await dbContext.ExecuteInTransactionAsync(async token =>
        {
            var currency = await FindAsync(id, token);
            if (!currency.IsActive) throw new InactiveCurrencyCannotBeDefaultException(id);
            var defaults = await dbContext.Currencies.Where(candidate => candidate.IsDefault && candidate.Id != id).ToListAsync(token);
            foreach (var currentDefault in defaults) currentDefault.SetDefault(false, UtcNow(), currentUser.UserId);
            currency.SetDefault(true, UtcNow(), currentUser.UserId);
            await dbContext.SaveChangesAsync(token);
        }, cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    private async Task<Currency> FindAsync(Guid id, CancellationToken cancellationToken) => await dbContext.Currencies.SingleOrDefaultAsync(currency => currency.Id == id, cancellationToken) ?? throw new CurrencyNotFoundException(id);
    private static CurrencyResponse ToResponse(Currency currency) => new(currency.Id, currency.Code, currency.Name, currency.Symbol, currency.DecimalPlaces, currency.IsActive, currency.IsDefault, currency.CreatedAtUtc, currency.UpdatedAtUtc);
    private DateTime UtcNow() => timeProvider.GetUtcNow().UtcDateTime;
}
