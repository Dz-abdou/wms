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

- Authentication
- Roles and permissions
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
4. Stock adjustments
5. Inventory history
6. Suppliers
7. Purchase orders
8. Goods receipts
9. Customers
10. Sales orders
11. Reservations
12. Shipping
13. Authentication
14. Audit logs
15. Dashboard
16. Deployment and documentation
