using FluentValidation;
using Warehouse.Application.Common.Pagination;

namespace Warehouse.Application.Inventory;

public sealed class InventoryAdjustmentInputValidator : AbstractValidator<InventoryAdjustmentInput>
{
    public InventoryAdjustmentInputValidator()
    {
        RuleFor(input => input.ProductId).NotEmpty();
        RuleFor(input => input.WarehouseId).NotEmpty();
        RuleFor(input => input.Quantity).GreaterThan(0m);
        RuleFor(input => input.Direction).IsInEnum();
    }
}

public sealed class InventoryMovementListQueryValidator : PagedRequestValidator<InventoryMovementListQuery>;
