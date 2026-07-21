# Architecture

## Architectural Style

Use a pragmatic layered architecture with feature-oriented organization and clear separation of concerns. Apply SOLID principles only where they make the code easier to change, test, or understand.

Suggested backend projects:

```text
src/
  Warehouse.Api/
  Warehouse.Application/
  Warehouse.Domain/
  Warehouse.Infrastructure/

tests/
  Warehouse.UnitTests/
  Warehouse.IntegrationTests/

frontend/
  warehouse-web/
```

## Project Responsibilities

### Warehouse.Domain

Contains:

- Core entities
- Value objects where useful
- Domain rules that do not depend on infrastructure
- Domain exceptions where useful
- Named business constants shared by domain logic

Must not depend on EF Core, ASP.NET Core or frontend concerns.

### Warehouse.Application

Contains:

- Use cases
- Commands and queries
- DTOs
- Validation
- Interfaces required by application logic

Avoid creating abstractions that have no concrete purpose. Application code coordinates a use case; it does not contain HTTP or persistence configuration.

### Warehouse.Infrastructure

Contains:

- EF Core DbContext
- Entity configurations
- PostgreSQL persistence
- External service implementations
- Logging or infrastructure adapters

Migrations belong here, but only the developer runs EF Core migration commands. Codex must not generate, edit, or remove migration files or the model snapshot.

### Warehouse.Api

Contains:

- HTTP endpoints
- Authentication and authorization configuration
- Dependency injection
- Middleware and Problem Details mapping
- OpenAPI configuration

Endpoints translate HTTP requests into application use cases. Exception-to-Problem-Details mapping belongs in dedicated handlers, not repeated endpoint `try`/`catch` blocks.


## Authentication and Authorization Plan

Phase 2.1 uses ASP.NET Core Identity for user/password and role persistence, and JWT bearer tokens for API authentication. It is a dedicated feature after Warehouses and before Inventory.

- Use GUID Identity keys, PostgreSQL EF Core storage, and ASP.NET Core Identity password, lockout, and unique-email protections.
- Use short-lived signed JWT access tokens with user ID and role claims.
- Use hashed, rotating refresh tokens in the database and secure HttpOnly cookies for the browser refresh flow.
- Put authentication configuration in API and Identity persistence in Infrastructure. Application use cases depend only on the current-user abstraction when required.
- Define authorization policies by capability, then map initial `admin`, `manager`, and `operator` roles to those policies. Do not scatter role-name comparisons through endpoints.
- Keep secrets out of source control; development bootstrap credentials and signing keys use user secrets or environment variables.
## Frontend Structure

Suggested structure:

```text
src/
  app/
  components/
  features/
    products/
      api/
      components/
      pages/
      constants.ts
      types/
    warehouses/
    inventory/
    purchasing/
    sales/
  layouts/
  shared/
```

Feature API modules own endpoint paths and request serialization. Feature hooks own TanStack Query keys, queries, mutations, and cache invalidation. Pages compose hooks and components; presentation components do not call `fetch` directly.

## Main Backend Patterns

- Feature-oriented use cases
- Thin endpoints
- Explicit business operations
- EF Core directly through a scoped DbContext
- Dedicated exception handlers for Problem Details and stable machine-readable error codes
- Transactions for inventory workflows
- Pagination for large lists
- Optimistic concurrency for inventory balances
- Named constants for business limits and magic values

## Main Frontend Patterns

- TanStack Query for queries, mutations and cache invalidation
- Ant Design for layout, forms, modals, tables, notifications and navigation
- Route-level pages
- Feature-specific API hooks
- Server-side filtering, sorting and pagination for large tables
- Reusable error and loading components
- Centralized feature constants and endpoint paths
- Environment configuration for deploy-specific values; no hardcoded origins or credentials
- English and French JSON locale resources; all display text is referenced by translation key

## Dependency Rules

- Domain depends on nothing.
- Application may depend on Domain.
- Infrastructure may depend on Application and Domain.
- API may depend on Application and Infrastructure.
- Frontend communicates only through the HTTP API.
- New packages must be current, compatible, justified, and recorded in the implementation report.

## Auditable Entity and Audit-Event Convention

Audit is an opt-in, property-level history subsystem for business entities. It uses side tables rather than EF Core table inheritance or one generic audit table.

### Opt-in

- `PersistentEntity` provides normal persistent-entity metadata (`Id`, timestamps, and GUID actor IDs); it is deliberately separate from auditing.
- Only entities decorated with `[AuditEntity]` participate. Base-class inheritance alone never enables auditing.
- Product and Warehouse are intentionally not opted in. A later business feature must document and add its own opt-in before it creates audit trails for them.
- `[AuditEntity(SnapshotOnCreate = true|false)]` configures creation snapshots; the opted-in default is `true`.
- `[AuditIgnore]` excludes a property, and `[AuditDisabled]` excludes an entity.

### Persistence and querying

Each audited entity is mapped to an append-only `<Table>_AuditTrails` table in the same schema. Rows include an audit-row ID, parent entity ID, changed timestamp, actor ID, action, property path, old/new values, correlation ID, and optional reason. History is queried on demand through a helper; it is never eager-loaded with normal entities.

### Save pipeline and transaction rules

The audit pipeline scans only attributed Added, Modified, and Deleted EF Core entries. It writes only real non-ignored changes, one `__deleted__` marker for deletes, and creation snapshots after the parent save has generated final keys. Parent and audit rows commit or roll back together, including caller-owned transactions through a savepoint. `SaveChanges`, `SaveChangesAsync`, and `acceptAllChangesOnSuccess: false` preserve normal EF Core change tracking.

### Context and sinks

`IAuditContext` provides the actor ID, correlation ID, and optional reason. HTTP requests derive it from the authenticated user and request trace; workers, CLI tools, and tests provide an explicit safe context. A single DI extension registers profiles, diffing, event creation, serialization, and the transactional PostgreSQL table writer. Additional sinks must be explicitly added and must not weaken the table audit.

### Safety and boundaries

Values use stable JSON serialization. Passwords, access tokens, refresh tokens, token hashes, credentials, secrets, security stamps, and protected data are excluded by default; `[AuditIgnore]` excludes additional fields. Owned navigations and non-GUID/composite keys are rejected until explicitly supported. Inventory movements remain business records and are not replaced by audit trails. The developer generates and applies required EF Core migrations manually; Codex never edits migrations or the model snapshot.

## Initial Data Model

Expected early entities:

- Product
- Warehouse
- InventoryBalance
- InventoryMovement
- Supplier
- PurchaseOrder
- PurchaseOrderLine
- GoodsReceipt
- GoodsReceiptLine
- Customer
- SalesOrder
- SalesOrderLine
- StockReservation
- Shipment
- ShipmentLine
- User
- AuditEntry

The model may evolve feature by feature. Do not generate all entities at the beginning.