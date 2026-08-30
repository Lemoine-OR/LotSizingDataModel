# Small-Bucket Solution Checking — alpha.26

The normalized solution does not duplicate mathematical auxiliaries.

Persisted business decisions:

- production quantity -> `ProductionDecision.Quantities`;
- setup state -> `ProductionDecision.Setups`.

Mathematical-only variables:

- DLSP full-bucket production activation;
- setup-state start.

The independent `MathematicalSolutionValueProjector` reconstructs:

- setup start at `t=1` from the current setup state;
- later setup start from the transition `0 -> 1`;
- DLSP production activation from strictly positive production quantity.

Consequently the same generated MILP equations and objective can be checked
independently without storing redundant variables in `LotSizingSolution`.
