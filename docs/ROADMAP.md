# Implementation Roadmap

## How to Use This Roadmap

This is the authoritative sequence of implementation phases. Read it with [the Operational WMS Enhancement Plan](OPERATIONAL_WMS_PLAN.md), which supplies the business rules, fields, relationships, screens, and operational rationale that each roadmap phase must satisfy. The feature specification in `docs/features/` then narrows one approved vertical slice into implementation-ready acceptance criteria.

Do not treat the operational plan as permission to implement multiple phases at once. Follow the development workflow: inspect the existing code, create or refresh the next feature specification, agree scope, then complete one branch/PR before moving on.

## Current Planning State

The supplier-management and core purchase-order slices are intentionally combined on `features/Suppliers-+-Purchase-Orders` by explicit developer decision. They establish supplier records, supplier catalogue entries, and draft/submitted purchase orders; the developer owns the generated purchasing migration files.

Once that branch is reviewed, the migration is manually applied, and the slice is fully verified and merged, the next approved work is **Phase 4.1 — Purchase Order Operational Hardening**. Its purpose is to close the operational gaps before any goods-receipt work begins. In particular, Phase 5 must not start until the Phase 4.1 exit criteria pass.

The phase status must be updated when work begins or completes. A feature specification may say a slice is implemented on a branch; it is only complete after its documented verification and merge requirements are met.

## Phase 0 — Foundation

### Goal

Create a working backend, frontend, database and test environment.

### Deliverables

- .NET solution
- React TypeScript application
- PostgreSQL Docker service
- EF Core configuration
- Health endpoint
- Swagger
- Problem Details
- Serilog
- Backend unit and integration test projects
- Frontend test setup
- CI workflow
- Basic application layout

### Exit Criteria

- Backend builds.
- Frontend builds.
- PostgreSQL runs through Docker Compose.
- API can connect to PostgreSQL.
- Health check passes.
- One backend integration smoke test passes.
- One frontend component test passes.

---

## Phase 1 — Products

### Deliverables

- Create product
- Edit product
- View product
- Paginated product list
- Search by SKU or name
- Activate/deactivate product
- Unique SKU enforcement
- Backend and frontend validation

### Exit Criteria

- Product workflow works through UI and API.
- Duplicate SKU is rejected.
- Automated tests pass.

---

## Phase 1.1 — Localization and Error Contract

### Goal

Establish English/French localization and stable API error codes before another business feature adds more UI and error behavior.

### Deliverables

- English and French JSON locale resources.
- A frontend i18n provider and language selection/persistence.
- Translation keys for all existing Product UI states, forms, navigation, confirmation text, and validation messages.
- A standard API Problem Details extension named `code` for machine-readable errors.
- Field-level API validation error codes that the frontend maps to localized messages.
- Shared frontend error-feedback convention: keep lightweight client validation for UX, map server field errors inline, and show a translated notification only for non-field mutation failures.
- Product `404`, `409`, validation, and unexpected-error code mappings.
- Tests for English/French rendering and error-code translation.

### Exit Criteria

- Switching language changes all visible Product and shared-layout text without a page reload.
- The frontend never displays backend English/French error text as its primary user-facing message.
- Product API errors expose stable codes and the UI displays the matching English/French translation.
- Existing Product behavior and automated tests still pass.

---

## Phase 2 — Warehouses

### Deliverables

- Create warehouse
- Edit warehouse
- Paginated warehouse list
- Activate/deactivate warehouse
- Unique warehouse code

### Exit Criteria

- Warehouse workflow works through UI and API.
- Automated tests pass.

---


## Phase 2.1 — Authentication and RBAC

### Goal

Protect the application before inventory workflows introduce sensitive operational actions.

### Deliverables

- ASP.NET Core Identity with PostgreSQL persistence and GUID user IDs.
- JWT access-token authentication using `Microsoft.AspNetCore.Authentication.JwtBearer`.
- Rotating refresh tokens stored hashed server-side; refresh tokens are delivered in secure, HttpOnly cookies.
- Login, refresh, logout, and current-user endpoints.
- Initial roles: `admin`, `manager`, and `operator`.
- Role-based authorization policies applied to protected API endpoints.
- A bootstrap development administrator configured through development secrets/environment variables, never source code.
- Localized frontend login, session restoration, logout, protected routes, and access-denied handling.
- Unit, integration, and frontend tests for authentication and authorization boundaries.

