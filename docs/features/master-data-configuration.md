# Feature: Master Data Configuration

## Status

Planned — approved as an extension of the combined Supplier and core Purchase Order branch.

## Goal

Let catalogue managers configure the reference data that purchasing and products depend on, rather than relying on application settings or free-text values.

## Scope

- Persistent Currency master data with code, name, symbol, decimal places, active state, and one default currency.
- Initial seeded currencies: DZD (default), EUR, and USD.
- Currency list/create/status/default API and manager UI.
- Supplier catalogue currency choices loaded from active Currency records; inactive or missing currencies are rejected by the API.
- Product Category list/create/edit UI, including optional parent category.
- Product Category update API with unique-code and parent-category validation.
- Grouped navigation for Master data, Inbound, Inventory, and Administration.

## Out of Scope

- Exchange rates, conversion, accounting, tax, supplier payment terms, and multi-currency PO settlement.
- Currency deletion; historical supplier catalogue and purchase-order data must remain referentially valid.
- Category deletion or category activation state.
- Customer, sales, receiving, or warehouse-bin screens.

## Business Rules

1. Currency code is a unique normalized three-letter code; the client chooses from active records rather than typing it.
2. A currency has a nonblank name, optional nonblank symbol, and 0–4 decimal places.
3. Exactly one active currency is default. A manager cannot deactivate the default currency; they must first choose another active currency as default.
4. Supplier catalogue prices reference an active Currency by its stable code. Existing `CurrencyCode` values remain transaction-friendly ISO snapshots and gain a database foreign key to Currency code.
5. Product Category code remains unique and uppercase. A category cannot be its own parent; a parent must exist.
6. Managers can configure currencies/categories; operators can read them where purchasing or product entry requires it.

## Data Model Changes

- `Currency`: UUID ID, unique Code, Name, optional Symbol, DecimalPlaces, IsActive, IsDefault, and persistent metadata.
- `SupplierProduct.CurrencyCode` gains a foreign key to `Currency.Code`.
- `ProductCategory` gains update behavior only; no new category table fields are required.

The developer must manually generate and review a new migration. Codex must not edit generated migrations or the model snapshot.

## API Requirements

| Method | Route | Purpose |
|---|---|---|
| GET | `/api/currencies` | List currencies; `activeOnly=true` supports purchasing selectors. |
| POST | `/api/currencies` | Create a currency. |
| PUT | `/api/currencies/{id}` | Edit name, symbol, and decimal places. |
| PATCH | `/api/currencies/{id}/status` | Activate/deactivate a non-default currency. |
| PATCH | `/api/currencies/{id}/default` | Make an active currency the sole default. |
| GET | `/api/product-categories/{id}` | Read one category for editing. |
| PUT | `/api/product-categories/{id}` | Update code, name, and parent. |

## Acceptance Criteria

1. A manager creates EUR, activates it if needed, and makes it default; DZD is no longer default and cannot be selected as default until reactivated.
2. A supplier catalogue entry accepts an active currency and rejects an inactive/unconfigured currency with a stable API error code.
3. The supplier catalogue form loads active currencies from `/api/currencies`, not application settings.
4. A manager can create and edit categories; duplicate codes, missing parents, and self-parenting are rejected.
5. Navigation groups current pages without exposing Administration actions to non-administrators.

## Tests

- Domain/unit tests for Currency normalization, default/status transitions, and category update rules.
- PostgreSQL integration tests for currency uniqueness/default constraints, active-currency catalogue validation, and category update validation.
- Frontend tests for Currency/Category list states and product-derived purchase-unit/currency selection.

## Manual Test Checklist

- [ ] Generate and review the migration.
- [ ] Apply the migration and confirm DZD, EUR, and USD exist with DZD as the default.
- [ ] Create/edit a category and select it from the Product form.
- [ ] Create/deactivate/default a currency and confirm its effect in Supplier Catalogue.
- [ ] Verify grouped navigation in English and French for manager, operator, and administrator accounts.
