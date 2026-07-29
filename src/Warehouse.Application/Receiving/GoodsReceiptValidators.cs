using FluentValidation;
using Warehouse.Application.Common.Errors;
using Warehouse.Application.Common.Pagination;
using Warehouse.Domain.Purchasing;
using Warehouse.Domain.Receiving;

namespace Warehouse.Application.Receiving;

public sealed class GoodsReceiptListQueryValidator : PagedRequestValidator<GoodsReceiptListQuery>
{
    public GoodsReceiptListQueryValidator()
    {
        RuleFor(query => query.PurchaseOrderNumber)
            .MaximumLength(PurchaseOrderRules.MaxNumberLength)
            .WithErrorCode(ApiErrorCodes.ValidationMaxLength);
    }
}

public sealed class GoodsReceiptInputValidator : AbstractValidator<GoodsReceiptInput>
{
    public GoodsReceiptInputValidator()
    {
        RuleFor(input => input.PurchaseOrderId)
            .NotEmpty()
            .WithErrorCode(ApiErrorCodes.ValidationRequired);
        RuleFor(input => input.PurchaseOrderVersion)
            .GreaterThanOrEqualTo(0)
            .WithErrorCode(ApiErrorCodes.ValidationInvalid);
        RuleFor(input => input.ReceivedAtUtc)
            .NotEmpty()
            .WithErrorCode(ApiErrorCodes.ValidationRequired)
            .Must(timestamp => timestamp.Kind == DateTimeKind.Utc)
            .WithErrorCode(ApiErrorCodes.ValidationInvalid);
        RuleFor(input => input.SupplierDeliveryNote)
            .MaximumLength(GoodsReceiptRules.MaxSupplierDeliveryNoteLength)
            .WithErrorCode(ApiErrorCodes.ValidationMaxLength);
        RuleFor(input => input.Notes)
            .MaximumLength(GoodsReceiptRules.MaxNotesLength)
            .WithErrorCode(ApiErrorCodes.ValidationMaxLength);
        RuleFor(input => input.Lines)
            .NotEmpty()
            .WithErrorCode(ApiErrorCodes.ValidationRequired);
        RuleForEach(input => input.Lines)
            .SetValidator(new GoodsReceiptLineInputValidator());
        RuleFor(input => input.Lines)
            .Custom((lines, context) =>
            {
                if (lines is null)
                {
                    return;
                }

                var duplicate = lines
                    .Select((line, index) => new { line.PurchaseOrderLineId, Index = index })
                    .GroupBy(line => line.PurchaseOrderLineId)
                    .SelectMany(group => group.Skip(1))
                    .FirstOrDefault();
                if (duplicate is not null)
                {
                    context.AddFailure(new FluentValidation.Results.ValidationFailure(
                        $"Lines[{duplicate.Index}].PurchaseOrderLineId",
                        "Each purchase-order line can be received only once per receipt)")
                    {
                        ErrorCode = ApiErrorCodes.GoodsReceiptDuplicatePurchaseOrderLine
                    });
                }
            });
    }
}

public sealed class GoodsReceiptLineInputValidator : AbstractValidator<GoodsReceiptLineInput>
{
    public GoodsReceiptLineInputValidator()
    {
        RuleFor(input => input.PurchaseOrderLineId)
            .NotEmpty()
            .WithErrorCode(ApiErrorCodes.ValidationRequired);
        RuleFor(input => input.AcceptedQuantity)
            .GreaterThan(0m)
            .WithErrorCode(ApiErrorCodes.ValidationInvalid);
    }
}
