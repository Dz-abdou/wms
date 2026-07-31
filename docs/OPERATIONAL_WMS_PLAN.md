# Operational WMS Enhancement Plan

## Purpose

This plan evolves the current product, warehouse, supplier, inventory, and purchasing foundation into an operational WMS for a small or medium business. It prioritizes inbound receiving and stock correctness before advanced accounting, forecasting, or automation.

## How This Plan Is Used

This is the business-design companion to [the implementation roadmap](ROADMAP.md). The roadmap controls sequence; this document defines the operational details that the corresponding roadmap phase and feature specification must cover. It is not a request to build every item in one change.

Before an agent implements a phase, it must compare the relevant priority below with the current code and create or refresh the matching `docs/features/` specification. That specification records the approved vertical-slice boundary, exclusions, API contract, acceptance criteria, and tests. If this plan and a current feature specification differ, the conflict must be made explicit and resolved before code changes.

**Current next business priority:** the inventory overview, movement ledger, adjustment-document, and cycle-count slices are complete. Continue Priority 4 incrementally with an inter-warehouse transfer workflow; do not combine it with reservations or reorder planning.

## Guiding Decisions

1. A submitted purchase order is an operational record: its commercial and quantity terms must remain historically correct even when catalogue data changes later.
2. One purchase order has one destination warehouse and one currency in the first version.
3. Supplier catalogue data is current purchasing guidance; purchase-order lines snapshot the terms actually ordered.
4. Mutable operational documents use optimistic concurrency by default. A
   version token prevents stale draft writes from silently overwriting a newer
   edit; the API returns a stable conflict code and the UI guides the user to
   refresh and review. Pessimistic locking requires a documented operational
   reason and a bounded lock lifecycle.
5. Inventory movements are an append-only operational ledger. Each movement carries a source document and reason.
6. Product master data stays pragmatic. Lot, expiry, serial, and bin controls are opt-in business capabilities rather than defaults.
7. A business document is entered as a header plus a line table, not as a stack of repeated cards or form rows. The header owns shared fields; each line exposes only the inputs and operational context needed for that line.
8. A multi-line inventory action is all-or-nothing. Its API validates all lines and writes all balances/movements in one database transaction; the frontend must never emulate a batch operation with sequential single-line requests.

## Transactional Form and Table Standard

Apply this standard whenever a phase introduces purchase orders, receipts, orders, adjustments, counts, transfers, picks, shipments, or a repeatable product configuration such as UoM conversions.

| Screen state               | Required structure                                                                                                                                                                            |
| -------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Create/edit document       | Header form, then editable Ant Design Table for lines. Use `Form.List` for dynamic line state and Ant Design form controls inside cells.                                                      |
| Read-only detail           | Header/summary, read-only line table, totals when applicable, and status/audit timeline for stateful documents.                                                                               |
| Multi-line stock operation | Header includes reason and optional reference/note; lines include product, warehouse/location when applicable, direction, UoM, and quantity. One API command commits or rejects the full set. |
| UoM/package configuration  | An editable table with UoM, base quantity, fractional-quantity rule, and remove action.                                                                                                       |

The line-table specification must identify its editable columns, read-only derived columns, remove/add behavior, validation, server errors, and how each selected line is constrained by master data. Typical context includes product/SKU, supplier SKU, UoM, MOQ, available/outstanding quantity, currency, unit price, and line amount. The reusable editable-table component exposes one full-width table-footer row with an accessible `+ Add line` action; the row action column remains reserved for actions on that row, such as Remove. It must not render a detached add button below the table or repeat the add action in every row.

Every operational document line has a stable, one-based `LineNumber` (`#1`,
`#2`, and so on), unique within its parent document. It identifies that exact
line in receipts, exceptions, audit records, integrations, and support
conversations; use it as a line reference, never as a quantity. Once a line is
referenced by a downstream document or the parent is final, do not reuse its
number for a different line. A document list may instead show a **Line count**:
the aggregate number of lines on the document. Label that summary explicitly
as `Line count` / `Nombre de lignes`, not `Line number` or `Lines`.

