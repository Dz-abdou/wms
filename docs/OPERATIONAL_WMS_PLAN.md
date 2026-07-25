# Operational WMS Enhancement Plan

## Purpose

This plan evolves the current product, warehouse, supplier, inventory, and purchasing foundation into an operational WMS for a small or medium business. It prioritizes inbound receiving and stock correctness before advanced accounting, forecasting, or automation.

## Guiding Decisions

1. A submitted purchase order is an operational record: its commercial and quantity terms must remain historically correct even when catalogue data changes later.
2. One purchase order has one destination warehouse and one currency in the first version.
3. Supplier catalogue data is current purchasing guidance; purchase-order lines snapshot the terms actually ordered.
4. Inventory movements are an append-only operational ledger. Each movement carries a source document and reason.
5. Product master data stays pragmatic. Lot, expiry, serial, and bin controls are opt-in business capabilities rather than defaults.

## Priority 1 — Purchase Order Hardening

### Purchase Order Header

| Field | Required | Reason |
|---|---:|---|
| `PurchaseOrderNumber` | Yes | Human-readable, unique document reference such as `PO-2026-000123`. |
| `SupplierId` | Yes | Supplier account being ordered from. |
| `DestinationWarehouseId` | Yes | Determines the intended receiving location. |
| `CurrencyCode` | Yes | Enforces one currency and meaningful totals per order. |
| `OrderDate` | Yes | Commercial document date. |
| `ExpectedDeliveryDate` | No | Supports inbound planning and overdue reporting. |
| `SupplierReference` | No | Supplier acknowledgement/reference number. |
| `BuyerUserId` | Yes | Responsible purchaser. |
| `Notes` | No | Delivery instructions or internal notes. |
| `SubmittedAtUtc` | No | Records when the supplier-facing document became final. |
| `Version` | Yes | Optimistic concurrency for draft edits. |

### Purchase Order Line

| Field | Required | Reason |
|---|---:|---|
| `LineNumber` | Yes | Stable reference for supplier and receipt documents. |
| `SupplierProductId` | Yes | Catalogue item selected at the time of ordering. |
| Product SKU/name/supplier SKU | Yes | Immutable display snapshots. |
| Purchase UoM | Yes | Unit ordered from the supplier. |
| `QuantityInBaseUnit` | Yes | Immutable conversion factor; prevents later product-UoM edits from changing receipt quantity. |
| `OrderedQuantity` / `OrderedQuantityInBaseUnit` | Yes | Original commitment in both document and stock units. |
| `UnitPrice`, `CurrencyCode` | Yes | Commercial snapshot. |
| `LineAmount` | Yes | Stored or deterministically calculated with documented rounding. |
| `ReceivedQuantity` / `OutstandingQuantity` | Later Phase 5 | Enables partial receipt and over-receipt prevention. |
| Line note | No | Supplier or receiving clarification. |

### Status and Audit

Use explicit states and transitions:

```text
Draft → Submitted → PartiallyReceived → Received/Closed
                  ↘ Cancelled
```

Maintain `PurchaseOrderStatusHistory` with previous/new status, timestamp, user, and optional reason. Draft edits require a concurrency token; a conflicting save returns `purchase_order.concurrency_conflict` rather than overwriting another manager's changes.

### Acceptance Criteria

- A submitted PO has an immutable number, supplier, destination warehouse, currency, line snapshots, and submission timestamp.
- A product conversion or catalogue price change after submission cannot alter the PO or its later receipt conversion.
- A PO cannot contain mixed currencies.
- Two simultaneous draft edits cannot silently overwrite each other.

## Priority 2 — Supplier and Supplier Catalogue

### Supplier Account

Keep the existing code/name/contact fields. Add only when operationally needed:

- Legal name and trading name
- Tax/VAT registration number
- Status with a reason: active, blocked, pending approval
- Default currency, payment terms, default lead time, and notes
- `SupplierContact` records: name, role, email, phone, preferred flag
- `SupplierAddress` records: ordering/remittance, dispatch, and return address types

Email and phone numbers should not be database-unique; shared purchasing addresses and switchboards are normal. Validate email format when it is present.

### Supplier Product Catalogue

The relationship remains many-to-many: one product can have many suppliers, and one supplier can offer many products/UoMs.