### Security Rules

- Passwords are hashed only by ASP.NET Core Identity; they are never logged or returned.
- JWT signing keys, bootstrap credentials, and production connection strings come only from secrets/configuration.
- Access tokens are short-lived; refresh-token rotation revokes the previous token on use.
- The frontend does not infer permissions from display text: the backend remains the authorization authority.
- Every non-public endpoint requires an explicit authorization decision.

### Exit Criteria

- An anonymous request to a protected endpoint returns `401`.
- An authenticated user without the required role receives `403` with a stable API error code.
- Login, refresh, logout, expiration, and refresh-token reuse behavior are covered by automated tests.
- Products and Warehouses are protected by explicit policies before Inventory begins.

---

## Phase 2.2 — Shared Pagination Foundation

### Goal

Standardize list pagination before more business features add tables and filters.

### Deliverables

- A shared backend page request, default values, maximum page size, and validation.
- Continued use of the existing shared paged-result contract.
- A shared frontend helper for Ant Design Table pagination state and request mapping.
- Feature-local filters, sorting, query keys, endpoint paths, and table columns.
- Refactoring of Products and Warehouses to use the shared mechanics.
- Unit, integration, and frontend tests for pagination defaults, limits, and page changes.

### Guardrails

- Do not introduce a generic repository, generic CRUD service, or database abstraction.
- Do not force unrelated lists to use pagination when their data set is genuinely fixed and small.
- Keep feature-specific search/filter rules inside their feature.

### Exit Criteria

- Products and Warehouses keep their current paging behavior through the shared foundation.
- A new paginated feature can adopt the standard with minimal feature-specific code.
- Automated tests cover the shared boundaries and existing feature behavior.

---

## Phase 2.3 — Advanced Audit Foundation

### Goal

Add an opt-in, transaction-safe, property-level audit subsystem before Inventory introduces high-value operational records.

### Deliverables

- A shared `PersistentEntity` for common persistence metadata, plus explicit `[AuditEntity]`, `[AuditIgnore]`, and `[AuditDisabled]` opt-in controls. Persistent-entity inheritance alone must not create audit trails.
- Per-entity `<Table>_AuditTrails` mappings in the same PostgreSQL schema; no generic audit table or EF inheritance table.
- Audit rows containing generated audit ID, parent ID, database transaction timestamp, actor ID, action, property path, safely serialized old/new values, correlation ID, and optional reason.
- A two-phase save pipeline covering synchronous and asynchronous saves: update/delete diffs before parent persistence, creation snapshots after generated keys are final.
- Transaction handling that commits or rolls back parent and audit rows together, respects caller-owned transactions, and preserves `acceptAllChangesOnSuccess: false` semantics.
- `IAuditContext` implementations for HTTP, workers, CLI tools, and tests.
- One DI registration extension, default transactional table writer, profile provider, diff engine, event factory, and safe serializer. Additional sinks are explicit extensions, not implicit routing.
- On-demand audit-history query helpers; normal entity queries never eager-load audit trails.
- PostgreSQL integration tests for creation snapshots, updates, deletes, ignored/disabled rules, metadata, rollback, and asynchronous saves.

### Guardrails

- Audit only opted-in entities and properties; redact or ignore passwords, tokens, hashes, and protected/sensitive data.
- Validate GUID, composite-key, inheritance, owned-type, and shadow-property behavior before finalizing the mapping approach.
- Do not copy reference-project namespaces or types; reuse WMS conventions and clean-architecture boundaries.
- Do not replace inventory movements with audit trails.
- The developer generates and applies the required EF Core migration manually; Codex does not edit migrations or the model snapshot.

### Exit Criteria

- An explicitly opted-in entity produces correct, queryable per-entity history; Product and Warehouse remain non-audited until a documented business requirement opts them in.
- Parent data and audit rows are atomic for successful and failed saves.
- Creation snapshots use final database-generated keys.
- Audit context, correlation, and optional reason are retained without recording secrets.
- Unit, PostgreSQL integration, and frontend tests pass.

---

## Phase 3 — Inventory Foundation

### Deliverables

