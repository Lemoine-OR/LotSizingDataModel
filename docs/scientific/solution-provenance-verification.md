# Scientific Provenance Verification

Scientific provenance verification and solution feasibility are orthogonal.

A solution can be numerically feasible while provenance is missing, stale,
incomplete or contradictory. Coherent provenance likewise does not prove
feasibility.

## Recorded notation

Recorded detected notation is reinterpreted as a positive historical
specification against the current descriptor:

- `Exact` -> coherent;
- `Compatible` -> stale, not contradictory;
- `Incomplete` -> incomplete;
- `Contradiction` -> contradiction.

This supports forward evolution of the scientific classifier.

## Problem class

A different unique canonical problem class is a contradiction.

The same class with a changed `ExactCore` / `CompatibleExtension` status is
stale evidence.

## Formulation

The recorded formulation ID is reassessed with the current
`ScientificFormulationCompatibilityService`.

- `Compatible` -> accepted;
- `Undetermined` / `Blocked` -> incomplete;
- `Incompatible` -> contradiction.

Thus scientific support-contract changes remain auditable after a solution has
been generated.
