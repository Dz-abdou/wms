# Localization and API Error Contract

## Status

Implemented — Phase 1.1

## Purpose

English and French are mandatory application languages. The backend exposes stable error identifiers; the frontend owns user-facing translation. This keeps API behavior predictable and prevents UI logic from parsing English or French server messages.

## Frontend Localization

Use a lightweight React i18n solution when Phase 1.1 begins. Verify the latest compatible versions before adding packages; `i18next` and `react-i18next` are the expected baseline unless current project constraints indicate a better supported option.

Recommended structure:

```text
frontend/warehouse-web/src/
  shared/i18n/
    i18n.ts
    locales/
      en/common.json
      fr/common.json
  shared/errors/
    problemDetails.ts
```

Rules:

- Every user-visible label, button, heading, empty state, loading state, confirmation, form message, navigation item, and accessible label uses a translation key.
- Locale resources are JSON files, with English and French keys kept structurally identical.
- The selected language is persisted locally and defaults to the browser preference, falling back to English.
- Formatting for dates, numbers, and future currencies uses the active locale.
- Presentation components receive translated text or call the translation hook; they do not contain hardcoded display strings.

## API Error Contract

All API failures use Problem Details and include a stable extension field named `code`.

Example conflict response:

```json
{
  "type": "https://httpstatuses.com/409",
  "title": "SKU already exists.",
  "status": 409,
  "detail": "A product with SKU 'SKU-001' already exists.",
  "code": "product.sku_conflict",
  "traceId": "..."
}
```

The frontend uses `code` to select localized copy. `title` and `detail` remain useful for logs, API consumers, and fallback diagnostics, but are not parsed to drive UI behavior.

### Validation responses

Keep standard Problem Details field names in `errors` for compatibility. Add a parallel `errorCodes` extension keyed by field name.

```json
{
  "status": 400,
  "title": "One or more validation errors occurred.",
  "code": "validation.failed",
  "errors": {
    "sku": ["SKU is required."]
  },
  "errorCodes": {
    "sku": ["validation.required"]
  }
}
```

The frontend renders `errorCodes.sku` through locale JSON. If it receives an unknown code, it shows a generic localized error rather than backend text.

## Initial Error-Code Catalogue

| Code | Meaning |
|---|---|
| `validation.failed` | Request has one or more invalid fields. |
| `validation.required` | A required field is blank or missing. |
| `validation.max_length` | A field exceeds its maximum length. |
| `product.not_found` | The requested Product does not exist. |
| `warehouse.not_found` | The requested Warehouse does not exist. |
| `warehouse.code_conflict` | Another Warehouse already uses the code. |
| `product.sku_conflict` | Another Product already uses the SKU. |
| `system.unexpected` | An unexpected server error occurred. |

Codes are additive and stable. Renaming or removing a published code is a breaking API change.

## Phase 1.1 Implementation Order

1. Verify and add the supported i18n packages.
2. Add the i18n provider, locale JSON files, language switcher, and persistence.
3. Replace existing shared-layout and Product display strings with translation keys.
4. Add a reusable backend Problem Details code writer and validation error-code mapping.
5. Refactor Product errors to return the documented codes.
6. Refactor frontend `ApiError` handling to map codes and field error codes to translated messages.
7. Add frontend tests in both languages and backend/integration tests for exact error codes.
8. Manually verify language switching, validation, duplicate SKU, missing Product, and unexpected-error states.

No database migration is expected for Phase 1.1.