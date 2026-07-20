# Codex Development Workflow

Use this process for every feature.

## 1. Define the Feature

Create a feature specification using `FEATURE_TEMPLATE.md`.

The specification must include:

- Goal
- Scope
- Out of scope
- Business rules
- Acceptance criteria
- Required tests

## 2. Ask Codex to Inspect

Prompt:

```text
Read AGENTS.md and the relevant files in /docs.

Inspect the current code related to this feature.
Do not edit anything.

Explain:
- current implementation
- relevant extension points
- risks
- missing requirements
```

## 3. Ask Codex for a Plan

Prompt:

```text
Create an implementation plan for this feature.

Include:
- database changes
- backend changes
- frontend changes
- validation
- unit tests
- integration tests
- manual tests
- files expected to change

Do not write code yet.
Keep the work limited to the approved scope.
```

Review and correct the plan before implementation.

## 4. Implement One Slice

Prompt:

```text
Implement the approved plan.

Requirements:
- follow AGENTS.md
- do not make unrelated changes
- run relevant builds and tests
- report files changed and test results
```

## 5. Review the Change

Prompt:

```text
Review the implemented change as a pull request.

Look for:
- incorrect business rules
- transaction problems
- concurrency problems
- EF Core issues
- missing database constraints
- missing tests
- API error-handling problems
- frontend loading and error-state problems
- unnecessary abstractions

Do not change anything yet. Report findings first.
```

Approve only valid fixes.

## 6. Manual Verification

Run the manual checklist from the feature specification.

Verify the feature through:

- Swagger or an API client
- The React UI
- PostgreSQL data where needed

## 7. Complete the Feature

A feature is complete only when:

- Acceptance criteria pass.
- Unit tests pass.
- Integration tests pass.
- Frontend tests pass where relevant.
- Backend builds.
- Frontend builds.
- Manual checks pass.
- Documentation is updated.
- The change is committed separately.

## 8. Move to the Next Feature

Before starting the next feature:

- Confirm the current branch is clean.
- Confirm all tests pass.
- Update the roadmap if scope changed.
- Create the next feature specification.
