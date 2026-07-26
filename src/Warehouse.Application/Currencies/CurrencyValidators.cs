using FluentValidation;
using Warehouse.Application.Common.Errors;
using Warehouse.Application.Common.Pagination;
using Warehouse.Domain.Currencies;

namespace Warehouse.Application.Currencies;

public sealed class CurrencyListQueryValidator : PagedRequestValidator<CurrencyListQuery>
{
    public CurrencyListQueryValidator()
    {
        RuleFor(query => query.Search).MaximumLength(200)
            .WithErrorCode(ApiErrorCodes.ValidationMaxLength);
    }
}

public sealed class CurrencyInputValidator : AbstractValidator<CurrencyInput>
{
    public CurrencyInputValidator()
    {
        RuleFor(input => input.Code).NotEmpty().WithErrorCode(ApiErrorCodes.ValidationRequired).Length(CurrencyRules.CodeLength).WithErrorCode(ApiErrorCodes.ValidationInvalid).Must(code => code is not null && code.Trim().All(char.IsAsciiLetter)).WithErrorCode(ApiErrorCodes.ValidationInvalid);
        RuleFor(input => input.Name).NotEmpty().WithErrorCode(ApiErrorCodes.ValidationRequired).MaximumLength(CurrencyRules.MaxNameLength).WithErrorCode(ApiErrorCodes.ValidationMaxLength);
        RuleFor(input => input.Symbol).MaximumLength(CurrencyRules.MaxSymbolLength).WithErrorCode(ApiErrorCodes.ValidationMaxLength);
        RuleFor(input => input.DecimalPlaces).InclusiveBetween(0, CurrencyRules.MaxDecimalPlaces).WithErrorCode(ApiErrorCodes.ValidationInvalid);
    }
}

public sealed class UpdateCurrencyInputValidator : AbstractValidator<UpdateCurrencyInput>
{
    public UpdateCurrencyInputValidator()
    {
        RuleFor(input => input.Name).NotEmpty().WithErrorCode(ApiErrorCodes.ValidationRequired).MaximumLength(CurrencyRules.MaxNameLength).WithErrorCode(ApiErrorCodes.ValidationMaxLength);
        RuleFor(input => input.Symbol).MaximumLength(CurrencyRules.MaxSymbolLength).WithErrorCode(ApiErrorCodes.ValidationMaxLength);
        RuleFor(input => input.DecimalPlaces).InclusiveBetween(0, CurrencyRules.MaxDecimalPlaces).WithErrorCode(ApiErrorCodes.ValidationInvalid);
    }
}
