# ULSAlgorithms heuristic adapters — alpha.35

## Scope

Alpha.35 exposes every public heuristic strategy in the pinned
`ULSAlgorithms` v1.1.0 catalog. The external dependency itself is unchanged
from alpha.34 and remains the tracked, SHA-256-verified release artifact.

The pinned heuristic inventory contains 19 methods:

- lot-for-lot;
- periodic-order-quantity;
- silver-meal;
- segerstedt-reformulated-silver-meal;
- least-unit-cost;
- chiu-modified-least-unit-cost;
- ho-chang-solis-net-least-period-cost;
- ho-chang-solis-improved-net-least-period-cost;
- part-period-simplified;
- part-period-balancing;
- chiu-ting-modified-part-period-balancing;
- patterson-laforge-incremental-part-period;
- wemmerlov-modified-ppb;
- wemmerlov-ppb-lalb;
- wemmerlov-modified-ppb-lalb;
- mclaren-order-moment;
- groff;
- freeland-colley;
- karni-maximum-part-period-gain.

The bridge compares the runtime catalog against this exact pinned list before
execution. A catalog drift is therefore a hard error.

## Semantic distinction from exact methods

Heuristics use the same external `IUlsSolver` interface but their scientific
contract is different:

- external status must be `UlsSolveStatus.Feasible`;
- the normalized mathematical result has `HasFeasibleSolution = true`;
- `IsOptimal` is always `false`;
- no best bound or optimality gap is manufactured.

An exact solver ID cannot be passed through the heuristic adapter.

## Applicability

Alpha.35 deliberately reuses the strict canonical SI-ULS extractor introduced
by alpha.34. Heuristics therefore do not silently obtain broader applicability
than the exact adapters.

Individual published heuristics can impose stronger restrictions. The external
ULSAlgorithms implementation remains authoritative and rejects incompatible
instances rather than changing the published rule.

## Validation

Every returned plan is independently checked with
`UlsSolutionValidator.Validate` before it can be projected into the
LotSizingDataModel mathematical result or normalized solution.

The targeted alpha.35 smoke uses a stationary-cost, strictly-positive-demand
instance compatible with all 19 pinned heuristics. It checks:

1. complete catalog parity;
2. feasible validated output from all heuristics;
3. objective value not below the Wagner–Whitin exact optimum;
4. rejection of an exact-method ID by the heuristic catalog.

## Dependency continuity

Alpha.35 does not download or replace ULSAlgorithms. The validated alpha.34
baseline must already contain:

- repository-root `NuGet.Config`;
- tracked `external/nuget/ULSAlgorithms.1.1.0.nupkg`;
- package SHA-256
  `4eff21da87a9ff7649ee2e0f1c6f835b0af3cfa6bc08dd9ba91306fdd5ff2a3c`.

Any dependency drift aborts the alpha.35 preflight.
