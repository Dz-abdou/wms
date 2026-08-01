using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Common.Errors;
using Warehouse.Application.Common.Identity;
using Warehouse.Application.Common.Models;
using Warehouse.Application.Common.Numbering;
using Warehouse.Application.Common.Pagination;
using Warehouse.Application.Common.Persistence;
using Warehouse.Domain.Customers;
using Warehouse.Domain.Numbering;
using Warehouse.Domain.Sales;
using WarehouseEntity = Warehouse.Domain.Warehouses.Warehouse;

namespace Warehouse.Application.Sales;

public sealed class SalesOrderService(IWarehouseDbContext dbContext, TimeProvider timeProvider, ICurrentUser currentUser, IDocumentNumberService documentNumbers)
{
    public async Task<PagedResult<SalesOrderResponse>> GetListAsync(SalesOrderListQuery query, CancellationToken cancellationToken)
    {
        var orders = dbContext.SalesOrders.AsNoTracking();
        if (query.Status is { } status) orders = orders.Where(order => order.Status == status);
        if (query.CustomerId is { } customerId) orders = orders.Where(order => order.CustomerId == customerId);
        if (query.FromOrderDate is { } fromDate) orders = orders.Where(order => order.OrderDate >= fromDate);
        if (query.ToOrderDate is { } toDate) orders = orders.Where(order => order.OrderDate <= toDate);
        var totalCount = await orders.CountAsync(cancellationToken);
        var items = await orders.OrderByDescending(order => order.CreatedAtUtc).Skip((query.Page - PaginationConstants.DefaultPage) * query.PageSize).Take(query.PageSize).Include(order => order.Lines).ToListAsync(cancellationToken);
        return new PagedResult<SalesOrderResponse>(items.Select(ToResponse).ToList(), query.Page, query.PageSize, totalCount);
    }

