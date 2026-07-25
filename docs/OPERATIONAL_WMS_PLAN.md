# Operational WMS Enhancement Plan

## Purpose

This plan evolves the current product, warehouse, supplier, inventory, and purchasing foundation into an operational WMS for a small or medium business. It prioritizes inbound receiving and stock correctness before advanced accounting, forecasting, or automation.

## How This Plan Is Used

This is the business-design companion to [the implementation roadmap](ROADMAP.md). The roadmap controls sequence; this document defines the operational details that the corresponding roadmap phase and feature specification must cover. It is not a request to build every item in one change.

Before an agent implements a phase, it must compare the relevant priority below with the current code and create or refresh the matching `docs/features/` specification. That specification records the approved vertical-slice boundary, exclusions, API contract, acceptance criteria, and tests. If this plan and a current feature specification differ, the conflict must be made explicit and resolved before code changes.

**Current next business priority:** after the combined Supplier and core Purchase Order branch is fully verified and merged, implement Priority 1 / Roadmap Phase 4.1, Purchase Order Operational Hardening. Do not start Priority 3 Goods Receipts until its acceptance criteria are met.

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

## Priority 6 — Outbound Fulfilment

Inbound stock is only useful when the warehouse can commit and issue it safely. Build the outbound flow after goods receipts, inventory controls, and the product/UoM rules above are dependable.

### Customer Account

| Field or relationship | Required | Reason |
|---|---:|---|
| `CustomerCode`, legal/trading name, status | Yes | Identifies the selling account and prevents new orders for blocked customers. |
| Default currency and payment terms | No | Commercial defaults; accounting remains out of scope. |
| `CustomerContact` records | No | Sales, delivery, and accounts contacts are normally different people. |
| `CustomerAddress` records | Yes for an order | A customer may have multiple shipping and billing addresses; snapshot the selected address on the sales order. |
| Delivery instructions and service notes | No | Supports gate, time-window, handling, and carrier instructions. |

Do not make contact email or phone unique: shared inboxes, central switchboards, and a single contact serving more than one account are normal.

### Sales Order Header and Lines

The sales-order header needs a unique human document number, customer, order date, requested ship date, currency, shipping-address snapshot, customer reference, owner, notes, status, and optimistic concurrency token. A line needs a stable line number, product SKU/name snapshot, ordered UoM, conversion snapshot, ordered base quantity, unit price/currency where commercial capture is required, and a fulfilment status.

Start with:

```text
Draft → Submitted → Allocated → PartiallyShipped → Shipped/Closed
                  ↘ Cancelled
```

The records should retain a status history, actor, timestamp, and optional cancellation or exception reason. Do not allow edits to quantities, products, or address snapshots after the order has reached Allocated without an explicit controlled change/release workflow.

### Reservation, Picking, and Shipment

- `InventoryReservation` must record product, warehouse, sales-order line, quantity in base unit, status, expiry/release reason, and allocation policy used.
- Available stock is `on hand - active reservations`; reservations must be atomic so two orders cannot allocate the same stock.
- The first release can create one pick task per sales-order line. Later versions can group tasks into pick waves, zones, routes, or batches.
- `Shipment` records number, warehouse, customer/address snapshot, carrier/service, tracking number, shipped timestamp, packed/shipped-by users, and status.
- `ShipmentLine` records the sales-order line, picked/shipped quantity, source reservation/allocation, and any short-ship reason.
- Confirming a shipment writes inventory movements in the same transaction, releases the matching reservation, and advances the sales-order state. A cancelled shipment must never remove stock.

## Priority 7 — Operational Support and Governance

### Company Settings and Document Control

Create an administration area for the company identity shown on documents, default currency/time zone, and per-document numbering policies for PO, receipt, sales order, transfer, and shipment numbers. Number allocation must be transactional and unique; users must not type a number that silently duplicates an existing document.

### Audit, Exceptions, and User Experience

- Expose a read-only history timeline for operational documents: who created, changed, submitted, received, allocated, shipped, cancelled, or adjusted stock and when.
- Keep inventory movements as the stock ledger; audit history explains general record changes and must not replace movements.
- Require a reason for cancellation, write-off, stock correction, short shipment, and authorised over-receipt/override.
- Provide a profile page where a user can change their own display preferences and password/session without gaining administration rights.
- Add role-aware action buttons and route guards for purchaser, receiver, inventory operator, sales operator, manager, and administrator capabilities. The API remains the authorization authority.

### Operational Visibility

The initial dashboard should be deliberately small: overdue purchase orders, receipts waiting to be processed, low-stock items, orders waiting for allocation/pick, and recent exceptions. Add server-side export/print views for document detail and filtered lists only after their columns and access permissions are stable.

## Priority 8 — Conditional Traceability and Returns

These capabilities are important in food, pharma, electronics, or regulated work, but should be enabled only for products that need them.