Client validation should use the selected master-data context to prevent obvious invalid entries immediately (for example, a quantity below catalogue MOQ). The backend must repeat those checks immediately before persistence. If a line fails, it returns a 4xx validation Problem Details response containing `errors` and stable `errorCodes` for the exact nested field, such as `Lines[0].Quantity`; the frontend maps the nested property to its `Form.List` field path, translates the code, and displays the message beside that cell. A generic toast is reserved for non-field errors only.

Every create/edit page finishes with the shared form action bar: Cancel is left-aligned and returns to the relevant list or read-only detail route, while the primary Create/Save action is right-aligned. This is part of the page contract, not a feature-specific layout decision.

Use horizontal scrolling for genuinely wide operational tables rather than hiding essential fields. High-volume selectors must use server-side search rather than a fixed first-page list. For stock operations, reject or explicitly consolidate duplicate product/warehouse lines and validate the projected final quantity before committing. Frontend tests must cover adding/removing a line, line validation, derived values, and failed atomic submissions; backend tests must prove no partial persistence on failure.

Every table that exposes a row-level **Actions** column—including list, detail, and create/edit line tables—must keep that column fixed on the right with an explicit width. Its table must provide an explicit `scroll.x` width so the action remains reachable while the data columns scroll within the table wrapper. Tables without row actions do not need an empty Actions column.

## Application UI/UX Consistency Standard

The application must feel like one operational system rather than a collection of feature pages. This standard applies to every existing and future authenticated screen.

### Page layouts and return behaviour

- A list page has a consistent title/subtitle area, one shared primary `New` action when creation is available, an optional filter toolbar, and explicit loading, empty, error, and populated states. The title provides the object context; do not repeat it in the button.
- A create/edit page has a visible `Back to …` link above its heading, grouped form sections where the form is long, and the standard Cancel/Create or Save action bar at the bottom. The top return link and bottom Cancel action target the same safe route.
- A detail page has `Back to …`, title/status, contextual actions, a summary area, and related tables/timeline sections. A detail action must preserve the document's read-only/status rules.
- Back must be an explicit known route, never blind browser history. Links from a list preserve that list's current path and URL query; direct navigation or a refreshed page uses the safe feature-list fallback. A dirty form warns before leaving.
- Every create and edit workflow has a dedicated route-level page, including master data and administration. Do not use a modal as a substitute for a create/edit page.

### Actions, tables, and feedback

- One page has at most one visually primary action. Use neutral secondary actions for edit/navigation and confirmed danger actions for deactivate, cancel, submit, or other irreversible operations.
- Do not nest a router link inside a button. Shared navigation/action primitives own the accessible semantics.
- Operational ledgers are read-only. A shortcut to a related document action must be named as that action (for example `Record adjustment`), never `New movement`.
- Lists retain filters in URL query parameters where practical. Filter toolbars, pagination, empty states, and retryable error states use the shared layout instead of bespoke cards.
- Every list search/filter is executed by the backend before pagination. The frontend must not filter only the current loaded page or use Ant Design's default in-memory table filters. Shared list controls synchronize an explicit query string (`q`, `status`, `supplierId`, date range, and so on) with feature-owned API request parameters.
- A table with more columns than its content area must scroll horizontally
  inside the table wrapper; it must never create a page-level horizontal
  scrollbar. Set an explicit Ant Design `scroll.x` width when a feature knows
  its operational columns, and retain the shared table-wrapper containment.
- Every row-level Actions column is fixed to the right with an explicit width.
  Apply this equally to list tables and editable/read-only line tables; do not
  add an empty Actions column where a table has no row action.
- Choose filters from the operational decision, not from every visible field. Initial guidance: Products—SKU/name and active/category; Warehouses—code/name and active; Suppliers—code/name and active; Categories/Currencies—code/name and active where applicable; Supplier Catalogue—supplier/product/status/currency; Purchase Orders—supplier/status and, after hardening, warehouse/date; Adjustments—reason/date/reference; Movement History—product/warehouse/type/reference/date; Users—email/role.
- All copy, accessible labels, empty states, error states, and confirmation text use English/French translation keys.
- Before frontend completion, audit direct translation-key use against both
  locale files. A raw key on screen is a release-blocking localization defect.
- Every changed frontend file is formatted with the repository formatter before
  review; linting is an additional verification step, not a substitute for
  formatting.
- Feature-specific styles live in a CSS module beside the owning page or
  component. Global `styles.css` is reserved for shared application layout,
  reusable primitives, and responsive foundations.
