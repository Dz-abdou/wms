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

public sealed class InventoryTransferInputValidator : AbstractValidator<InventoryTransferInput>
{
    public InventoryTransferInputValidator()
    {
        RuleFor(input => input.SourceWarehouseId).NotEmpty();
        RuleFor(input => input.DestinationWarehouseId).NotEmpty();
        RuleFor(input => input.DestinationWarehouseId)
            .NotEqual(input => input.SourceWarehouseId)
            .WithMessage("Source and destination warehouses must be different.");
        RuleFor(input => input.Reference).MaximumLength(InventoryTransferRules.MaxReferenceLength);
        RuleFor(input => input.Note).MaximumLength(InventoryTransferRules.MaxNoteLength);
        RuleFor(input => input.Lines).NotEmpty();
        RuleForEach(input => input.Lines).SetValidator(new InventoryTransferLineInputValidator());
        RuleFor(input => input.Lines)
            .Must(lines => lines.Select(line => line.ProductId).Distinct().Count() == lines.Count)
            .WithMessage("Each product can appear only once in a transfer.");
    }
}

public sealed class InventoryTransferLineInputValidator : AbstractValidator<InventoryTransferLineInput>
{
    public InventoryTransferLineInputValidator()
    {
        RuleFor(input => input.ProductId).NotEmpty();
        RuleFor(input => input.Quantity).GreaterThan(0m);
        RuleFor(input => input.UnitOfMeasure).NotEmpty();
        RuleFor(input => input.SourceQuantityInBase).GreaterThanOrEqualTo(0m);
        RuleFor(input => input.SourceBalanceVersion).GreaterThanOrEqualTo(0);
    }
}

public sealed class InventoryTransferCandidateQueryValidator : AbstractValidator<InventoryTransferCandidateQuery>
{
    public InventoryTransferCandidateQueryValidator()
    {
        RuleFor(query => query.SourceWarehouseId).NotEmpty();
        RuleFor(query => query.ProductId).NotEmpty();
    }
}

public sealed class InventoryTransferListQueryValidator : PagedRequestValidator<InventoryTransferListQuery>
{
    public InventoryTransferListQueryValidator()
    {
        RuleFor(query => query.Reference).MaximumLength(InventoryTransferRules.MaxReferenceLength);
        RuleFor(query => query.ToUtc).GreaterThanOrEqualTo(query => query.FromUtc)
            .When(query => query.FromUtc.HasValue && query.ToUtc.HasValue);
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

public sealed class CycleCountInputValidator : AbstractValidator<CycleCountInput>
{
    public CycleCountInputValidator()
    {
        RuleFor(input => input.WarehouseId).NotEmpty();
        RuleFor(input => input.Reference).MaximumLength(CycleCountRules.MaxReferenceLength);
        RuleFor(input => input.Note).MaximumLength(CycleCountRules.MaxNoteLength);
        RuleFor(input => input.Lines).NotEmpty();
        RuleForEach(input => input.Lines).SetValidator(new CycleCountLineInputValidator());
        RuleFor(input => input.Lines)
            .Must(lines => lines.Select(line => line.ProductId).Distinct().Count() == lines.Count)
            .WithMessage("Each product can appear only once in a cycle count.");
    }
}

public sealed class CycleCountLineInputValidator : AbstractValidator<CycleCountLineInput>
{
    public CycleCountLineInputValidator()
    {
        RuleFor(input => input.ProductId).NotEmpty();
        RuleFor(input => input.SystemQuantityInBase).GreaterThanOrEqualTo(0m);
        RuleFor(input => input.SystemBalanceVersion).GreaterThanOrEqualTo(0);
        RuleFor(input => input.CountedUnitOfMeasure).NotEmpty();
        RuleFor(input => input.CountedQuantityInUnit).GreaterThanOrEqualTo(0m);
    }
}

public sealed class CycleCountCandidateQueryValidator : AbstractValidator<CycleCountCandidateQuery>
{
    public CycleCountCandidateQueryValidator()
    {
        RuleFor(query => query.WarehouseId).NotEmpty();
        RuleFor(query => query.ProductId).NotEmpty();
    }
}

public sealed class CycleCountListQueryValidator : PagedRequestValidator<CycleCountListQuery>
{
    public CycleCountListQueryValidator()
    {
        RuleFor(query => query.Reference).MaximumLength(CycleCountRules.MaxReferenceLength);
        RuleFor(query => query.ToUtc).GreaterThanOrEqualTo(query => query.FromUtc)
            .When(query => query.FromUtc.HasValue && query.ToUtc.HasValue);
    }
}
