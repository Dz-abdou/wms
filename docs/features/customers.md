# Feature: Customer Management

## Status

In progress — first Phase 6 vertical slice on `features/Customer-management`.

## Goal

Maintain reliable customer accounts before Sales Orders, reservations, picking, and shipping introduce outbound commitments.

## Scope

- Create, edit, view, paginate, search, activate, and block customer accounts.
- Store a unique customer code, legal name, optional trading name, optional active default currency, delivery instructions, and service notes.
- Maintain zero or more customer contacts for sales, delivery, accounts, or general use.
- Maintain zero or more customer addresses, each usable for shipping, billing, or both.
- English/French UI, stable API error codes, inline server validation, and role-aware read/manage authorization.

## Out of Scope

- Sales Orders, address snapshots, commercial pricing, payment terms, credit limits, tax, reservations, picking, shipment, returns, or customer-account deletion.
- Requiring a customer to have a contact or address before it can be created. Sales Orders will later enforce the appropriate shipping/billing address requirements.

## Business Rules

1. Customer code is required, trimmed, uppercase, 1–32 characters, and unique.
2. Legal name is required, trimmed, and 1–200 characters. Trading name is optional and limited to 200 characters.
3. A supplied default currency must be an active ISO currency. It is optional at customer-master stage and becomes a Sales Order default later.
4. Contacts may share email addresses or phone numbers across customers. A contact needs a nonblank name; role, email, and phone are optional.
5. An address needs a nonblank label, address line 1, city, and ISO country code. It must be marked for shipping, billing, or both.
6. New customers are active. Status changes are idempotent. Blocked customers remain readable but future Sales Orders must reject them.
7. Missing customers return `customer.not_found`; duplicate codes return `customer.code_conflict`; invalid/default-inactive currencies return stable field validation codes.
8. Operators can read customers. Managers can create, edit, and change status.

## Data Model Changes

- `Customer`: UUID ID, Code, LegalName, optional TradingName, optional DefaultCurrencyCode, optional DeliveryInstructions, optional ServiceNotes, active status, and UTC audit metadata.
- `CustomerContact`: UUID ID, CustomerId, Name, optional Role, Email, PhoneNumber, and UTC audit metadata.
- `CustomerAddress`: UUID ID, CustomerId, Label, AddressLine1, optional AddressLine2, City, optional PostalCode, CountryCode, Shipping/Billing flags, optional DeliveryInstructions, and UTC audit metadata.
- PostgreSQL enforces nonblank/uppercase/unique customer codes, nonblank required text fields, valid address-purpose flags, and restrictive Customer foreign keys.

Codex implements entity and configuration changes only. The developer manually generates, reviews, and applies the EF Core migration; Codex never edits migrations or the model snapshot.

## API Requirements

| Method | Route | Purpose |
|---|---|---|
| GET | `/api/customers?page=1&pageSize=20&search=&isActive=` | Paged customer list. |
| GET | `/api/customers/{id}` | Customer detail with contacts and addresses. |
| POST | `/api/customers` | Create a customer account. |
| PUT | `/api/customers/{id}` | Update a customer account. |
| PATCH | `/api/customers/{id}/status` | Activate or block customer. |
| POST/PUT/DELETE | `/api/customers/{id}/contacts[/{contactId}]` | Maintain a contact. |
| POST/PUT/DELETE | `/api/customers/{id}/addresses[/{addressId}]` | Maintain a shipping/billing address. |

## Frontend Requirements

- Outbound navigation exposes **Customers** and its paginated list with one primary **New** action.
- Create/edit pages use the shared customer-header form. Contacts and addresses are maintained from the customer detail page and remain optional at this stage.
- Customer detail follows the shared detail layout: summary, contacts table, then addresses table; Sales Order history waits for the Sales Order slice.
- List filters are URL-backed, server-side search by code/legal/trading name, and active status.
- All visible copy, accessible labels, validation, loading, empty, and error states are translated in English/French.

## Acceptance Criteria

1. Creating ` customer-001 ` stores and returns `CUSTOMER-001` and appears in a paginated search.
2. Duplicate customer codes return `customer.code_conflict` without creating a second account.
3. A customer can be saved with multiple contacts and separate shipping/billing addresses; an address without either purpose is rejected.
4. A supplied inactive or unknown default currency is shown as a localized error on the currency field.
5. Operators can view but cannot modify customers; managers can create, edit, activate, and block them.

## Tests

- Unit: normalization, required/optional field limits, address-purpose rule, and idempotent status transitions.
- Integration: CRUD, code uniqueness, contact/address persistence, currency validation, pagination/search/status filtering, authorization, and stable error codes.
- Frontend: list states/filtering, form validation/server errors, editable contact/address tables, detail rendering, and English/French copy.

## Manual Test Checklist

- [ ] Generate, review, and apply the Customer migration.
- [ ] Create a customer with contacts and separate shipping/billing addresses.
- [ ] Verify code normalization, duplicate-code rejection, and optional default currency validation.
- [ ] Edit contacts/addresses, then block and reactivate the customer.
- [ ] Verify English/French states and list return-navigation behaviour.

## Definition of Done

- [ ] Acceptance criteria pass.
- [ ] Developer reviewed and applied the migration manually.
- [ ] Unit, integration, and frontend tests pass.
- [ ] Backend and frontend production builds pass.
- [ ] Manual checklist passes.
- [ ] Feature is reviewed and merged before the Sales Order slice begins.
