# Implementation Roadmap

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

- `IAuditableEntity`/`AuditableEntity`, `[AuditEntity]`, `[AuditIgnore]`, `[AuditDisabled]`, and explicit per-entity sink-routing customization.
- Per-entity `<Table>_AuditTrails` mappings in the same PostgreSQL schema; no generic audit table or EF inheritance table.
- Audit rows containing generated audit ID, parent ID, database transaction timestamp, actor ID, action, property path, safely serialized old/new values, correlation ID, and optional reason.
- A two-phase save pipeline covering synchronous and asynchronous saves: update/delete diffs before parent persistence, creation snapshots after generated keys are final.
- Transaction handling that commits or rolls back parent and audit rows together, respects caller-owned transactions, and preserves `acceptAllChangesOnSuccess: false` semantics.
- `IAuditContext` implementations for HTTP, workers, CLI tools, and tests.
- One DI registration extension, default table sink, router, profile provider, diff engine, event factory, and optional explicitly routed sinks.
- On-demand audit-history query helpers; normal entity queries never eager-load audit trails.
- PostgreSQL integration tests for creation snapshots, updates, deletes, ignored/disabled rules, metadata, rollback, and asynchronous saves.

### Guardrails

- Audit only opted-in entities and properties; redact or ignore passwords, tokens, hashes, and protected/sensitive data.
- Validate GUID, composite-key, inheritance, owned-type, and shadow-property behavior before finalizing the mapping approach.
- Do not copy reference-project namespaces or types; reuse WMS conventions and clean-architecture boundaries.
- Do not replace inventory movements with audit trails.
- The developer generates and applies the required EF Core migration manually; Codex does not edit migrations or the model snapshot.

### Exit Criteria

- Opted-in Product and Warehouse changes produce correct, queryable per-entity history.
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

## Phase 4 — Suppliers and Purchase Orders

### Deliverables

- Supplier management
- Draft purchase order
- Purchase order lines
- Submit purchase order
- Purchase order details
- Purchase order status tracking

### Exit Criteria

- Valid purchase order can be created and submitted.
- Invalid quantities are rejected.
- Submitted orders cannot be freely changed.

---

## Phase 5 — Goods Receipts

### Deliverables

- Receive purchase order partially
- Receive remaining quantities later
- Prevent over-receipt
- Update inventory
- Create inventory movements
- Complete purchase order when fully received

### Exit Criteria

- Partial and complete receipt workflows pass.
- Receipt and inventory changes are atomic.
- Automated tests pass.

---

## Phase 6 — Customers and Sales Orders

### Deliverables

- Customer management
- Draft sales order
- Sales order lines
- Submit sales order
- Order status tracking

---

## Phase 7 — Reservations and Shipping

### Deliverables

- Reserve available stock
- Reject insufficient stock
- Release reservation when required
- Confirm shipment
- Reduce inventory
- Create shipment movements

### Exit Criteria

- Reserved stock cannot be double allocated.
- Shipment updates all related records atomically.
- Automated tests pass.

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
16. Audit logs
17. Dashboard
18. Deployment and documentation