Add over time:

- `IsPreferredSupplier` per product
- `LeadTimeDays`
- Supplier product description, supplier barcode, and manufacturer part number where needed
- Availability status: active, discontinued, temporarily unavailable
- Effective price records with date range, rather than overwriting historic prices
- Quantity-break prices only when the business needs them

The catalogue is current guidance. A PO must snapshot the price, UoM, and conversion factor selected from it.

## Priority 3 — Goods Receipts

### Goods Receipt Header

- Receipt number
- Purchase order ID
- Destination warehouse ID
- Received timestamp
- Supplier delivery note/packing slip number
- Received-by user
- Optional notes

### Goods Receipt Line

- PO line ID
- Previously received, receiving-now, accepted, damaged/rejected, and outstanding quantities
- PO-line UoM and snapped conversion factor
- Optional lot, expiry, and serial data only for products configured to require them

### Rules

- A receipt cannot exceed the remaining outstanding quantity unless an explicit authorized over-receipt policy is added later.
- Accepted quantity creates inventory balance changes and inventory movements in one transaction.
- Damaged/rejected quantity does not enter normal available inventory.
- Receipt status changes the PO to `PartiallyReceived` or `Received/Closed`.

## Priority 4 — Inventory Control Operations

### Inventory Ledger and Adjustments

Add these fields to every movement as relevant:

- Source document type and ID
- Human document reference
- Reason code: initial balance, cycle count, damage, write-off, found stock, goods receipt, transfer, shipment
- Optional note

Manual adjustments should require a reason and optional reference. The service must reject inactive products and inactive warehouses, not only hide them in the UI.

### Screens

- Inventory overview: product, warehouse, on-hand quantity, last movement, and later available/reserved quantity
- Server-side search by SKU/name and warehouse
- Filters for low stock, warehouse, category, and active status
- Cycle-count workflow rather than only positive/negative manual adjustments
- Inter-warehouse transfer workflow with matched transfer-out and transfer-in movements

## Priority 5 — Master Data Expansion

### Product

Add only on demonstrated need:

- Barcode/GTIN, brand, manufacturer part number
- Storage condition and hazardous flag
- Lot-controlled, expiry-controlled, or serial-controlled flags
- Lifecycle fields: discontinued date/reason and replacement product
- Reorder point, safety stock, and reorder quantity at the `ProductWarehouse` level, not globally on Product

### Warehouse

- Physical address and time zone
- Warehouse type: main, returns, quarantine, virtual
- Responsible manager
- Default receiving and shipping areas

When operational complexity requires it, introduce:

```text
Warehouse → Zone → Aisle → Rack → Shelf → Bin
```

## Screen and Navigation Plan

### Navigation

Group the current growing header menu:

```text
Master data: Products, Warehouses, Suppliers, Supplier catalogue
Operations: Inventory, Purchase orders, Goods receipts
Administration: Users, Roles
```

Hide or disable UI actions that the signed-in user cannot perform, while retaining backend authorization as the final authority.

### List Screens

All high-volume selectors and lists use server-side search; do not limit product, supplier, warehouse, or catalogue choices to the first 100 records.

The purchase-order list should show number, supplier, status, order date, expected delivery, destination warehouse, line count, total/currency, and filters for status, supplier, date range, and warehouse.

The supplier-catalogue list should expose supplier, product, status, and currency filters.

### Purchase Order Form and Detail

The line editor should show product, supplier SKU, UoM, MOQ, quantity, unit price, currency, line total, and remove action. The detail screen should add a summary panel, totals, status timeline, received/outstanding quantities, and eventually a direct “Receive goods” action.

## Deliberately Deferred

- Supplier invoices, payment matching, tax, and full accounting
- Customer/sales flows until inbound receiving is reliable
- Advanced pricing, approval thresholds, and budget checks
- Barcode hardware integration
- Multi-company tenancy
- Lot/serial/expiry behavior for products that do not require traceability

## Recommended Delivery Order

1. Purchase-order hardening and concurrency.
2. Supplier catalogue usability, preferred supplier, lead time, and effective pricing.
3. Partial goods receipts with atomic inventory updates.
4. Inventory overview, adjustment reasons, and transfers.
5. Master-data extensions driven by real operational needs.
