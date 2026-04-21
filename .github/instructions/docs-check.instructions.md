---
description: "Use when creating or updating markdown documentation. Requires validating the change with the docs-check workflow before finishing."
applyTo: "**/*.md"
---

# Documentation Validation Rule

When you create or update any markdown documentation, you must validate it with the same checks used by `.github/workflows/docs-check.yml` before finishing the task.

## Required Validation

- Run markdown linting for the changed markdown files.
- Run spelling checks for the changed markdown files.
- Fix any issues found before considering the documentation complete.

## Scope

- `README.md`
- `QUICK-START.md`
- `INDEX.md`
- `docs/**/*.md`
- Any other markdown file in the repository
