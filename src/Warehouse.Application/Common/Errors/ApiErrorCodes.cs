namespace Warehouse.Application.Common.Errors;

public static class ApiErrorCodes
{
    public const string SystemUnexpected = "system.unexpected";
    public const string ValidationFailed = "validation.failed";
    public const string ValidationMaxLength = "validation.max_length";
    public const string ValidationRequired = "validation.required";
    public const string ProductNotFound = "product.not_found";
    public const string ProductSkuConflict = "product.sku_conflict";
}