- Frontend tests never sit beside production pages, components, or API modules.
  Store them in a dedicated directory matching ownership: `app/tests/`,
  `shared/tests/<area>/`, or `features/<feature>/tests/`.

### Navigation and visual foundations

- Navigation is grouped by business area: Master data, Inbound, Outbound, Inventory, and Administration. Administration itself is a group, not separate top-level items.
- The layout must remain usable at narrower widths as operational groups grow; group structure and active-state behaviour stay consistent across desktop and responsive navigation.
- Shared theme/layout tokens own colour, spacing, type scale, borders, cards, and responsive breakpoints. Feature code adds only feature-specific presentation.

### Future-feature definition of done

Every feature specification must state its page type, return target/fallback, action hierarchy, table/filter states, and responsive behaviour. New pages must use the shared layouts and action primitives from their first implementation. Frontend tests must prove any newly introduced shared interaction, including return destinations and dirty-form protection when applicable.

## Priority 1 — Purchase Order Hardening

### Purchase Order Header

| Field                    | Required | Reason                                                              |
| ------------------------ | -------: | ------------------------------------------------------------------- |
| `PurchaseOrderNumber`    |      Yes | Human-readable, unique document reference such as `PO-2026-000123`. |
| `SupplierId`             |      Yes | Supplier account being ordered from.                                |
| `DestinationWarehouseId` |      Yes | Determines the intended receiving location.                         |
| `CurrencyCode`           |      Yes | Enforces one currency and meaningful totals per order.              |
| `OrderDate`              |      Yes | Commercial document date.                                           |
| `ExpectedDeliveryDate`   |       No | Supports inbound planning and overdue reporting.                    |
| `SupplierReference`      |       No | Supplier acknowledgement/reference number.                          |
| `BuyerUserId`            |      Yes | Responsible purchaser.                                              |
| `Notes`                  |       No | Delivery instructions or internal notes.                            |
| `SubmittedAtUtc`         |       No | Records when the supplier-facing document became final.             |
| `Version`                |      Yes | Optimistic concurrency for draft edits.                             |

### Purchase Order Line

| Field                                           |      Required | Reason                                                                                        |
| ----------------------------------------------- | ------------: | --------------------------------------------------------------------------------------------- |
| `LineNumber`                                    |           Yes | Stable reference for supplier and receipt documents.                                          |
| `SupplierProductId`                             |           Yes | Catalogue item selected at the time of ordering.                                              |
| Product SKU/name/supplier SKU                   |           Yes | Immutable display snapshots.                                                                  |
| Purchase UoM                                    |           Yes | Unit ordered from the supplier.                                                               |
| `QuantityInBaseUnit`                            |           Yes | Immutable conversion factor; prevents later product-UoM edits from changing receipt quantity. |
| `OrderedQuantity` / `OrderedQuantityInBaseUnit` |           Yes | Original commitment in both document and stock units.                                         |
| `UnitPrice`, `CurrencyCode`                     |           Yes | Commercial snapshot.                                                                          |
| `LineAmount`                                    |           Yes | Stored or deterministically calculated with documented rounding.                              |
| `ReceivedQuantity` / `OutstandingQuantity`      | Later Phase 5 | Enables partial receipt and over-receipt prevention.                                          |
| Line note                                       |            No | Supplier or receiving clarification.                                                          |

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

The receipt create/edit workflow uses a receipt header form and an editable line table. It shows the PO line context, previously received, outstanding, receiving-now, accepted, and damaged/rejected quantities. One receipt command validates and commits all lines and their inventory effects atomically.

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

Manual adjustment must evolve into an adjustment document: shared reason/reference/note in its header and an editable line table for product, warehouse/location, direction, UoM, quantity, current balance, and resulting balance. Submitting it is all-or-nothing and creates one inventory movement per committed line; sequential frontend calls are prohibited.

### Screens

- Inventory overview: product, warehouse, on-hand quantity, last movement, and later available/reserved quantity
- Movement history: a dedicated read-only investigation screen with product/warehouse filters, movement type, quantity delta, balance after, timestamp, and later source document/reference filters. Do not combine it with a stock-changing form.
- Manual adjustments: a dedicated adjustment-document screen with its own header and editable line table. On success, take the user to the relevant filtered movement history (or later the adjustment detail), rather than embedding the history in the form.
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