- Inventory balance per product and warehouse
- Manual positive adjustment
- Manual negative adjustment
- Inventory movement history
- Stock cannot become negative
- Transactional balance and movement update
- Optimistic concurrency handling

### Exit Criteria

- Every adjustment creates one movement.
- Failed adjustments modify nothing.
- Concurrent updates are handled safely.
- Automated tests pass.

---

## Phase 3.1 — Product Catalogue Enrichment

### Goal

Make product quantities unambiguous before purchase orders and goods receipts introduce operational stock.

### Deliverables

- Required base unit of measure (UoM), such as each, kilogram, litre, or metre.
- Product-specific UoM conversions, such as one carton containing 24 each.
- Optional net weight and gross weight with an explicit weight unit.
- Optional length, width, and height with an explicit dimension unit.
- Derived or stored volume with a documented calculation rule.
- Validation that stock movements, purchase-order lines, and receipt lines use valid product UoMs and conversions.

### Exit Criteria

- Every quantity used by operational inventory has a defined UoM.
- Product-specific conversion calculations are exact and covered by unit tests.
- Physical measurements remain optional, but retain both value and unit when supplied.
- API, persistence, and frontend tests pass.

---

## Phase 4 — Suppliers and Purchase Orders

### Deliverables

- Supplier management
- Supplier product catalogue: a many-to-many supplier/product relationship with supplier-specific SKU, preferred-supplier selection, lead time, minimum order quantity, valid purchase unit, and price/currency history or effective price.
- Draft purchase order
- Purchase order lines
- Submit purchase order
- Purchase order details
- Purchase order status tracking

### Exit Criteria

- Valid purchase order can be created and submitted.
- Invalid quantities are rejected.
- Submitted orders cannot be freely changed.
- A purchase-order line can be validated against the selected supplier's catalogue; a product is not limited to one supplier.

---

## Phase 4.1 — Purchase Order Operational Hardening

### Goal

Make submitted purchase orders reliable inbound documents before goods receipts depend on them.

### Deliverables

- Human-readable, unique purchase-order number.
- Required destination warehouse, header currency, order date, buyer, and concurrency token.
- Optional expected delivery date, supplier reference, and notes.
- Immutable submitted-line snapshots for product/supplier identifiers, UoM conversion factor, quantities, price, currency, and line amount.
- Explicit status transition/history records for Draft, Submitted, PartiallyReceived, Received/Closed, and Cancelled.
- Purchase-order list and detail views with operational filtering, totals, and status timeline.

### Exit Criteria

- Concurrent draft edits return a stable conflict rather than silently overwriting data.
- Submitted PO data remains correct after product conversion, supplier catalogue, or price changes.
- Each PO has one destination warehouse and one currency.

---

## Phase 5 — Goods Receipts

### Deliverables

- Receive purchase order partially
- Receive remaining quantities later
- Prevent over-receipt
- Update inventory
- Create inventory movements
- Complete purchase order when fully received
- Capture supplier delivery-note reference, receiver, and receipt timestamp.
- Record accepted and damaged/rejected quantity separately.
- Link every receipt inventory movement to its receipt and purchase-order line.

### Exit Criteria

- Partial and complete receipt workflows pass.
- Receipt and inventory changes are atomic.
- Automated tests pass.

---

## Phase 5.2 — Inventory Control Operations

### Deliverables

- Searchable inventory overview by product and warehouse.
- Mandatory adjustment reason and optional reference/note.
- Movement source-document references and human-readable document numbers.
- Cycle-count and inter-warehouse transfer workflows.
- Warehouse-specific reorder point, safety stock, and reorder quantity when planning is introduced.

### Exit Criteria

- Stock adjustments are explainable from reason and source records.
- Transfers create linked out/in movements without changing total company stock.
- Inactive products and warehouses cannot be adjusted through the API.

---

## Phase 5.1 — Stock Allocation and Costing

### Goal

Add lot-aware stock allocation and inventory costing after goods receipts establish the layers from which stock can be issued.

### Deliverables

- Receipt lots or inventory cost layers with remaining quantity and unit cost.
- FIFO allocation for normal stock issue flows.
- FEFO allocation when an expiry date is tracked and the business enables it.
- Weighted-average cost (WAC, also called moving weighted-average cost) for inventory valuation.
- Explicit allocation and valuation-policy configuration; do not use LIFO unless a documented business or accounting requirement requires it.
- Allocation and cost-layer history linked to receipts, reservations, and shipments.

