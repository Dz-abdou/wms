# How to Start This Project with Codex

Give Codex access to this repository, then begin with this prompt:

```text
Read AGENTS.md and every Markdown file in /docs.

Do not write code yet.

Review the project scope, architecture and roadmap.
Identify contradictions, missing foundation decisions and unnecessary complexity.

Then propose a detailed implementation plan for Phase 0 only.

The plan must include:
- repository structure
- .NET projects
- frontend structure
- dependencies
- Docker Compose
- PostgreSQL configuration
- test setup
- CI setup
- exact verification commands

Do not start Products, Warehouses or Inventory.
Wait for approval before editing files.
```

After approving the plan, use:

```text
Implement the approved Phase 0 plan.

Follow AGENTS.md.
Do not add business entities yet.
Run all available build and test commands.
Report:
- files created or modified
- dependencies added
- commands executed
- results
- remaining issues
```

After Phase 0 is verified, copy `FEATURE_TEMPLATE.md` and create the specification for Products.
