# LotSizingDataModel scientific governance

This directory contains machine-readable contracts that govern the scientific
and software evolution of LotSizingDataModel.

## Authority and scope

The domain model remains the source of truth for the problem data. Files in
this directory describe project governance, scientific coverage, validation
namespaces, extension completeness and generated-artifact lifecycle. They do
not replace the domain model and they must never be used to invent problem
semantics that are absent from `Core`.

## Files

- `CAPITALIZATION.json`: permanent lessons and process guards.
- `SCIENTIFIC-COVERAGE.json`: current cross-layer coverage and known gaps.
- `VALIDATION-RULES.json`: stable validation namespaces, severities and service
  responsibilities.
- `EXTENSION-CONTRACT.schema.json`: structured contract required for future
  scientific extensions.
- `EXTENSION-CONTRACT.template.json`: starter contract for a new extension.
- `ARTIFACT-LIFECYCLE.json`: lifecycle and retention policy for generated
  artifacts owned by project automation.

## Cross-layer invariant

A scientific extension is complete only when all applicable layers are in
parity:

`Core <-> Instance <-> Classification/Notation <-> Mathematical Model
<-> Solver <-> Solution <-> Model Checker/Feasibility/Solution Checker
<-> Tests <-> Documentation`

A deliberately unsupported layer must be explicitly marked as such.

## Scientific provenance

Published/canonical behavior, platform adaptations and project-specific
extensions must be distinguished. Literature attributions are added only after
verification of the relevant source.

## Transactional version identity

A version-changing candidate is committed inside the disposable shadow
worktree before the official validated build. The real NextGen branch is
advanced only by fast-forward to that exact validated commit. This guarantees
that the version computation, binaries, package metadata and final local Git
HEAD describe the same immutable source state.
