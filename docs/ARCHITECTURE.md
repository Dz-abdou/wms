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