### Exit Criteria

- An issue or shipment allocates the configured eligible stock layers deterministically.
- FIFO and FEFO never allocate unavailable stock.
- Weighted-average cost is recalculated correctly after receipts and remains traceable.
- Automated tests cover allocation ordering, expiry handling, and costing calculations.

---


## Phase 6 — Customers and Sales Orders

### Deliverables

- Customer management with active/blocked state, contacts, and multiple shipping/billing addresses.
- Human-readable, unique sales-order numbers and customer/address snapshots.
- Draft sales order, line entry, submit/cancel workflow, actor/timestamp history, and optimistic concurrency.
- Product/UoM/conversion/quantity snapshots on submitted sales-order lines.
- Requested ship date, customer reference, delivery instructions, and optional commercial currency/pricing fields.
- Explicit states: Draft, Submitted, Allocated, PartiallyShipped, Shipped/Closed, and Cancelled.
- Searchable sales-order list and detail pages with customer, status, date, and fulfilment-progress filters.

### Exit Criteria

- A blocked customer cannot receive a new submitted sales order.
- A submitted order retains its selected shipping address, product/UoM conversion, and quantities when master data later changes.
- Concurrent draft changes return a stable conflict rather than overwriting another user's work.
- Automated domain, API/persistence, and frontend workflow tests pass.

---

## Phase 7 — Reservations and Shipping

### Deliverables

- Atomically reserve available stock by sales-order line and warehouse.
- Reject insufficient stock; allow controlled partial allocation only when the business policy permits it.
- Release reservation on cancellation, controlled order change, expiry, or authorised shortage handling.
- Pick-task workspace and exception/short-pick reasons.
- Shipment header/lines, carrier/service/tracking fields, customer-address snapshot, and print/export-ready detail.
- Confirm shipment, reduce inventory, release allocations, and create linked shipment movements in one transaction.
- Status history and read-only document timeline for reservation, picking, shipment, cancellation, and override events.

### Exit Criteria

- Reserved stock cannot be double allocated.
- Shipment updates all related records atomically.
- A cancelled or failed shipment cannot reduce inventory.
- Shipment and inventory movement records identify the source sales-order line and document number.
- Automated tests pass.

---

## Phase 7.1 — Operational Support

### Deliverables

- Company profile, default currency/time zone, and transactional document-number policies.
- Role-aware UI actions, user profile/session settings, and backend authorization policies for operational roles.
- Read-only operational history views; inventory movements remain the stock ledger.
- Dashboard queues for overdue inbound, pending receipts, low stock, allocation/picking backlog, and exceptions.
- Permission-aware CSV/export and print views for stable operational documents and lists.

### Exit Criteria

- Document numbers are unique under concurrent creation.
- Operational changes are explainable by source document, reason, actor, and timestamp.
- Dashboard figures link to the filtered underlying records.

---

## Phase 7.2 — Traceability and Returns (Conditional)

### Deliverables

- Per-product lot, expiry, serial, and quality-hold settings.
- Lot/serial capture at goods receipt and preservation through stock movement, allocation, and shipment.
- Supplier returns and customer return/RMA workflows with reason, disposition, and stock movements.
- FEFO allocation, recall/traceability reports, and bin locations only after the relevant input data is reliable.

### Exit Criteria

- Traceability can follow an enabled lot/serial from receipt through shipment or return.
- A return disposition always produces an auditable inventory outcome.
- Automated tests cover enabled traceability rules without burdening ordinary non-traceable products.

---

## Phase 8 — Enterprise Features

### Deliverables

- Audit logs
- Dashboard
- Low-stock view
- CSV export
- Demo data
- Deployment

## Recommended Feature Order

Do not work on the next item before the previous item meets its definition of done:

1. Foundation
2. Products
3. Warehouses
4. Authentication and RBAC
5. Shared list pagination
6. Audit and entity metadata foundation
7. Stock adjustments
8. Inventory history
9. Suppliers
10. Purchase orders
11. Goods receipts
12. Customers
13. Sales orders
14. Reservations
15. Shipping
16. Operational support and document control
17. Conditional traceability and returns
18. Dashboard and reporting
19. Deployment and documentation
