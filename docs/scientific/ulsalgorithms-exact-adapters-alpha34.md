# ULSAlgorithms exact adapters — alpha.34

## Scope

Alpha.34 deliberately exposes two exact methods from the pinned
`ULSAlgorithms` v1.1.0 release:

- `wagner-whitin-classical` — classical Wagner–Whitin dynamic programming;
- `zangwill-network` — Zangwill network / shortest-path algorithm.

No heuristic is enabled in alpha.34 and there is no silent exact-method
fallback.

## Immutable external dependency

- package: `ULSAlgorithms` 1.1.0;
- source commit: `3e5595996d35373dd93f90a9245c6bc9e65f9e0d`;
- release package SHA-256:
  `4eff21da87a9ff7649ee2e0f1c6f835b0af3cfa6bc08dd9ba91306fdd5ff2a3c`.

The launcher verifies the exact package hash before creating the shadow
candidate. The validated package is then vendored under `external/nuget` so
future restores do not depend on a mutable global package source.

## Applicability

The LotSizingDataModel bridge is intentionally conservative. It accepts only a
canonical deterministic single-item uncapacitated model with one demand, one
production routing, one inventory, zero production lead time, zero fixed
initial inventory, no scheduled receipts, no BOM/procurement/transport
extension, and only canonical production/setup/inventory plus optional delivery
decision variables.

The mathematical model must contain only inventory-balance,
demand-satisfaction and production/setup-link constraints. Any richer
formulation is rejected instead of being approximated.

## End-to-end mapping

The external solution is validated independently by
`ULSAlgorithms.Validation.UlsSolutionValidator`. Only a complete feasible
result with consistent objective value is projected onto the already-built
LotSizingDataModel mathematical variables. The existing
`IMathematicalSolutionMappingService` then creates the normalized
`LotSizingSolution`.

This preserves the separation between problem class, mathematical formulation,
external exact method and ordinary MILP backends.

## Remaining scope

Other exact ULSAlgorithms methods remain outside alpha.34 R1. Heuristic
adapters remain scheduled for alpha.35.
