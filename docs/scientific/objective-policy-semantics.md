# Objective Policy Semantics — alpha.23

The presence of several cost components is not a multiobjective model.

alpha.23 introduces explicit Core objective semantics:

- `OptimizationObjectiveKind`
  - Economic
  - Financial
  - Sustainability
  - ServiceLevel
- `ObjectiveAggregationMode`
  - Single
  - WeightedSum
  - Lexicographic
- `OptimizationObjectiveCriterion`
  - kind
  - enabled state
  - weight
  - priority
- `OptimizationObjectivePolicy`
  - criterion collection
  - aggregation mode

## Validation

Single:
- exactly one enabled criterion.

WeightedSum:
- at least two enabled criteria;
- at least one positive weight.

Lexicographic:
- at least two enabled criteria;
- distinct priorities.

Enabled criterion kinds are unique and cannot be `Unknown`.

## Current executable coverage

The standard formulation still has one scalar minimized economic objective.

Therefore alpha.23 supports:

`Single + Economic`

and explicitly rejects:

- single Financial objective;
- single Sustainability objective;
- single ServiceLevel objective;
- true multiple-objective policies.

Weighted-sum and lexicographic policy data are representable and detectable,
but no scalarizer/lexicographic solve engine is claimed yet.