| Field or relationship                      |         Required | Reason                                                                                                         |
| ------------------------------------------ | ---------------: | -------------------------------------------------------------------------------------------------------------- |
| `CustomerCode`, legal/trading name, status |              Yes | Identifies the selling account and prevents new orders for blocked customers.                                  |
| Default currency and payment terms         |               No | Commercial defaults; accounting remains out of scope.                                                          |
| `CustomerContact` records                  |               No | Sales, delivery, and accounts contacts are normally different people.                                          |
| `CustomerAddress` records                  | Yes for an order | A customer may have multiple shipping and billing addresses; snapshot the selected address on the sales order. |
| Delivery instructions and service notes    |               No | Supports gate, time-window, handling, and carrier instructions.                                                |

Do not make contact email or phone unique: shared inboxes, central switchboards, and a single contact serving more than one account are normal.

### Sales Order Header and Lines

The sales-order header needs a unique human document number, customer, order date, requested ship date, currency, shipping-address snapshot, customer reference, owner, notes, status, and optimistic concurrency token. A line needs a stable line number, product SKU/name snapshot, ordered UoM, conversion snapshot, ordered base quantity, unit price/currency where commercial capture is required, and a fulfilment status.

Start with:

```text
Draft → Submitted → Allocated → PartiallyShipped → Shipped/Closed
                  ↘ Cancelled
```

The records should retain a status history, actor, timestamp, and optional cancellation or exception reason. Do not allow edits to quantities, products, or address snapshots after the order has reached Allocated without an explicit controlled change/release workflow.

Sales-order create/edit uses a header form and editable line table. Each line shows product/reference, UoM, ordered quantity, availability context, pricing/currency and line total when commercial pricing is enabled, and an explicit remove action.

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
Inventory: Inventory overview, Movement history, Adjustments, Transfers, Counts
Administration: Users, Roles
```

Hide or disable UI actions that the signed-in user cannot perform, while retaining backend authorization as the final authority.

### List Screens

All high-volume selectors and lists use server-side search; do not limit product, supplier, warehouse, or catalogue choices to the first 100 records.

The purchase-order list should show number, supplier, status, order date, expected delivery, destination warehouse, line count, total/currency, and filters for status, supplier, date range, and warehouse.

The supplier-catalogue list should expose supplier, product, status, and currency filters.

| Screen                        | Essential information and actions                                                                                              |
| ----------------------------- | ------------------------------------------------------------------------------------------------------------------------------ |
| Product detail                | UoMs/conversions, active state, supplier options, warehouse stock, movement shortcut, optional traceability controls.          |
| Inventory overview            | On-hand, reserved, available, incoming, low-stock indicator; filter and drill down to movement history.                        |
| Adjustment / count / transfer | Reason, source/destination, quantity in clear UoM, document reference, review/confirm action, and immutable result history.    |
| Goods receipt detail          | PO progress, accepted/damaged/rejected quantity, delivery-note reference, receipt timeline, and printable receipt.             |
| Customer detail               | Contacts, shipping addresses, sales-order history, active/blocked state, and delivery notes.                                   |
| Sales-order list/detail       | Order number, customer, dates, status, allocation/pick/ship progress, shortages, address snapshot, and shipment links.         |
| Picking workspace             | Warehouse/zone, product/location, requested and picked quantity, exception reason, and scan-friendly future layout.            |
| Shipment detail               | Customer/address, carrier/tracking, lines and quantities, document timeline, confirm/cancel controls, and print/export action. |
| Dashboard                     | A small set of queue counts with deep links; it is not a replacement for operational lists.                                    |

Every list page needs loading, empty, error, and success states; server-side pagination; stable saved/filterable query parameters; and URL-accessible filters where practical. Forms need a clear read-only view after submission, server validation shown beside fields, confirmation for irreversible actions, and translated error-code feedback.

### Purchase Order Form and Detail

The create/edit view uses a header form followed by an editable line table. The table shows catalogue item, product, supplier SKU, UoM, MOQ, quantity, unit price, currency, line total, and remove action. The detail screen uses a summary panel and read-only line table, then adds totals, status timeline, received/outstanding quantities, and eventually a direct “Receive goods” action. Draft and submitted versions should be visually distinct so a user does not assume a submitted document remains freely editable.

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
