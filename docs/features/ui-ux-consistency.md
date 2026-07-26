# Feature: UI/UX Consistency

## Status

In progress — dedicated cross-cutting UI/UX branch after the current inventory-adjustment work is merged.

## Goal

Make every completed authenticated screen feel like one predictable WMS application through reusable layouts, action placement, return behaviour, navigation, feedback states, and visual foundations.

## Scope

- Shared list, form, detail, back-link, and action-area primitives for React/Ant Design pages.
- Explicit, safe `Back to …` behaviour that preserves list path/query context and has a feature-list fallback.
- Standard action hierarchy, including one shared `New` list action, dedicated create/edit routes for every object, confirmation for destructive/irreversible actions, and no nested links inside buttons.
- Consistent loading, empty, error, filter, table, and pagination layouts.
- Reusable URL-backed search/filter controls that always request server-filtered, paginated list data.
- Apply the shared system to existing Products, Warehouses, Suppliers, Product Categories, Currencies, Supplier Catalogue, Purchase Orders, Inventory Movements, and Inventory Adjustments screens.
- Group Administration navigation and make the growing grouped navigation responsive.
- Document the standard in the roadmap and operational plan so later screens implement it from the start.

## Out of Scope

- Changing business rules, API contracts, database schema, authorization policies, or existing operational workflow semantics.
- New dashboard, goods receipt, sales, inventory overview, responsive mobile redesign, or a paid UI dependency.
- Replacing Ant Design or adding a second component library.

## Business Rules

1. List, create/edit, detail, and ledger pages use the documented page type and its shared layout. Create/edit pages always use their own route.
2. A Back action has an explicit safe route; it does not rely on browser history. It returns to a preserved list URL when available and otherwise to the owning feature list.
3. A dirty create/edit form warns before navigation away through its Back/Cancel action.
4. Only one action is visually primary on a page. Destructive/irreversible operations require confirmation.
5. Operational ledgers are read-only; related document shortcuts are clearly named and do not imply direct movement creation.
6. All user-visible UI and accessible labels remain translated in English and French.
7. List search and filtering is server-side, applied before pagination, and limited to operationally meaningful fields.

## Data Model Changes

None.

## API Requirements

None. Existing frontend routes and API contracts remain unchanged.

## Frontend Requirements

- `ListPageLayout`: heading, optional primary action, filter slot, and state/content slot.
- `FormPageLayout`: back link, heading/context, content, standard action bar, safe return handling, and dirty-form protection.
- `DetailPageLayout`: back link, title/status, action slot, summary, and content sections.
- Shared link/button primitives with accessible Ant Design semantics.
- A centralized route-return helper that preserves `pathname + search` from originating list pages.
- Shared CSS/theme tokens and responsive navigation behaviour.
- Each list declares its search fields and allowed server filters; the shared layout serializes them to URL query state and never filters a loaded page in memory.
- Existing screens migrate to the appropriate layout without duplicating headings, action placement, or generic states.

## Acceptance Criteria

1. Every existing authenticated list page has a consistent heading/action/state layout and its relevant loading, empty, error, and populated states.
2. Every existing create/edit screen displays a visible Back action and the shared bottom Cancel/Create or Save action bar.
3. Every existing detail screen displays a visible Back action, consistent contextual actions, and a summary followed by related content.
4. A user following a detail/form link from a filtered list returns to the same list URL; a direct page visit falls back safely to the feature list.
5. Leaving a dirty form through Back or Cancel asks for confirmation before discarding edits.
6. Currencies, Categories, and Users open dedicated create/edit pages from their lists; none use a configuration modal.
7. The main navigation groups Administration and stays usable at the supported narrow layout.
8. English and French remain structurally complete for all new text.

## Frontend Tests

- Shared return helper: preserved route, direct-route fallback, and dirty-form confirmation.
- Shared list/form/detail layouts render their required areas and accessible controls.
- Representative list, form, detail, master-data/admin create/edit, and ledger pages use the new primitives.
- English/French translation coverage for new shared UI copy.

## Manual Test Checklist

- [ ] Visit each list screen and verify loading, empty, error, and populated states where applicable.
- [ ] From a filtered list, open detail/edit/create and verify Back returns to the same list URL.
- [ ] Try to leave a dirty form through Back and Cancel; verify confirmation and safe navigation.
- [ ] Verify one clear primary action per page and confirmation before destructive/irreversible actions.
- [ ] Verify Movement History remains read-only and its adjustment shortcut is clearly labelled.
- [ ] Verify desktop and narrow-width grouped navigation in English and French.

## Definition of Done

- [ ] All acceptance criteria pass.
- [ ] No model, migration, or API change is introduced.
- [ ] Frontend tests and production build pass.
- [ ] Roadmap and operational-plan standards are updated.
- [ ] Existing completed pages use shared layouts rather than local copies.
- [ ] The branch has focused documentation and implementation commits.
