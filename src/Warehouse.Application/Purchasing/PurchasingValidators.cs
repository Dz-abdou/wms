using FluentValidation;
using Warehouse.Application.Common.Errors;
using Warehouse.Application.Common.Pagination;
using Warehouse.Domain.Purchasing;

namespace Warehouse.Application.Purchasing;

public sealed class SupplierProductListQueryValidator : PagedRequestValidator<SupplierProductListQuery>
{
    public SupplierProductListQueryValidator()
    {
        RuleFor(query => query.CurrencyCode)
            .MaximumLength(SupplierProductRules.CurrencyCodeLength)
            .WithErrorCode(ApiErrorCodes.ValidationMaxLength)
            .Must(code => string.IsNullOrWhiteSpace(code) || code.Trim().All(char.IsAsciiLetter))
            .WithErrorCode(ApiErrorCodes.ValidationInvalid);
    }
}

public sealed class SupplierProductInputValidator : AbstractValidator<SupplierProductInput>
{
    public SupplierProductInputValidator()
    {
        RuleFor(input => input.SupplierId).NotEmpty().WithErrorCode(ApiErrorCodes.ValidationRequired);
        RuleFor(input => input.ProductId).NotEmpty().WithErrorCode(ApiErrorCodes.ValidationRequired);
        RuleFor(input => input.SupplierSku).MaximumLength(SupplierProductRules.MaxSupplierSkuLength).WithErrorCode(ApiErrorCodes.ValidationMaxLength);
        RuleFor(input => input.PurchaseUnitOfMeasure).NotEmpty().WithErrorCode(ApiErrorCodes.ValidationRequired).MaximumLength(SupplierProductRules.UnitOfMeasureLength).WithErrorCode(ApiErrorCodes.ValidationMaxLength);
        RuleFor(input => input.MinimumOrderQuantity).GreaterThan(0m).WithErrorCode(ApiErrorCodes.ValidationInvalid);
        RuleFor(input => input.UnitPrice).GreaterThanOrEqualTo(0m).WithErrorCode(ApiErrorCodes.ValidationInvalid);
        RuleFor(input => input.CurrencyCode).NotEmpty().WithErrorCode(ApiErrorCodes.ValidationRequired).Length(SupplierProductRules.CurrencyCodeLength).WithErrorCode(ApiErrorCodes.ValidationInvalid).Must(code => code is not null && code.Trim().All(char.IsAsciiLetter)).WithErrorCode(ApiErrorCodes.ValidationInvalid);
    }
}

public sealed class UpdateSupplierProductInputValidator : AbstractValidator<UpdateSupplierProductInput>
{
    public UpdateSupplierProductInputValidator()
    {
        RuleFor(input => input.SupplierSku).MaximumLength(SupplierProductRules.MaxSupplierSkuLength).WithErrorCode(ApiErrorCodes.ValidationMaxLength);
        RuleFor(input => input.PurchaseUnitOfMeasure).NotEmpty().WithErrorCode(ApiErrorCodes.ValidationRequired).MaximumLength(SupplierProductRules.UnitOfMeasureLength).WithErrorCode(ApiErrorCodes.ValidationMaxLength);
        RuleFor(input => input.MinimumOrderQuantity).GreaterThan(0m).WithErrorCode(ApiErrorCodes.ValidationInvalid);
        RuleFor(input => input.UnitPrice).GreaterThanOrEqualTo(0m).WithErrorCode(ApiErrorCodes.ValidationInvalid);
        RuleFor(input => input.CurrencyCode).NotEmpty().WithErrorCode(ApiErrorCodes.ValidationRequired).Length(SupplierProductRules.CurrencyCodeLength).WithErrorCode(ApiErrorCodes.ValidationInvalid).Must(code => code is not null && code.Trim().All(char.IsAsciiLetter)).WithErrorCode(ApiErrorCodes.ValidationInvalid);
    }
}

public sealed class PurchaseOrderListQueryValidator : PagedRequestValidator<PurchaseOrderListQuery>
{
    public PurchaseOrderListQueryValidator()
    {
        RuleFor(query => query.Status).IsInEnum().When(query => query.Status.HasValue);
        RuleFor(query => query.ToOrderDate).GreaterThanOrEqualTo(query => query.FromOrderDate).When(query => query.FromOrderDate.HasValue && query.ToOrderDate.HasValue).WithErrorCode(ApiErrorCodes.ValidationInvalid);
    }
}

public sealed class PurchaseOrderInputValidator : AbstractValidator<PurchaseOrderInput>
{
    public PurchaseOrderInputValidator()
    {
        RuleFor(input => input.SupplierId).NotEmpty().WithErrorCode(ApiErrorCodes.ValidationRequired);
        RuleFor(input => input.DestinationWarehouseId).NotEmpty().WithErrorCode(ApiErrorCodes.ValidationRequired);
        RuleFor(input => input.CurrencyCode)
            .Must(code => code is null || (code.Trim().Length == SupplierProductRules.CurrencyCodeLength && code.Trim().All(char.IsAsciiLetter)))
            .WithErrorCode(ApiErrorCodes.ValidationInvalid);
        RuleFor(input => input.ExpectedDeliveryDate).GreaterThanOrEqualTo(input => input.OrderDate).When(input => input.ExpectedDeliveryDate.HasValue).WithErrorCode(ApiErrorCodes.ValidationInvalid);
        RuleFor(input => input.Version).GreaterThanOrEqualTo(0).When(input => input.Version.HasValue).WithErrorCode(ApiErrorCodes.ValidationInvalid);
        RuleForEach(input => input.Lines).SetValidator(new PurchaseOrderLineInputValidator());
    }
}

public sealed class PurchaseOrderLineInputValidator : AbstractValidator<PurchaseOrderLineInput>
{
    public PurchaseOrderLineInputValidator()
    {
        RuleFor(input => input.SupplierProductId).NotEmpty().WithErrorCode(ApiErrorCodes.ValidationRequired);
        RuleFor(input => input.Quantity).GreaterThan(0m).WithErrorCode(ApiErrorCodes.ValidationInvalid);
    }
}

public sealed class PurchaseOrderVersionInputValidator : AbstractValidator<PurchaseOrderVersionInput>
{
    public PurchaseOrderVersionInputValidator() =>
        RuleFor(input => input.Version).GreaterThanOrEqualTo(0).WithErrorCode(ApiErrorCodes.ValidationInvalid);
}

public sealed class PurchaseOrderCancelInputValidator : AbstractValidator<PurchaseOrderCancelInput>
{
    public PurchaseOrderCancelInputValidator()
    {
        RuleFor(input => input.Version).GreaterThanOrEqualTo(0).WithErrorCode(ApiErrorCodes.ValidationInvalid);
        RuleFor(input => input.Reason).MaximumLength(PurchaseOrderRules.MaxStatusReasonLength).WithErrorCode(ApiErrorCodes.ValidationMaxLength);
    }
}