    public async Task<SalesOrderResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken) => ToResponse(await FindAsync(id, true, cancellationToken));

    public async Task<IReadOnlyList<SalesOrderAvailabilityResponse>> GetAvailabilityAsync(
        SalesOrderAvailabilityQuery query,
        CancellationToken cancellationToken)
    {
        await EnsureFulfillmentWarehouseAsync(query.FulfillmentWarehouseId, cancellationToken);
        var productIds = query.ProductIds.Distinct().Where(id => id != Guid.Empty).ToArray();
        if (productIds.Length == 0) return [];

        var products = await dbContext.Products.AsNoTracking()
            .Where(product => productIds.Contains(product.Id) && product.IsActive)
            .Select(product => new { product.Id, product.BaseUnitOfMeasure })
            .ToListAsync(cancellationToken);
        var quantities = await dbContext.InventoryBalances.AsNoTracking()
            .Where(balance => balance.WarehouseId == query.FulfillmentWarehouseId && productIds.Contains(balance.ProductId))
            .ToDictionaryAsync(balance => balance.ProductId, balance => balance.Quantity, cancellationToken);

        return products.Select(product => new SalesOrderAvailabilityResponse(
            product.Id,
            product.BaseUnitOfMeasure,
            quantities.GetValueOrDefault(product.Id))).ToList();
    }

    public async Task<SalesOrderResponse> CreateAsync(SalesOrderInput input, CancellationToken cancellationToken)
    {
        SalesOrderResponse? response = null;
        await dbContext.ExecuteInTransactionAsync(async token =>
        {
            var customer = await EnsureCustomerAsync(input.CustomerId, token);
            var address = await EnsureShippingAddressAsync(input.CustomerId, input.ShippingAddressId, token);
            var warehouse = await EnsureFulfillmentWarehouseAsync(input.FulfillmentWarehouseId, token);
            var currencyCode = await EnsureCurrencyAsync(input.CurrencyCode, token);
            var lines = await ResolveLinesAsync(input.Lines ?? [], token);
            var now = UtcNow();
            var actor = currentUser.UserId ?? throw new SalesOrderFieldValidationException("OwnerUserId", ApiErrorCodes.ValidationRequired, "An authenticated sales owner is required.");
            var order = SalesOrder.Create(await documentNumbers.AllocateAsync(DocumentNumberCodes.SalesOrder, now, token), customer.Id, customer.Code, customer.TradingName ?? customer.LegalName, address.Id, warehouse.Id, warehouse.Code, warehouse.Name, ToSnapshot(address), currencyCode, input.OrderDate, input.RequestedShipDate, input.CustomerReference, input.DeliveryInstructions ?? address.DeliveryInstructions ?? customer.DeliveryInstructions, actor, now);
            order.ReplaceLines(lines, now, actor);
            dbContext.SalesOrders.Add(order);
            await SaveAsync(order.Id, token);
            response = ToResponse(order);
        }, cancellationToken);
        return response ?? throw new InvalidOperationException("Sales order did not produce a result.");
    }

    public async Task<SalesOrderResponse> UpdateAsync(Guid id, SalesOrderInput input, CancellationToken cancellationToken)
    {
        var order = await FindAsync(id, false, cancellationToken);
        if (order.Status != SalesOrderStatus.Draft) throw new SalesOrderImmutableException(id);
        var version = input.Version ?? throw new SalesOrderFieldValidationException("Version", ApiErrorCodes.ValidationRequired, "A sales-order version is required when updating a draft.");
        if (version != order.Version) throw new SalesOrderConcurrencyException(id);
        var customer = await EnsureCustomerAsync(input.CustomerId, cancellationToken);
        var address = await EnsureShippingAddressAsync(input.CustomerId, input.ShippingAddressId, cancellationToken);
        var warehouse = await EnsureFulfillmentWarehouseAsync(input.FulfillmentWarehouseId, cancellationToken);
        var lines = await ResolveLinesAsync(input.Lines ?? [], cancellationToken);
        var now = UtcNow();
        var actor = currentUser.UserId ?? throw new SalesOrderFieldValidationException("OwnerUserId", ApiErrorCodes.ValidationRequired, "An authenticated sales owner is required.");
        order.UpdateDraft(customer.Id, customer.Code, customer.TradingName ?? customer.LegalName, address.Id, warehouse.Id, warehouse.Code, warehouse.Name, ToSnapshot(address), await EnsureCurrencyAsync(input.CurrencyCode, cancellationToken), input.OrderDate, input.RequestedShipDate, input.CustomerReference, input.DeliveryInstructions ?? address.DeliveryInstructions ?? customer.DeliveryInstructions, version, now, actor);
        order.ReplaceLines(lines, now, actor);
        await SaveAsync(id, cancellationToken);
        return ToResponse(order);
    }

    public async Task<SalesOrderResponse> SubmitAsync(Guid id, SalesOrderVersionInput input, CancellationToken cancellationToken)
    {
        var order = await FindAsync(id, false, cancellationToken);
        if (order.Status != SalesOrderStatus.Draft) throw new SalesOrderImmutableException(id);
        if (order.Version != input.Version) throw new SalesOrderConcurrencyException(id);
        await EnsureCustomerAsync(order.CustomerId, cancellationToken);
        order.Submit(UtcNow(), currentUser.UserId ?? throw new SalesOrderFieldValidationException("OwnerUserId", ApiErrorCodes.ValidationRequired, "An authenticated sales owner is required."));
        await SaveAsync(id, cancellationToken);
        return ToResponse(order);
    }

    public async Task<SalesOrderResponse> CancelAsync(Guid id, SalesOrderCancelInput input, CancellationToken cancellationToken)
    {
        var order = await FindAsync(id, false, cancellationToken);
        if (order.Version != input.Version) throw new SalesOrderConcurrencyException(id);
        if (order.Status is not (SalesOrderStatus.Draft or SalesOrderStatus.Submitted)) throw new SalesOrderInvalidTransitionException(id);
        order.Cancel(input.Reason, UtcNow(), currentUser.UserId ?? throw new SalesOrderFieldValidationException("OwnerUserId", ApiErrorCodes.ValidationRequired, "An authenticated sales owner is required."));
        await SaveAsync(id, cancellationToken);
        return ToResponse(order);
    }

    private async Task<SalesOrder> FindAsync(Guid id, bool noTracking, CancellationToken token) => await (noTracking ? dbContext.SalesOrders.AsNoTracking() : dbContext.SalesOrders).Include(order => order.Lines).Include(order => order.StatusHistory).SingleOrDefaultAsync(order => order.Id == id, token) ?? throw new SalesOrderNotFoundException(id);
    private async Task<Customer> EnsureCustomerAsync(Guid id, CancellationToken token)
    {
        var customer = await dbContext.Customers.SingleOrDefaultAsync(item => item.Id == id, token);
        if (customer is null || !customer.IsActive) throw new SalesOrderFieldValidationException("CustomerId", ApiErrorCodes.SalesOrderCustomerUnavailable, "The selected customer is unavailable.");
        return customer;
    }
    private async Task<CustomerAddress> EnsureShippingAddressAsync(Guid customerId, Guid addressId, CancellationToken token) => await dbContext.CustomerAddresses.SingleOrDefaultAsync(item => item.CustomerId == customerId && item.Id == addressId && item.IsShippingAddress, token) ?? throw new SalesOrderFieldValidationException("ShippingAddressId", ApiErrorCodes.SalesOrderShippingAddressUnavailable, "Select a shipping address for the selected customer.");
    private async Task<WarehouseEntity> EnsureFulfillmentWarehouseAsync(Guid id, CancellationToken token)
    {
        var warehouse = await dbContext.Warehouses.SingleOrDefaultAsync(item => item.Id == id && item.IsActive, token);
        return warehouse ?? throw new SalesOrderFieldValidationException("FulfillmentWarehouseId", ApiErrorCodes.SalesOrderFulfillmentWarehouseUnavailable, "Select an active fulfilment warehouse.");
    }
    private async Task<string> EnsureCurrencyAsync(string? currencyCode, CancellationToken token)
    {
        var normalized = currencyCode?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalized) || !await dbContext.Currencies.AnyAsync(item => item.Code == normalized && item.IsActive, token)) throw new SalesOrderFieldValidationException("CurrencyCode", ApiErrorCodes.SalesOrderCurrencyUnavailable, "The selected currency is unavailable.");
        return normalized;
    }
    private async Task<IReadOnlyList<SalesOrderLine>> ResolveLinesAsync(IReadOnlyList<SalesOrderLineInput> inputs, CancellationToken token)
    {
        var duplicate = inputs.Select((input, index) => (input, index)).GroupBy(item => item.input.ProductId).SelectMany(group => group.Skip(1)).FirstOrDefault();
        if (duplicate.input is not null) throw new SalesOrderFieldValidationException($"Lines[{duplicate.index}].ProductId", ApiErrorCodes.SalesOrderDuplicateProduct, "Each product can be selected only once.");
        var products = await dbContext.Products.Where(product => inputs.Select(input => input.ProductId).Contains(product.Id)).ToDictionaryAsync(product => product.Id, token);
        var lines = new List<SalesOrderLine>();
        foreach (var (input, index) in inputs.Select((input, index) => (input, index)))
        {
            if (!products.TryGetValue(input.ProductId, out var product) || !product.IsActive) throw new SalesOrderFieldValidationException($"Lines[{index}].ProductId", ApiErrorCodes.SalesOrderProductUnavailable, "The selected product is unavailable.");
            try { lines.Add(SalesOrderLine.Create(index + 1, product, input.UnitOfMeasure, input.Quantity)); }
            catch (ArgumentException) { throw new SalesOrderFieldValidationException($"Lines[{index}].Quantity", ApiErrorCodes.SalesOrderQuantityUnitInvalid, "The quantity is not valid for the selected unit."); }
        }
        return lines;
    }
    private async Task SaveAsync(Guid id, CancellationToken token) { try { await dbContext.SaveChangesAsync(token); } catch (DbUpdateConcurrencyException exception) { throw new SalesOrderConcurrencyException(id, exception); } }
    private static SalesOrderShippingAddress ToSnapshot(CustomerAddress address) => new(address.Label, address.AddressLine1, address.AddressLine2, address.City, address.PostalCode, address.CountryCode, address.DeliveryInstructions);
    private static SalesOrderResponse ToResponse(SalesOrder order) => new(order.Id, order.Number, order.CustomerId, order.CustomerCode, order.CustomerName, order.ShippingAddressId, new SalesOrderAddressResponse(order.ShippingAddress.Label, order.ShippingAddress.AddressLine1, order.ShippingAddress.AddressLine2, order.ShippingAddress.City, order.ShippingAddress.PostalCode, order.ShippingAddress.CountryCode, order.ShippingAddress.DeliveryInstructions), order.FulfillmentWarehouseId, order.FulfillmentWarehouseCode, order.FulfillmentWarehouseName, order.CurrencyCode, order.OrderDate, order.RequestedShipDate, order.CustomerReference, order.DeliveryInstructions, order.OwnerUserId, order.Status, order.Lines.Select(line => new SalesOrderLineResponse(line.Id, line.LineNumber, line.ProductId, line.ProductSku, line.ProductName, line.UnitOfMeasure, line.Quantity, line.QuantityInBaseUnit, line.ConversionFactorToBaseUnit)).ToList(), order.Version, order.SubmittedAtUtc, order.StatusHistory.Select(item => new SalesOrderStatusHistoryResponse(item.Id, item.PreviousStatus, item.Status, item.ChangedAtUtc, item.ActorUserId, item.Reason)).ToList(), order.CreatedAtUtc, order.UpdatedAtUtc);
    private DateTime UtcNow() => timeProvider.GetUtcNow().UtcDateTime;
}
