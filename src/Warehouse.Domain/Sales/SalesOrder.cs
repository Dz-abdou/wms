using Warehouse.Domain.Common;
using Warehouse.Domain.Products;

namespace Warehouse.Domain.Sales;

public sealed class SalesOrder : PersistentEntity
{
    private readonly List<SalesOrderLine> lines = [];
    private readonly List<SalesOrderStatusHistory> statusHistory = [];

    private SalesOrder(
        Guid id,
        string number,
        Guid customerId,
        string customerCode,
        string customerName,
        Guid shippingAddressId,
        Guid fulfillmentWarehouseId,
        string fulfillmentWarehouseCode,
        string fulfillmentWarehouseName,
        string currencyCode,
        DateOnly orderDate,
        DateOnly? requestedShipDate,
        string? customerReference,
        string? deliveryInstructions,
        Guid ownerUserId,
        DateTime createdAtUtc)
        : base(id, createdAtUtc, createdAtUtc, ownerUserId, ownerUserId)
    {
        Number = NormalizeRequired(number, SalesOrderRules.MaxNumberLength, nameof(number)).ToUpperInvariant();
        CustomerId = RequireId(customerId, nameof(customerId));
        CustomerCode = NormalizeRequired(customerCode, SalesOrderRules.MaxCustomerCodeLength, nameof(customerCode)).ToUpperInvariant();
        CustomerName = NormalizeRequired(customerName, SalesOrderRules.MaxCustomerNameLength, nameof(customerName));
        ShippingAddressId = RequireId(shippingAddressId, nameof(shippingAddressId));
        FulfillmentWarehouseId = RequireId(fulfillmentWarehouseId, nameof(fulfillmentWarehouseId));
        FulfillmentWarehouseCode = NormalizeRequired(fulfillmentWarehouseCode, SalesOrderRules.MaxWarehouseCodeLength, nameof(fulfillmentWarehouseCode)).ToUpperInvariant();
        FulfillmentWarehouseName = NormalizeRequired(fulfillmentWarehouseName, SalesOrderRules.MaxWarehouseNameLength, nameof(fulfillmentWarehouseName));
        CurrencyCode = NormalizeCurrencyCode(currencyCode);
        OrderDate = orderDate;
        RequestedShipDate = ValidateRequestedShipDate(orderDate, requestedShipDate);
        CustomerReference = NormalizeOptional(customerReference, SalesOrderRules.MaxCustomerReferenceLength, nameof(customerReference));
        DeliveryInstructions = NormalizeOptional(deliveryInstructions, SalesOrderRules.MaxDeliveryInstructionsLength, nameof(deliveryInstructions));
        OwnerUserId = RequireId(ownerUserId, nameof(ownerUserId));
        Status = SalesOrderStatus.Draft;
        statusHistory.Add(SalesOrderStatusHistory.Create(null, Status, createdAtUtc, ownerUserId, null));
    }

    public Guid CustomerId { get; private set; }
    public string CustomerCode { get; private set; } = null!;
    public string CustomerName { get; private set; } = null!;
    public Guid ShippingAddressId { get; private set; }
    public Guid FulfillmentWarehouseId { get; private set; }
    public string FulfillmentWarehouseCode { get; private set; } = null!;
    public string FulfillmentWarehouseName { get; private set; } = null!;
    public SalesOrderShippingAddress ShippingAddress { get; private set; } = null!;
    public string Number { get; private set; } = null!;
    public string CurrencyCode { get; private set; } = null!;
    public DateOnly OrderDate { get; private set; }
    public DateOnly? RequestedShipDate { get; private set; }
    public string? CustomerReference { get; private set; }
    public string? DeliveryInstructions { get; private set; }
    public Guid OwnerUserId { get; private set; }
    public SalesOrderStatus Status { get; private set; }
    public DateTime? SubmittedAtUtc { get; private set; }
    public int Version { get; private set; }
    public IReadOnlyCollection<SalesOrderLine> Lines => lines;
    public IReadOnlyCollection<SalesOrderStatusHistory> StatusHistory => statusHistory;

    public static SalesOrder Create(
        string number,
        Guid customerId,
        string customerCode,
        string customerName,
        Guid shippingAddressId,
        Guid fulfillmentWarehouseId,
        string fulfillmentWarehouseCode,
        string fulfillmentWarehouseName,
        SalesOrderShippingAddress shippingAddress,
        string currencyCode,
        DateOnly orderDate,
        DateOnly? requestedShipDate,
        string? customerReference,
        string? deliveryInstructions,
        Guid ownerUserId,
        DateTime createdAtUtc)
    {
        var order = new SalesOrder(Guid.NewGuid(), number, customerId, customerCode, customerName, shippingAddressId, fulfillmentWarehouseId, fulfillmentWarehouseCode, fulfillmentWarehouseName, currencyCode, orderDate, requestedShipDate, customerReference, deliveryInstructions, ownerUserId, createdAtUtc)
        {
            ShippingAddress = shippingAddress ?? throw new ArgumentNullException(nameof(shippingAddress))
        };
        return order;
    }

