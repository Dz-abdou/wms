# Feature: Suppliers

## Status

In progress — Phase 4 Supplier management slice

## Goal

Allow authorized operators to maintain a trustworthy supplier catalogue before purchase-order workflows are introduced.

## Scope

- Create, edit, view, paginate, activate, and deactivate suppliers.
- Store a unique supplier code, name, and optional contact details.
- Provide English/French UI text and stable API error codes.

## Out of Scope

- Purchase orders, supplier price lists, payment terms, tax details, and supplier deletion.
- Supplier search or filtering.

## Planned Follow-up: Supplier Product Catalogue

Implement this with the Purchase Order slice, not as a single `SupplierId` on `Product`. A product can be supplied by many suppliers and a supplier can supply many products, so purchasing will introduce a dedicated relationship with the supplier-specific SKU, preferred-supplier marker, lead time, minimum order quantity, allowed purchase unit, and price/currency history or effective price. Purchase-order lines will use that catalogue to validate and prefill supplier-specific purchasing data.

## Business Rules

1. Code is required, trimmed, stored uppercase, 1–32 characters, and unique.
2. Name is required, trimmed, and 1–200 characters.
3. Email, phone number, and address are optional; when supplied they are trimmed and limited to 320, 50, and 500 characters respectively.
4. New suppliers are active; status changes are idempotent.
5. Missing suppliers return `supplier.not_found`; duplicate codes return `supplier.code_conflict`.
6. Catalogue readers can view suppliers; catalogue managers can create, edit, and change supplier status.

## Data Model

`Supplier`: UUID ID, Code, Name, optional Email, PhoneNumber, and Address, active status, and UTC creation/update metadata. PostgreSQL enforces nonblank, uppercase, uniquely indexed supplier codes and nonblank names.

## API

- `GET /api/suppliers?page=1&pageSize=20`
- `GET /api/suppliers/{id}`
- `POST /api/suppliers`
- `PUT /api/suppliers/{id}`
- `PATCH /api/suppliers/{id}/status`

## Acceptance Criteria

1. Creating ` supplier-001 ` returns `201 Created` with `SUPPLIER-001`, and it appears in the paginated list.
2. Creating or editing another supplier with the same case-insensitive code returns `409` with `supplier.code_conflict`.
3. Blank required fields and overlong contact details return translated field validation errors in the UI.
4. Operators can view suppliers; managers can create, edit, activate, and deactivate them.
5. Loading, empty, error, and populated list states are clear in both supported languages.

## Tests

- Unit tests cover normalization, length rules, and idempotent status changes.
- PostgreSQL integration tests cover creation, validation/error codes, pagination, status persistence, and the unique database index.
- Frontend tests cover list states and client/server form validation.

## Manual Test Checklist

- [ ] Apply the manually generated Supplier migration after reviewing it.
- [ ] Create a supplier through Swagger and the UI.
- [ ] Verify code normalization and duplicate-code rejection.
- [ ] Edit contact details and activate/deactivate a supplier.
- [ ] Switch English/French and verify Supplier copy and error feedback.
- [ ] Verify pagination with more than one page of suppliers.

## Definition of Done

- [ ] Acceptance criteria pass.
- [ ] Developer reviewed and applied the Supplier migration manually.
- [ ] Unit, integration, and frontend tests pass.
- [ ] Backend and frontend production builds pass.
- [ ] Manual checklist passes.
- [ ] Supplier slice is reviewed, merged, and only then Purchase Orders begin.
