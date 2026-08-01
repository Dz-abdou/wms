using FluentValidation;
using Warehouse.Application.Common.Errors;
using Warehouse.Application.Common.Pagination;
using Warehouse.Domain.Sales;

namespace Warehouse.Application.Sales;

public sealed class SalesOrderListQueryValidator : PagedRequestValidator<SalesOrderListQuery>
{
    public SalesOrderListQueryValidator()
    {
        RuleFor(query => query.Status).IsInEnum().When(query => query.Status.HasValue);
        RuleFor(query => query.ToOrderDate).GreaterThanOrEqualTo(query => query.FromOrderDate).When(query => query.FromOrderDate.HasValue && query.ToOrderDate.HasValue).WithErrorCode(ApiErrorCodes.ValidationInvalid);
    }
}
public sealed class SalesOrderInputValidator : AbstractValidator<SalesOrderInput>
{
    public SalesOrderInputValidator()
    {
        RuleFor(input => input.CustomerId).NotEmpty().WithErrorCode(ApiErrorCodes.ValidationRequired);
        RuleFor(input => input.ShippingAddressId).NotEmpty().WithErrorCode(ApiErrorCodes.ValidationRequired);
        RuleFor(input => input.CurrencyCode).NotEmpty().Length(SalesOrderRules.CurrencyCodeLength).Must(value => value is not null && value.Trim().All(char.IsAsciiLetter)).WithErrorCode(ApiErrorCodes.ValidationInvalid);
        RuleFor(input => input.RequestedShipDate).GreaterThanOrEqualTo(input => input.OrderDate).When(input => input.RequestedShipDate.HasValue).WithErrorCode(ApiErrorCodes.ValidationInvalid);
        RuleFor(input => input.Version).GreaterThanOrEqualTo(0).When(input => input.Version.HasValue).WithErrorCode(ApiErrorCodes.ValidationInvalid);
        RuleForEach(input => input.Lines).SetValidator(new SalesOrderLineInputValidator());
    }
}
public sealed class SalesOrderLineInputValidator : AbstractValidator<SalesOrderLineInput>
{
    public SalesOrderLineInputValidator()
    {
        RuleFor(input => input.ProductId).NotEmpty().WithErrorCode(ApiErrorCodes.ValidationRequired);
        RuleFor(input => input.UnitOfMeasure).NotEmpty().WithErrorCode(ApiErrorCodes.ValidationRequired);
        RuleFor(input => input.Quantity).GreaterThan(0).WithErrorCode(ApiErrorCodes.ValidationInvalid);
    }
}
public sealed class SalesOrderVersionInputValidator : AbstractValidator<SalesOrderVersionInput> { public SalesOrderVersionInputValidator() => RuleFor(input => input.Version).GreaterThanOrEqualTo(0).WithErrorCode(ApiErrorCodes.ValidationInvalid); }
public sealed class SalesOrderCancelInputValidator : AbstractValidator<SalesOrderCancelInput> { public SalesOrderCancelInputValidator() { RuleFor(input => input.Version).GreaterThanOrEqualTo(0).WithErrorCode(ApiErrorCodes.ValidationInvalid); RuleFor(input => input.Reason).MaximumLength(SalesOrderRules.MaxStatusReasonLength).WithErrorCode(ApiErrorCodes.ValidationMaxLength); } }
