using Warehouse.Domain.Common;

namespace Warehouse.Domain.Customers;

public sealed class CustomerContact : PersistentEntity
{
    private CustomerContact(
        Guid id,
        Guid customerId,
        string name,
        string? role,
        string? email,
        string? phoneNumber,
        DateTime createdAtUtc,
        DateTime updatedAtUtc,
        Guid? createdByUserId,
        Guid? updatedByUserId)
        : base(id, createdAtUtc, updatedAtUtc, createdByUserId, updatedByUserId)
    {
        CustomerId = customerId;
        Name = name;
        Role = role;
        Email = email;
        PhoneNumber = phoneNumber;
    }

    public Guid CustomerId { get; private set; }

    public string Name { get; private set; } = null!;

    public string? Role { get; private set; }

    public string? Email { get; private set; }

    public string? PhoneNumber { get; private set; }

    public static CustomerContact Create(
        Guid customerId,
        string? name,
        string? role,
        string? email,
        string? phoneNumber,
        DateTime createdAtUtc,
        Guid? actorUserId = null)
    {
        if (customerId == Guid.Empty)
        {
            throw new ArgumentOutOfRangeException(nameof(customerId));
        }

        Customer.EnsureUtc(createdAtUtc);
        return new CustomerContact(
            Guid.NewGuid(),
            customerId,
            Customer.NormalizeRequired(name, CustomerContactRules.MaxNameLength, "Customer contact name"),
            Customer.NormalizeOptional(role, CustomerContactRules.MaxRoleLength, "Customer contact role"),
            Customer.NormalizeOptional(email, CustomerContactRules.MaxEmailLength, "Customer contact email"),
            Customer.NormalizeOptional(phoneNumber, CustomerContactRules.MaxPhoneNumberLength, "Customer contact phone number"),
            createdAtUtc,
            createdAtUtc,
            actorUserId,
            actorUserId);
    }

    public void Update(
        string? name,
        string? role,
        string? email,
        string? phoneNumber,
        DateTime updatedAtUtc,
        Guid? actorUserId = null)
    {
        Customer.EnsureUtc(updatedAtUtc);
        var normalizedName = Customer.NormalizeRequired(name, CustomerContactRules.MaxNameLength, "Customer contact name");
        var normalizedRole = Customer.NormalizeOptional(role, CustomerContactRules.MaxRoleLength, "Customer contact role");
        var normalizedEmail = Customer.NormalizeOptional(email, CustomerContactRules.MaxEmailLength, "Customer contact email");
        var normalizedPhoneNumber = Customer.NormalizeOptional(phoneNumber, CustomerContactRules.MaxPhoneNumberLength, "Customer contact phone number");

        if (Name == normalizedName && Role == normalizedRole && Email == normalizedEmail && PhoneNumber == normalizedPhoneNumber)
        {
            return;
        }

        Name = normalizedName;
        Role = normalizedRole;
        Email = normalizedEmail;
        PhoneNumber = normalizedPhoneNumber;
        UpdatedAtUtc = updatedAtUtc;
        SetUpdatedByUser(actorUserId);
    }
}

public static class CustomerContactRules
{
    public const int MaxNameLength = 200;
    public const int MaxRoleLength = 100;
    public const int MaxEmailLength = 320;
    public const int MaxPhoneNumberLength = 50;
}
