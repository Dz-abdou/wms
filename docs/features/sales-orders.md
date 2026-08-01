# Feature: Sales Orders

## Status

Implemented on `features/sales-orders`; pending review, manual acceptance testing, and merge.

## Goal

Create a reliable outbound commitment before reservations, picking, and shipment are introduced.

## Included

- Draft, submit, and cancel sales orders with optimistic concurrency and append-only status history.
- Immutable automatic `SO-YYYY-######` document numbers using the shared document-numbering service.
- One active customer, one active shipping address, one active currency, order date, requested ship date, customer reference, and delivery instructions per order.
- Customer code/name and selected shipping-address snapshots on the order; product SKU/name, UoM conversion, and ordered quantities on each line.
- Product/UoM/whole-quantity validation, server-side customer/status/date filtering, English/French API errors, and a localized list/create/edit/detail UI.

## Explicitly Excluded

- Sales prices, discounts, tax, totals, payment terms, invoices, and accounting. Product selling prices do not exist yet, so this operational slice must not invent commercial data.
- Inventory reservation, available-stock commitments, allocation, pick tasks, shipment confirmation, stock movements, returns, and fulfilment status beyond `Submitted`.
- Customer contact selection, delivery proof, print/export, and manual number overrides.

## Business Rules

1. A sales order must have an active customer and a selected address belonging to that customer which is valid for shipping.
2. The order stores a snapshot of its customer and shipping address. Later master-data edits do not alter the order.
3. A line selects an active product and a valid product UoM. Its ordered base quantity and conversion factor are captured at save time; units that disallow fractions reject fractional quantities.
4. A draft can be replaced only with its current version. Submitted and cancelled orders are immutable.
5. Submission requires at least one unique valid line. Cancellation is allowed from Draft or Submitted and creates a status-history entry.
6. Number allocation occurs inside the create transaction. Failed creation does not leave a persisted sales order or counter increment.

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
4. List/detail pages show the document number, status, customer, shipping destination, dates, reference, instructions, lines, and status history.
5. Every UI string is localized in English and French; modified frontend files are formatted.

## Migration and Tests

The developer generates and applies the EF Core migration after the domain/configuration change. Tests cover domain invariants, PostgreSQL persistence/API workflows, and frontend form/detail behavior.
