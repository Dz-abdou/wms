# Feature: Sales Orders

## Status

Hardening in progress on `features/sales-orders`; pending migration, verification, manual acceptance testing, and merge.

## Goal

Create a reliable outbound commitment before reservations, picking, and shipment are introduced.

## Included

- Draft, submit, and cancel sales orders with optimistic concurrency and append-only status history.
- Immutable automatic `SO-YYYY-######` document numbers using the shared document-numbering service.
- One active customer, one active shipping address, one active fulfilment warehouse, one active currency, order date, requested ship date, customer reference, and delivery instructions per order.
- Customer code/name and selected shipping-address snapshots on the order; product SKU/name, UoM conversion, and ordered quantities on each line.
- Product/UoM/whole-quantity validation, server-side customer/status/date filtering, warehouse-scoped current availability, English/French API errors, and a localized list/create/edit/detail UI.
- Currency defaults from the selected customer's default currency; each new product line defaults to the product base UoM. Both remain editable within valid active values.
- Each line shows the selected fulfilment warehouse's current on-hand quantity in the product base UoM and a clear shortage warning when the requested base quantity exceeds it. Shortages do not block draft saving or submission: they represent an allowed backorder until allocation is introduced.

## Explicitly Excluded

- Sales prices, discounts, tax, totals, payment terms, invoices, and accounting. Product selling prices do not exist yet, so this operational slice must not invent commercial data.
- Inventory reservation, available-stock commitments, allocation, pick tasks, shipment confirmation, stock movements, returns, and fulfilment status beyond `Submitted`. Availability is informational only in this slice and is not a reservation.
- Customer contact selection, delivery proof, print/export, and manual number overrides.

## Business Rules

1. A sales order must have an active customer, a selected address belonging to that customer which is valid for shipping, and an active fulfilment warehouse.
2. The order stores a snapshot of its customer and shipping address. Later master-data edits do not alter the order.
3. A line selects an active product and a valid product UoM. Its ordered base quantity and conversion factor are captured at save time; units that disallow fractions reject fractional quantities.
4. A draft can be replaced only with its current version. Submitted and cancelled orders are immutable.
5. Submission requires at least one unique valid line. Cancellation is allowed from Draft or Submitted and creates a status-history entry.
6. Current warehouse availability is read from `InventoryBalance` in the selected fulfilment warehouse and compared in base units. It is not locked or reserved, so it may change before later allocation.
7. Number allocation occurs inside the create transaction. Failed creation does not leave a persisted sales order or counter increment.

## Initial Lifecycle

```text
Draft → Submitted
  └──→ Cancelled
Submitted → Cancelled
```

`Allocated`, `PartiallyShipped`, and `Shipped` are introduced only with reservations and shipment workflows.

## Acceptance Criteria

1. Creating an order generates a unique `SO-YYYY-######` number, retains customer/address/product/UoM snapshots, and returns a Draft order.
2. A blocked customer, non-shipping address, invalid UoM, fractional whole-only quantity, or stale update produces a meaningful stable API error and inline frontend validation where editable.
3. A submitted order is read-only; concurrent draft edits return a conflict without overwriting newer data.
4. List/detail pages show the document number, status, customer, fulfilment warehouse, shipping destination, dates, reference, instructions, lines, and status history.
5. Selecting a customer defaults its currency; selecting a product defaults its base UoM. The form shows line availability and a localized shortage warning while allowing a backorder.
6. Every UI string is localized in English and French; modified frontend files are formatted.

## Migration and Tests

The developer generates and applies the EF Core migration after the domain/configuration change. Tests cover domain invariants, PostgreSQL persistence/API workflows, and frontend form/detail behavior.
