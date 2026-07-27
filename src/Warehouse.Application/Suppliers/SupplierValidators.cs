using FluentValidation;
using Warehouse.Application.Common.Errors;
using Warehouse.Application.Common.Pagination;
using Warehouse.Domain.Suppliers;

namespace Warehouse.Application.Suppliers;

public sealed class SupplierListQueryValidator : PagedRequestValidator<SupplierListQuery>
{
    public SupplierListQueryValidator()
    {
        RuleFor(query => query.Search).MaximumLength(200)
            .WithErrorCode(ApiErrorCodes.ValidationMaxLength);
    }
}

public sealed class SupplierInputValidator : AbstractValidator<SupplierInput>
{
    public SupplierInputValidator()
    {
        RuleFor(input => input.Code).Must(value => !string.IsNullOrWhiteSpace(value)).WithMessage("Supplier code is required.").WithErrorCode(ApiErrorCodes.ValidationRequired)
            .Must(value => value is null || value.Trim().Length <= SupplierRules.MaxCodeLength).WithMessage($"Supplier code cannot exceed {SupplierRules.MaxCodeLength} characters.").WithErrorCode(ApiErrorCodes.ValidationMaxLength);
        RuleFor(input => input.Name).Must(value => !string.IsNullOrWhiteSpace(value)).WithMessage("Supplier name is required.").WithErrorCode(ApiErrorCodes.ValidationRequired)
            .Must(value => value is null || value.Trim().Length <= SupplierRules.MaxNameLength).WithMessage($"Supplier name cannot exceed {SupplierRules.MaxNameLength} characters.").WithErrorCode(ApiErrorCodes.ValidationMaxLength);
        RuleFor(input => input.Email).Must(value => value is null || value.Trim().Length <= SupplierRules.MaxEmailLength).WithMessage($"Supplier email cannot exceed {SupplierRules.MaxEmailLength} characters.").WithErrorCode(ApiErrorCodes.ValidationMaxLength);
        RuleFor(input => input.PhoneNumber).Must(value => value is null || value.Trim().Length <= SupplierRules.MaxPhoneNumberLength).WithMessage($"Supplier phone number cannot exceed {SupplierRules.MaxPhoneNumberLength} characters.").WithErrorCode(ApiErrorCodes.ValidationMaxLength);
        RuleFor(input => input.Address).Must(value => value is null || value.Trim().Length <= SupplierRules.MaxAddressLength).WithMessage($"Supplier address cannot exceed {SupplierRules.MaxAddressLength} characters.").WithErrorCode(ApiErrorCodes.ValidationMaxLength);
        RuleFor(input => input.DefaultCurrencyCode)
            .Must(value => value is null || (value.Trim().Length == SupplierRules.CurrencyCodeLength && value.Trim().All(char.IsAsciiLetter)))
            .WithErrorCode(ApiErrorCodes.ValidationInvalid);
    }
}
