using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Common.Identity;
using Warehouse.Application.Common.Models;
using Warehouse.Application.Common.Pagination;
using Warehouse.Application.Common.Persistence;
using Warehouse.Domain.Customers;

namespace Warehouse.Application.Customers;

public sealed class CustomerService(IWarehouseDbContext dbContext, TimeProvider timeProvider, ICurrentUser currentUser)
{
    public async Task<PagedResult<CustomerListItemResponse>> GetListAsync(CustomerListQuery query, CancellationToken cancellationToken)
    {
        var customers = dbContext.Customers.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToUpperInvariant();
            customers = customers.Where(customer => customer.Code.ToUpper().Contains(search) || customer.LegalName.ToUpper().Contains(search) || (customer.TradingName != null && customer.TradingName.ToUpper().Contains(search)));
        }
        if (query.IsActive is { } isActive) customers = customers.Where(customer => customer.IsActive == isActive);

        var totalCount = await customers.CountAsync(cancellationToken);
        var items = await customers.OrderBy(customer => customer.Code).ThenBy(customer => customer.LegalName)
            .Skip((query.Page - PaginationConstants.DefaultPage) * query.PageSize).Take(query.PageSize)
            .Select(customer => new CustomerListItemResponse(customer.Id, customer.Code, customer.LegalName, customer.TradingName, customer.DefaultCurrencyCode, customer.IsActive))
            .ToListAsync(cancellationToken);
        return new PagedResult<CustomerListItemResponse>(items, query.Page, query.PageSize, totalCount);
    }

    public async Task<CustomerResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var customer = await FindCustomerAsync(id, true, cancellationToken);
        return await ToResponseAsync(customer, cancellationToken);
    }

    public async Task<CustomerResponse> CreateAsync(CustomerInput input, CancellationToken cancellationToken)
    {
        var customer = Customer.Create(input.Code, input.LegalName, input.TradingName, input.DefaultCurrencyCode, input.DeliveryInstructions, input.ServiceNotes, UtcNow(), currentUser.UserId);
        await EnsureDefaultCurrencyIsActiveAsync(customer.DefaultCurrencyCode, cancellationToken);
        await EnsureCodeAvailableAsync(customer.Code, null, cancellationToken);
        dbContext.Customers.Add(customer);
        await SaveCustomerAsync(customer.Code, cancellationToken);
        return await ToResponseAsync(customer, cancellationToken);
    }

    public async Task<CustomerResponse> UpdateAsync(Guid id, CustomerInput input, CancellationToken cancellationToken)
    {
        var customer = await FindCustomerAsync(id, false, cancellationToken);
        var code = Customer.NormalizeCode(input.Code);
        await EnsureCodeAvailableAsync(code, id, cancellationToken);
        var currencyCode = Customer.NormalizeOptionalCurrencyCode(input.DefaultCurrencyCode);
        await EnsureDefaultCurrencyIsActiveAsync(currencyCode, cancellationToken);
        customer.Update(code, input.LegalName, input.TradingName, currencyCode, input.DeliveryInstructions, input.ServiceNotes, UtcNow(), currentUser.UserId);
        await SaveCustomerAsync(code, cancellationToken);
        return await ToResponseAsync(customer, cancellationToken);
    }

    public async Task<CustomerResponse> SetStatusAsync(Guid id, SetCustomerStatusRequest request, CancellationToken cancellationToken)
    {
        var customer = await FindCustomerAsync(id, false, cancellationToken);
        customer.SetStatus(request.IsActive, UtcNow(), currentUser.UserId);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await ToResponseAsync(customer, cancellationToken);
    }

    public async Task<CustomerContactResponse> CreateContactAsync(Guid customerId, CustomerContactInput input, CancellationToken cancellationToken)
    {
        await FindCustomerAsync(customerId, false, cancellationToken);
        var contact = CustomerContact.Create(customerId, input.Name, input.Role, input.Email, input.PhoneNumber, UtcNow(), currentUser.UserId);
        dbContext.CustomerContacts.Add(contact);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(contact);
    }

    public async Task<CustomerContactResponse> UpdateContactAsync(Guid customerId, Guid contactId, CustomerContactInput input, CancellationToken cancellationToken)
    {
        var contact = await FindContactAsync(customerId, contactId, cancellationToken);
        contact.Update(input.Name, input.Role, input.Email, input.PhoneNumber, UtcNow(), currentUser.UserId);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(contact);
    }

    public async Task DeleteContactAsync(Guid customerId, Guid contactId, CancellationToken cancellationToken)
    {
        var contact = await FindContactAsync(customerId, contactId, cancellationToken);
        dbContext.CustomerContacts.Remove(contact);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<CustomerAddressResponse> CreateAddressAsync(Guid customerId, CustomerAddressInput input, CancellationToken cancellationToken)
    {
        await FindCustomerAsync(customerId, false, cancellationToken);
        var address = CustomerAddress.Create(customerId, input.Label, input.AddressLine1, input.AddressLine2, input.City, input.PostalCode, input.CountryCode, input.IsShippingAddress, input.IsBillingAddress, input.DeliveryInstructions, UtcNow(), currentUser.UserId);
        dbContext.CustomerAddresses.Add(address);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(address);
    }

    public async Task<CustomerAddressResponse> UpdateAddressAsync(Guid customerId, Guid addressId, CustomerAddressInput input, CancellationToken cancellationToken)
    {
        var address = await FindAddressAsync(customerId, addressId, cancellationToken);
        address.Update(input.Label, input.AddressLine1, input.AddressLine2, input.City, input.PostalCode, input.CountryCode, input.IsShippingAddress, input.IsBillingAddress, input.DeliveryInstructions, UtcNow(), currentUser.UserId);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(address);
    }

    public async Task DeleteAddressAsync(Guid customerId, Guid addressId, CancellationToken cancellationToken)
    {
        var address = await FindAddressAsync(customerId, addressId, cancellationToken);
        dbContext.CustomerAddresses.Remove(address);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Customer> FindCustomerAsync(Guid id, bool asNoTracking, CancellationToken cancellationToken)
    {
        var customers = asNoTracking ? dbContext.Customers.AsNoTracking() : dbContext.Customers.AsQueryable();
        return await customers.SingleOrDefaultAsync(customer => customer.Id == id, cancellationToken) ?? throw new CustomerNotFoundException(id);
    }

    private async Task<CustomerContact> FindContactAsync(Guid customerId, Guid contactId, CancellationToken cancellationToken) =>
        await dbContext.CustomerContacts.SingleOrDefaultAsync(contact => contact.CustomerId == customerId && contact.Id == contactId, cancellationToken) ?? throw new CustomerContactNotFoundException(contactId);

    private async Task<CustomerAddress> FindAddressAsync(Guid customerId, Guid addressId, CancellationToken cancellationToken) =>
        await dbContext.CustomerAddresses.SingleOrDefaultAsync(address => address.CustomerId == customerId && address.Id == addressId, cancellationToken) ?? throw new CustomerAddressNotFoundException(addressId);

    private async Task EnsureCodeAvailableAsync(string code, Guid? excludedId, CancellationToken cancellationToken)
    {
        if (await dbContext.Customers.AnyAsync(customer => customer.Code == code && customer.Id != excludedId, cancellationToken)) throw new CustomerCodeConflictException(code);
    }

    private async Task EnsureDefaultCurrencyIsActiveAsync(string? currencyCode, CancellationToken cancellationToken)
    {
        if (currencyCode is not null && !await dbContext.Currencies.AnyAsync(currency => currency.Code == currencyCode && currency.IsActive, cancellationToken))
        {
            throw new CustomerDefaultCurrencyNotSupportedException(currencyCode);
        }
    }

    private async Task SaveCustomerAsync(string code, CancellationToken cancellationToken)
    {
        try { await dbContext.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException exception) { throw new CustomerCodeConflictException(code, exception); }
    }

    private async Task<CustomerResponse> ToResponseAsync(Customer customer, CancellationToken cancellationToken)
    {
        var contacts = await dbContext.CustomerContacts.AsNoTracking().Where(contact => contact.CustomerId == customer.Id).OrderBy(contact => contact.Name).Select(contact => new CustomerContactResponse(contact.Id, contact.Name, contact.Role, contact.Email, contact.PhoneNumber)).ToListAsync(cancellationToken);
        var addresses = await dbContext.CustomerAddresses.AsNoTracking().Where(address => address.CustomerId == customer.Id).OrderBy(address => address.Label).Select(address => new CustomerAddressResponse(address.Id, address.Label, address.AddressLine1, address.AddressLine2, address.City, address.PostalCode, address.CountryCode, address.IsShippingAddress, address.IsBillingAddress, address.DeliveryInstructions)).ToListAsync(cancellationToken);
        return new CustomerResponse(customer.Id, customer.Code, customer.LegalName, customer.TradingName, customer.DefaultCurrencyCode, customer.DeliveryInstructions, customer.ServiceNotes, customer.IsActive, customer.CreatedAtUtc, customer.UpdatedAtUtc, contacts, addresses);
    }

    private DateTime UtcNow() => timeProvider.GetUtcNow().UtcDateTime;
    private static CustomerContactResponse ToResponse(CustomerContact contact) => new(contact.Id, contact.Name, contact.Role, contact.Email, contact.PhoneNumber);
    private static CustomerAddressResponse ToResponse(CustomerAddress address) => new(address.Id, address.Label, address.AddressLine1, address.AddressLine2, address.City, address.PostalCode, address.CountryCode, address.IsShippingAddress, address.IsBillingAddress, address.DeliveryInstructions);
}
