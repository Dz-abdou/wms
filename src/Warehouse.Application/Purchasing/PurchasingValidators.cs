using FluentValidation;
using Warehouse.Application.Common.Errors;
using Warehouse.Application.Common.Pagination;
using Warehouse.Domain.Purchasing;

namespace Warehouse.Application.Purchasing;

public sealed class SupplierProductListQueryValidator : PagedRequestValidator<SupplierProductListQuery> { }

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
        RuleFor(input => input.CurrencyCode).NotEmpty().WithErrorCode(ApiErrorCodes.ValidationRequired).Length(SupplierProductRules.CurrencyCodeLength).WithErrorCode(ApiErrorCodes.ValidationInvalid);
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
        RuleFor(input => input.CurrencyCode).NotEmpty().WithErrorCode(ApiErrorCodes.ValidationRequired).Length(SupplierProductRules.CurrencyCodeLength).WithErrorCode(ApiErrorCodes.ValidationInvalid);
    }
}

public sealed class PurchaseOrderListQueryValidator : PagedRequestValidator<PurchaseOrderListQuery> { }

public sealed class PurchaseOrderInputValidator : AbstractValidator<PurchaseOrderInput>
{
    public PurchaseOrderInputValidator()
    {
        RuleFor(input => input.SupplierId).NotEmpty().WithErrorCode(ApiErrorCodes.ValidationRequired);
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
