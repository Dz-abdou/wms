# Feature: <Feature Name>

## Status

Planned

## Goal

Describe the user or business outcome.

## Scope

- Included behavior 1
- Included behavior 2

## Out of Scope

- Excluded behavior 1
- Excluded behavior 2

## Business Rules

1. Rule
2. Rule

## Data Model Changes

List expected entities, fields, relationships, constraints and indexes.

Codex may implement model and configuration changes, but the developer must manually generate and apply EF Core migrations. Never ask Codex to edit generated migrations or snapshots.

## API Requirements

| Method | Route | Purpose |
|---|---|---|
| GET | `/api/...` | ... |
| POST | `/api/...` | ... |

## Frontend Requirements

- Pages
- Forms
- Tables
- Filters
- Modals
- Loading states
- Empty states
- Error states
- Feature hooks, centralized query keys, route paths, endpoint paths, and validation limits

## Acceptance Criteria

### Scenario 1

Given ...
When ...
Then ...

### Scenario 2

Given ...
When ...
Then ...

## Unit Tests

- Test case

## Integration Tests

- Test case

## Frontend Tests

- Test case

## Manual Test Checklist

- [ ] Check

## Definition of Done

- [ ] Acceptance criteria pass
- [ ] Backend validation exists
- [ ] Database constraints exist where applicable
- [ ] Developer generated and reviewed the migration manually
- [ ] Developer applied the migration manually
- [ ] Unit tests pass
- [ ] Integration tests pass
- [ ] Frontend tests pass where applicable
- [ ] Backend builds
- [ ] Frontend builds
- [ ] Manual tests pass
- [ ] Documentation is updated
- [ ] Feature has a focused commit