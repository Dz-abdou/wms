# Warehouse Management System — Project Brief

## Goal

Build a portfolio-quality warehouse management system for small and medium businesses.

The application must demonstrate:

- Clean .NET backend development
- Real business rules
- PostgreSQL and EF Core usage
- React enterprise UI development
- Automated testing
- Docker-based local setup
- Clear architecture and documentation

## Technology Stack

### Backend

- ASP.NET Core Web API
- C#
- Entity Framework Core
- PostgreSQL
- FluentValidation
- OpenAPI / Swagger
- Serilog
- xUnit
- Testcontainers for integration tests

### Frontend

- React
- TypeScript
- Vite
- TanStack Query for server state
- Ant Design for enterprise UI components
- TanStack Table only when Ant Design Table is not flexible enough
- React Hook Form for complex forms when useful
- Zod for client-side schemas when useful
- Vitest
- React Testing Library

## Product Scope

### Milestone 1 — Inventory Foundation

- Product management
- Warehouse management
- Inventory balances
- Manual stock adjustments
- Inventory movement history

### Milestone 2 — Purchasing

- Supplier management
- Purchase orders
- Partial goods receipts
- Automatic inventory updates

### Milestone 3 — Sales

- Customer management
- Sales orders
- Stock reservations
- Shipment confirmation
- Automatic inventory reduction

### Milestone 4 — Enterprise Features

- Authentication
- Role-based authorization
- Audit logs
- Dashboard
- Low-stock indicators
- CSV export
- Demo data

## Main Business Rules

- Product SKU must be unique.
- Warehouse code must be unique.
- Inventory balance is unique per product and warehouse.
- Stock cannot become negative.
- Every stock change must create an inventory movement.
- Inventory balance and movement changes must occur in one transaction.
- Purchase orders may be received partially.
- Received quantity cannot exceed outstanding quantity.
- Sales orders cannot reserve more than available stock.
- Reserved stock cannot be used by another order.
- Shipment reduces physical and reserved stock.
- Completed business documents cannot be freely edited.
- Important inventory updates must use optimistic concurrency.

## Out of Scope for the First Version

- Accounting
- Supplier invoices
- Customer payments
- Product manufacturing
- Returns
- Barcode hardware integration
- Mobile application
- Multi-company tenancy
- Advanced forecasting
- Paid UI libraries or paid component features

## Portfolio Deliverables

- Public Git repository
- Clear README
- Architecture documentation
- Database diagram
- Docker Compose setup
- Seeded demo account and data
- Automated unit and integration tests
- Deployed demo
- Screenshots
- Short demonstration video
