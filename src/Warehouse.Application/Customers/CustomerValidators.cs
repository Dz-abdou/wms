using FluentValidation;
using Warehouse.Application.Common.Errors;
using Warehouse.Application.Common.Pagination;
using Warehouse.Domain.Customers;

namespace Warehouse.Application.Customers;

public sealed class CustomerListQueryValidator : PagedRequestValidator<CustomerListQuery>
{
    public CustomerListQueryValidator()
    {
        RuleFor(query => query.Search).MaximumLength(200).WithErrorCode(ApiErrorCodes.ValidationMaxLength);
    }
}

public sealed class CustomerInputValidator : AbstractValidator<CustomerInput>
{
    public CustomerInputValidator()
    {
        RuleFor(input => input.Code).NotEmpty().WithErrorCode(ApiErrorCodes.ValidationRequired)
            .MaximumLength(CustomerRules.MaxCodeLength).WithErrorCode(ApiErrorCodes.ValidationMaxLength);
        RuleFor(input => input.LegalName).NotEmpty().WithErrorCode(ApiErrorCodes.ValidationRequired)
            .MaximumLength(CustomerRules.MaxLegalNameLength).WithErrorCode(ApiErrorCodes.ValidationMaxLength);
        RuleFor(input => input.TradingName).MaximumLength(CustomerRules.MaxTradingNameLength).WithErrorCode(ApiErrorCodes.ValidationMaxLength);
        RuleFor(input => input.DeliveryInstructions).MaximumLength(CustomerRules.MaxDeliveryInstructionsLength).WithErrorCode(ApiErrorCodes.ValidationMaxLength);
        RuleFor(input => input.ServiceNotes).MaximumLength(CustomerRules.MaxServiceNotesLength).WithErrorCode(ApiErrorCodes.ValidationMaxLength);
        RuleFor(input => input.DefaultCurrencyCode)
            .Must(value => string.IsNullOrWhiteSpace(value) || (value.Trim().Length == CustomerRules.CurrencyCodeLength && value.Trim().All(char.IsAsciiLetter)))
            .WithErrorCode(ApiErrorCodes.ValidationInvalid);
    }
}

public sealed class CustomerContactInputValidator : AbstractValidator<CustomerContactInput>
{
    public CustomerContactInputValidator()
    {
        RuleFor(input => input.Name).NotEmpty().WithErrorCode(ApiErrorCodes.ValidationRequired)
            .MaximumLength(CustomerContactRules.MaxNameLength).WithErrorCode(ApiErrorCodes.ValidationMaxLength);
        RuleFor(input => input.Role).MaximumLength(CustomerContactRules.MaxRoleLength).WithErrorCode(ApiErrorCodes.ValidationMaxLength);
        RuleFor(input => input.Email).MaximumLength(CustomerContactRules.MaxEmailLength).WithErrorCode(ApiErrorCodes.ValidationMaxLength);
        RuleFor(input => input.PhoneNumber).MaximumLength(CustomerContactRules.MaxPhoneNumberLength).WithErrorCode(ApiErrorCodes.ValidationMaxLength);
    }
}

public sealed class CustomerAddressInputValidator : AbstractValidator<CustomerAddressInput>
{
    public CustomerAddressInputValidator()
    {
        RuleFor(input => input.Label).NotEmpty().WithErrorCode(ApiErrorCodes.ValidationRequired)
            .MaximumLength(CustomerAddressRules.MaxLabelLength).WithErrorCode(ApiErrorCodes.ValidationMaxLength);
        RuleFor(input => input.AddressLine1).NotEmpty().WithErrorCode(ApiErrorCodes.ValidationRequired)
            .MaximumLength(CustomerAddressRules.MaxAddressLineLength).WithErrorCode(ApiErrorCodes.ValidationMaxLength);
        RuleFor(input => input.AddressLine2).MaximumLength(CustomerAddressRules.MaxAddressLineLength).WithErrorCode(ApiErrorCodes.ValidationMaxLength);
        RuleFor(input => input.City).NotEmpty().WithErrorCode(ApiErrorCodes.ValidationRequired)
            .MaximumLength(CustomerAddressRules.MaxCityLength).WithErrorCode(ApiErrorCodes.ValidationMaxLength);
        RuleFor(input => input.PostalCode).MaximumLength(CustomerAddressRules.MaxPostalCodeLength).WithErrorCode(ApiErrorCodes.ValidationMaxLength);
        RuleFor(input => input.DeliveryInstructions).MaximumLength(CustomerAddressRules.MaxDeliveryInstructionsLength).WithErrorCode(ApiErrorCodes.ValidationMaxLength);
        RuleFor(input => input.CountryCode).NotEmpty().WithErrorCode(ApiErrorCodes.ValidationRequired)
            .Length(CustomerAddressRules.CountryCodeLength).WithErrorCode(ApiErrorCodes.ValidationInvalid)
            .Must(value => value is null || value.Trim().All(char.IsAsciiLetter)).WithErrorCode(ApiErrorCodes.ValidationInvalid);
        RuleFor(input => input.IsShippingAddress).Must((input, isShipping) => isShipping || input.IsBillingAddress)
            .WithErrorCode(ApiErrorCodes.ValidationRequired);
    }
}