    public void UpdateDraft(
        Guid customerId,
        string customerCode,
        string customerName,
        Guid shippingAddressId,
        Guid fulfillmentWarehouseId,
        string fulfillmentWarehouseCode,
        string fulfillmentWarehouseName,
        SalesOrderShippingAddress shippingAddress,
        string currencyCode,
        DateOnly orderDate,
        DateOnly? requestedShipDate,
        string? customerReference,
        string? deliveryInstructions,
        int version,
        DateTime updatedAtUtc,
        Guid actorUserId)
    {
        EnsureDraft();
        EnsureUtc(updatedAtUtc);
        if (Version != version) throw new InvalidOperationException("The sales order was changed by another user.");

        CustomerId = RequireId(customerId, nameof(customerId));
        CustomerCode = NormalizeRequired(customerCode, SalesOrderRules.MaxCustomerCodeLength, nameof(customerCode)).ToUpperInvariant();
        CustomerName = NormalizeRequired(customerName, SalesOrderRules.MaxCustomerNameLength, nameof(customerName));
        ShippingAddressId = RequireId(shippingAddressId, nameof(shippingAddressId));
        FulfillmentWarehouseId = RequireId(fulfillmentWarehouseId, nameof(fulfillmentWarehouseId));
        FulfillmentWarehouseCode = NormalizeRequired(fulfillmentWarehouseCode, SalesOrderRules.MaxWarehouseCodeLength, nameof(fulfillmentWarehouseCode)).ToUpperInvariant();
        FulfillmentWarehouseName = NormalizeRequired(fulfillmentWarehouseName, SalesOrderRules.MaxWarehouseNameLength, nameof(fulfillmentWarehouseName));
        ShippingAddress = shippingAddress ?? throw new ArgumentNullException(nameof(shippingAddress));
        CurrencyCode = NormalizeCurrencyCode(currencyCode);
        OrderDate = orderDate;
        RequestedShipDate = ValidateRequestedShipDate(orderDate, requestedShipDate);
        CustomerReference = NormalizeOptional(customerReference, SalesOrderRules.MaxCustomerReferenceLength, nameof(customerReference));
        DeliveryInstructions = NormalizeOptional(deliveryInstructions, SalesOrderRules.MaxDeliveryInstructionsLength, nameof(deliveryInstructions));
        Version++;
        UpdatedAtUtc = updatedAtUtc;
        SetUpdatedByUser(actorUserId);
    }

    public void ReplaceLines(IEnumerable<SalesOrderLine> replacementLines, DateTime updatedAtUtc, Guid actorUserId)
    {
        EnsureDraft();
        EnsureUtc(updatedAtUtc);
        var normalizedLines = replacementLines?.ToList() ?? throw new ArgumentNullException(nameof(replacementLines));
        if (normalizedLines.Select(line => line.ProductId).Distinct().Count() != normalizedLines.Count)
            throw new ArgumentException("A sales order can contain each product only once.", nameof(replacementLines));

        lines.Clear();
        lines.AddRange(normalizedLines);
        UpdatedAtUtc = updatedAtUtc;
        SetUpdatedByUser(actorUserId);
    }

    public void Submit(DateTime updatedAtUtc, Guid actorUserId)
    {
        EnsureDraft();
        EnsureUtc(updatedAtUtc);
        if (lines.Count == 0) throw new InvalidOperationException("A sales order must contain at least one line before submission.");
        Status = SalesOrderStatus.Submitted;
        SubmittedAtUtc = updatedAtUtc;
        Version++;
        statusHistory.Add(SalesOrderStatusHistory.Create(SalesOrderStatus.Draft, Status, updatedAtUtc, actorUserId, null));
        UpdatedAtUtc = updatedAtUtc;
        SetUpdatedByUser(actorUserId);
    }

    public void Cancel(string? reason, DateTime updatedAtUtc, Guid actorUserId)
    {
        EnsureUtc(updatedAtUtc);
        if (Status is not (SalesOrderStatus.Draft or SalesOrderStatus.Submitted))
            throw new InvalidOperationException("This sales order cannot be cancelled in its current state.");
        var previousStatus = Status;
        Status = SalesOrderStatus.Cancelled;
        Version++;
        statusHistory.Add(SalesOrderStatusHistory.Create(previousStatus, Status, updatedAtUtc, actorUserId, NormalizeOptional(reason, SalesOrderRules.MaxStatusReasonLength, nameof(reason))));
        UpdatedAtUtc = updatedAtUtc;
        SetUpdatedByUser(actorUserId);
    }

    private void EnsureDraft()
    {
        if (Status != SalesOrderStatus.Draft) throw new InvalidOperationException("Only draft sales orders can be changed.");
    }

