# Scientific Solution Provenance Schema v2

alpha.20 evolves scientific provenance from schema 1 to schema 2.

New fields:

- `lsdm.scientific.solutionMethodId`;
- `lsdm.scientific.solutionMethodCategory`;
- `lsdm.scientific.solverBackendKind`.

Schema 1 remains readable.

A schema-1 solution is not invalid; provenance verification reports it as
`Stale` because explicit method/backend evidence did not exist when it was
captured.

New pipeline-generated solutions use schema 2.

The current end-to-end pipeline records:

- method: `MILP-GENERAL`;
- actual backend: the concrete `SolverRunResult.SolverKind`.

This means provenance now distinguishes the mathematical formulation from the
algorithmic solution-method family and from the native backend.
