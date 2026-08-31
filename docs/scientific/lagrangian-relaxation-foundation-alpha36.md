# Generic Lagrangian Relaxation & Decomposition Foundation — alpha.36

## Purpose

Alpha.36 introduces mathematical infrastructure only. It is deliberately
independent of any downstream algorithm platform or problem-specific
multi-level lot-sizing project.

The foundation operates directly on the solver-independent
`MathematicalModel`.

## Convention

Alpha.36 currently supports **minimization models only**.

For each relaxed constraint, define

`g(x) = lhs(x) - rhs`.

The relaxed objective is

`L(x, lambda) = f(x) + lambda g(x)`.

To preserve a valid dual lower bound for a minimization problem, multiplier
domains are:

- `lhs <= rhs`  => `lambda >= 0`;
- `lhs >= rhs`  => `lambda <= 0`;
- `lhs  = rhs`  => `lambda` free.

Unsupported objective directions are rejected rather than silently applying
another sign convention.

## Source preservation

The source `MathematicalModel` is never modified.

`LagrangianRelaxationModelBuilder` clones it, disables only the selected
constraints in the clone, and adds the complete residual penalty including the
right-hand-side constant to the cloned objective.

## Residual and subgradient

`LagrangianConstraintResidualEvaluator` uses the single canonical residual
`lhs - rhs`.

`LagrangianSubgradientUpdater` performs

`lambda(next) = projection(lambda + step * residual)`

with projection onto the valid multiplier domain above.

The step size is supplied by the caller. Alpha.36 intentionally does not impose
a Polyak rule, diminishing schedule or problem-specific update policy.

## Bounds

`LagrangianBoundTracker` distinguishes:

- dual values as lower bounds;
- primal feasible values as upper bounds.

It retains the strongest bound of each type and rejects claims that violate
weak duality beyond a scaled numerical tolerance.

## Architectural boundary

This milestone provides reusable primitives only:

- multiplier-domain validation;
- explicit relaxation specifications;
- source-preserving model transformation;
- residual evaluation;
- projected subgradient update;
- dual/primal bound tracking.

It does **not** implement a problem-specific decomposition algorithm and does
not introduce a dependency on a downstream project.

Later algorithm projects may consume these contracts while keeping their own
decomposition rules, subproblem definitions and stopping criteria outside
LotSizingDataModel.