    private static Guid RequireId(Guid value, string name) => value == Guid.Empty ? throw new ArgumentException("An identifier is required.", name) : value;
    private static void EnsureUtc(DateTime value) { if (value.Kind != DateTimeKind.Utc) throw new ArgumentException("Timestamp must be UTC."); }
    private static string NormalizeRequired(string? value, int maxLength, string name)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized) || normalized.Length > maxLength) throw new ArgumentException("A valid value is required.", name);
        return normalized;
    }
    private static string? NormalizeOptional(string? value, int maxLength, string name)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized)) return null;
        if (normalized.Length > maxLength) throw new ArgumentOutOfRangeException(name);
        return normalized;
    }
    private static string NormalizeCurrencyCode(string? value)
    {
        var normalized = NormalizeRequired(value, SalesOrderRules.CurrencyCodeLength, nameof(value));
        if (normalized.Length != SalesOrderRules.CurrencyCodeLength || !normalized.All(char.IsAsciiLetter)) throw new ArgumentException("A valid currency code is required.", nameof(value));
        return normalized.ToUpperInvariant();
    }
    private static DateOnly? ValidateRequestedShipDate(DateOnly orderDate, DateOnly? requestedShipDate) => requestedShipDate is null || requestedShipDate >= orderDate ? requestedShipDate : throw new ArgumentOutOfRangeException(nameof(requestedShipDate));
}

public sealed class SalesOrderShippingAddress
{
    public SalesOrderShippingAddress(string label, string addressLine1, string? addressLine2, string city, string? postalCode, string countryCode, string? deliveryInstructions)
    {
        Label = label; AddressLine1 = addressLine1; AddressLine2 = addressLine2; City = city; PostalCode = postalCode; CountryCode = countryCode; DeliveryInstructions = deliveryInstructions;
    }
    public string Label { get; private set; } = null!;
    public string AddressLine1 { get; private set; } = null!;
    public string? AddressLine2 { get; private set; }
    public string City { get; private set; } = null!;
    public string? PostalCode { get; private set; }
    public string CountryCode { get; private set; } = null!;
    public string? DeliveryInstructions { get; private set; }
}

public sealed class SalesOrderLine
{
    private SalesOrderLine(int lineNumber, Guid productId, string productSku, string productName, string unitOfMeasure, decimal quantity, decimal quantityInBaseUnit)
    {
        Id = Guid.NewGuid(); LineNumber = lineNumber; ProductId = productId; ProductSku = productSku; ProductName = productName; UnitOfMeasure = unitOfMeasure; Quantity = quantity; QuantityInBaseUnit = quantityInBaseUnit; ConversionFactorToBaseUnit = quantityInBaseUnit / quantity;
    }
    public Guid Id { get; private set; }
    public int LineNumber { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductSku { get; private set; } = null!;
    public string ProductName { get; private set; } = null!;
    public string UnitOfMeasure { get; private set; } = null!;
    public decimal Quantity { get; private set; }
    public decimal QuantityInBaseUnit { get; private set; }
    public decimal ConversionFactorToBaseUnit { get; private set; }
    public static SalesOrderLine Create(int lineNumber, Product product, string? unitOfMeasure, decimal quantity)
    {
        if (lineNumber <= 0) throw new ArgumentOutOfRangeException(nameof(lineNumber));
        if (!product.TryConvertToBaseQuantity(unitOfMeasure, quantity, out var quantityInBaseUnit)) throw new ArgumentException("The sales quantity cannot be converted to the product base unit.", nameof(quantity));
        return new SalesOrderLine(lineNumber, product.Id, product.Sku, product.Name, ProductUnitOfMeasure.NormalizeUnitOfMeasure(unitOfMeasure), quantity, quantityInBaseUnit);
    }
}

public sealed class SalesOrderStatusHistory
{
    private SalesOrderStatusHistory(Guid id, SalesOrderStatus? previousStatus, SalesOrderStatus status, DateTime changedAtUtc, Guid actorUserId, string? reason)
    {
        Id = id; PreviousStatus = previousStatus; Status = status; ChangedAtUtc = changedAtUtc; ActorUserId = actorUserId; Reason = reason;
    }
    public Guid Id { get; private set; }
    public SalesOrderStatus? PreviousStatus { get; private set; }
    public SalesOrderStatus Status { get; private set; }
    public DateTime ChangedAtUtc { get; private set; }
    public Guid ActorUserId { get; private set; }
    public string? Reason { get; private set; }
    public static SalesOrderStatusHistory Create(SalesOrderStatus? previousStatus, SalesOrderStatus status, DateTime changedAtUtc, Guid actorUserId, string? reason) => new(Guid.NewGuid(), previousStatus, status, changedAtUtc, actorUserId == Guid.Empty ? throw new ArgumentException("An actor is required.", nameof(actorUserId)) : actorUserId, reason);
}

public static class SalesOrderRules
{
    public const int MaxNumberLength = 32;
    public const int MaxCustomerCodeLength = 32;
    public const int MaxCustomerNameLength = 200;
    public const int MaxWarehouseCodeLength = 32;
    public const int MaxWarehouseNameLength = 200;
    public const int MaxCustomerReferenceLength = 100;
    public const int MaxDeliveryInstructionsLength = 1000;
    public const int MaxStatusReasonLength = 500;
    public const int CurrencyCodeLength = 3;
}
