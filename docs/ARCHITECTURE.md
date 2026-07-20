# Architecture

## Architectural Style

Use a pragmatic layered architecture with feature-oriented organization.

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

Must not depend on EF Core, ASP.NET Core or frontend concerns.

### Warehouse.Application

Contains:

- Use cases
- Commands and queries
- DTOs
- Validation
- Interfaces required by application logic

Avoid creating abstractions that have no concrete purpose.

### Warehouse.Infrastructure

Contains:

- EF Core DbContext
- Entity configurations
- Migrations
- PostgreSQL persistence
- External service implementations
- Logging or infrastructure adapters

### Warehouse.Api

Contains:

- HTTP endpoints
- Authentication and authorization configuration
- Dependency injection
- Middleware
- OpenAPI configuration
- Problem Details mapping

Endpoints should translate HTTP requests into application use cases.

## Frontend Structure

Suggested structure:

```text
src/
  app/
  api/
  components/
  features/
    products/
    warehouses/
    inventory/
    purchasing/
    sales/
  layouts/
  routes/
  types/
```

Each feature may contain:

```text
features/products/
  api/
  components/
  pages/
  schemas/
  types/
```

## Main Backend Patterns

- Feature-oriented use cases
- Thin endpoints
- Explicit business operations
- EF Core directly through a scoped DbContext
- Transactions for inventory workflows
- Problem Details for errors
- Pagination for large lists
- Optimistic concurrency for inventory balances

## Main Frontend Patterns

- TanStack Query for queries, mutations and cache invalidation
- Ant Design for layout, forms, modals, tables, notifications and navigation
- Route-level pages
- Feature-specific API hooks
- Server-side filtering, sorting and pagination for large tables
- Reusable error and loading components

## Dependency Rules

- Domain depends on nothing.
- Application may depend on Domain.
- Infrastructure may depend on Application and Domain.
- API may depend on Application and Infrastructure.
- Frontend communicates only through the HTTP API.

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