- Product controls: lot, expiry, serial, and quality-inspection requirement.
- `InventoryLot`/serial records: received quantity, available quantity, supplier lot, internal lot, manufacture/expiry date, status, and storage location when bins exist.
- Quality hold/quarantine: stock can be physically received but unavailable until released.
- Customer return (RMA): return number, reason, received condition, disposition (return to stock, quarantine, repair, scrap), and linked inventory movement.
- Supplier return: supplier, original receipt/PO reference, reason, approved quantity, shipment/credit reference, and stock movement.

Do not promise FEFO picking, serial validation, or recall reports until lot/serial data is collected consistently at receiving and every later inventory movement preserves the traceability chain.

## Screen and Navigation Plan

### Navigation

Group the current growing header menu:

```text
Master data: Products, Warehouses, Suppliers, Supplier catalogue
Inbound: Purchase orders, Goods receipts
Outbound: Customers, Sales orders, Picking, Shipments
Inventory: Inventory overview, Adjustments, Transfers, Counts
Administration: Users, Roles
```

Hide or disable UI actions that the signed-in user cannot perform, while retaining backend authorization as the final authority.

### List Screens

All high-volume selectors and lists use server-side search; do not limit product, supplier, warehouse, or catalogue choices to the first 100 records.

The purchase-order list should show number, supplier, status, order date, expected delivery, destination warehouse, line count, total/currency, and filters for status, supplier, date range, and warehouse.

The supplier-catalogue list should expose supplier, product, status, and currency filters.

| Screen | Essential information and actions |
|---|---|
| Product detail | UoMs/conversions, active state, supplier options, warehouse stock, movement shortcut, optional traceability controls. |
| Inventory overview | On-hand, reserved, available, incoming, low-stock indicator; filter and drill down to movement history. |
| Adjustment / count / transfer | Reason, source/destination, quantity in clear UoM, document reference, review/confirm action, and immutable result history. |
| Goods receipt detail | PO progress, accepted/damaged/rejected quantity, delivery-note reference, receipt timeline, and printable receipt. |
| Customer detail | Contacts, shipping addresses, sales-order history, active/blocked state, and delivery notes. |
| Sales-order list/detail | Order number, customer, dates, status, allocation/pick/ship progress, shortages, address snapshot, and shipment links. |
| Picking workspace | Warehouse/zone, product/location, requested and picked quantity, exception reason, and scan-friendly future layout. |
| Shipment detail | Customer/address, carrier/tracking, lines and quantities, document timeline, confirm/cancel controls, and print/export action. |
| Dashboard | A small set of queue counts with deep links; it is not a replacement for operational lists. |

Every list page needs loading, empty, error, and success states; server-side pagination; stable saved/filterable query parameters; and URL-accessible filters where practical. Forms need a clear read-only view after submission, server validation shown beside fields, confirmation for irreversible actions, and translated error-code feedback.

### Purchase Order Form and Detail

The line editor should show product, supplier SKU, UoM, MOQ, quantity, unit price, currency, line total, and remove action. The detail screen should add a summary panel, totals, status timeline, received/outstanding quantities, and eventually a direct “Receive goods” action. Draft and submitted versions should be visually distinct so a user does not assume a submitted document remains freely editable.

## Core Relationship Map

```text
Supplier ──< SupplierProduct >── Product ──< ProductWarehouse >── Warehouse
                  │                  │
                  └──< PurchaseOrderLine >── PurchaseOrder ──< GoodsReceipt
                                                    │
Customer ──< SalesOrder ──< SalesOrderLine ──< InventoryReservation
                                             │             │
                                             └──< ShipmentLine >── Shipment

All stock changes ──> InventoryMovement (source document + reason)
```

The relationship map is intentionally lean: the supplier catalogue guides purchasing, while PO and sales-order lines preserve transactional snapshots. Inventory balances are derived/maintained separately from the immutable movement ledger and must never be changed without a movement.

## Deliberately Deferred

- Supplier invoices, payment matching, tax, and full accounting
- Advanced pricing, approval thresholds, and budget checks
- Barcode hardware integration
- Multi-company tenancy
- Warehouse labour planning, route optimization, and carrier-rate shopping
- Lot/serial/expiry behavior for products that do not require traceability

## Recommended Delivery Order

1. Purchase-order hardening and concurrency.
2. Supplier catalogue usability, preferred supplier, lead time, and effective pricing.
3. Partial goods receipts with atomic inventory updates.
4. Inventory overview, adjustment reasons, cycle counts, and transfers.
5. Master-data extensions driven by real operational needs.
6. Customer accounts and controlled sales-order entry.
7. Atomic reservations, then picking and shipment confirmation.
8. Operational dashboard, audit/history views, exports, and document settings.
9. Traceability, returns, and bin locations only where the business case requires them.
