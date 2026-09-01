# Feasibility & Checker Expansion — alpha.40

## Separation of responsibilities

Alpha.40 makes three different questions explicit:

1. structural validity: handled by existing validators/checkers;
2. intrinsic pre-solve impossibility: handled by `InstanceFeasibilityAnalyzer`;
3. feasibility of one returned candidate: handled by independent solution/model checkers.

The intrinsic analyzer is deliberately conservative. Its status set contains
only `Unknown` and `Infeasible`. It never promotes absence of a contradiction
to a proof of feasibility.

## Bound-interval proof

For every enabled linear constraint, alpha.40 computes a safe attainable
interval for the left-hand side from variable bounds.

- `lhs <= rhs` is impossible when `min(lhs) > rhs`;
- `lhs >= rhs` is impossible when `max(lhs) < rhs`;
- `lhs = rhs` is impossible when `rhs` is outside `[min(lhs), max(lhs)]`.

Infinite or conflicting infinite contributions are conservatively widened to
`[-infinity,+infinity]` rather than guessed.

This analysis is a necessary-condition proof only. It can miss infeasibilities
caused by integrality or interaction among several constraints, but it must not
create false `Infeasible` conclusions.

## Instance facade

`InstanceFeasibilityAnalyzer` can build any supplied
`IMathematicalModelFormulation` before applying the intrinsic analysis. The
standard formulation has a convenience entry point. Alpha.39 closed-loop
streams are automatically decorated onto the built model before analysis.

## Independent mathematical result checker

`MathematicalModelSolveResultFeasibilityChecker` independently checks:

- complete variable values;
- finite values and variable bounds;
- integer/binary domains;
- every enabled linear constraint.

Missing variable values produce `PartiallyEvaluated`, not a false
`Infeasible`. A solver's own `HasFeasibleSolution` flag is therefore not used
as proof by itself.

## Closed-loop normalized solution

Alpha.40 adds serializable `ClosedLoopDecision` objects to the partial
`LotSizingSolution` extension. `ClosedLoopLotSizingSolutionMapper` maps the
alpha.39 mathematical recovery/disposal results into these normalized
business decisions.

`ClosedLoopSolutionFeasibilityChecker` independently verifies:

- return conservation;
- recovery-yield identity;
- recovery capacity;
- stream identity and horizon completeness.

A missing closed-loop decision yields `PartiallyEvaluated`; an explicit
violation yields `Infeasible`.

## Diagnostic namespaces

Pre-solve intrinsic diagnostics use stable `LSDM-FEAS-00x` codes.
Mathematical-candidate checks use `LSDM-FEAS-SOL-00x`.
Closed-loop semantic checks use `LSDM-FEAS-CL-00x`.
