using FluentValidation;
using Warehouse.Application.Common.Pagination;
using Warehouse.Domain.Inventory;

namespace Warehouse.Application.Inventory;

public sealed class InventoryAdjustmentInputValidator : AbstractValidator<InventoryAdjustmentInput>
{
    public InventoryAdjustmentInputValidator()
    {
        RuleFor(input => input.Reason).IsInEnum();
        RuleFor(input => input.Reference).MaximumLength(InventoryAdjustmentRules.MaxReferenceLength);
        RuleFor(input => input.Note).MaximumLength(InventoryAdjustmentRules.MaxNoteLength);
        RuleFor(input => input.Lines).NotEmpty();
        RuleForEach(input => input.Lines).SetValidator(new InventoryAdjustmentLineInputValidator());
        RuleFor(input => input.Lines).Must(lines => lines.Select(line => new { line.ProductId, line.WarehouseId }).Distinct().Count() == lines.Count)
            .WithMessage("Each product and warehouse combination can appear only once.");
    }
}

public sealed class InventoryAdjustmentLineInputValidator : AbstractValidator<InventoryAdjustmentLineInput>
{
    public InventoryAdjustmentLineInputValidator()
    {
        RuleFor(input => input.ProductId).NotEmpty();
        RuleFor(input => input.WarehouseId).NotEmpty();
        RuleFor(input => input.Quantity).GreaterThan(0m);
        RuleFor(input => input.UnitOfMeasure).NotEmpty();
        RuleFor(input => input.Direction).IsInEnum();
    }
}

public sealed class InventoryMovementListQueryValidator : PagedRequestValidator<InventoryMovementListQuery>
{
    public InventoryMovementListQueryValidator()
    {
        RuleFor(query => query.Type).IsInEnum().When(query => query.Type.HasValue);
        RuleFor(query => query.Reference).MaximumLength(InventoryAdjustmentRules.MaxReferenceLength);
        RuleFor(query => query.ToUtc).GreaterThanOrEqualTo(query => query.FromUtc)
            .When(query => query.FromUtc.HasValue && query.ToUtc.HasValue);
    }
}

public sealed class InventoryAdjustmentListQueryValidator : PagedRequestValidator<InventoryAdjustmentListQuery>
{
    public InventoryAdjustmentListQueryValidator()
    {
        RuleFor(query => query.Reason).IsInEnum().When(query => query.Reason.HasValue);
        RuleFor(query => query.Reference).MaximumLength(InventoryAdjustmentRules.MaxReferenceLength);
        RuleFor(query => query.ToUtc).GreaterThanOrEqualTo(query => query.FromUtc)
            .When(query => query.FromUtc.HasValue && query.ToUtc.HasValue);
    }
}

public sealed class InventoryOverviewQueryValidator : PagedRequestValidator<InventoryOverviewQuery>
{
    public InventoryOverviewQueryValidator()
    {
        RuleFor(query => query.Search).MaximumLength(200);
    }
}
