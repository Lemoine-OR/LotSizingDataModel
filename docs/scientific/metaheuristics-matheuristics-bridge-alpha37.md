# Metaheuristics / Matheuristics Bridge — alpha.37

Alpha.37 bridges LotSizingDataModel to MetaheuristicsPlatform 1.0.1, pinned to
source commit `0ab7521dc1f42f50209c8badea811502977b8409` and official binaries ZIP
SHA-256 `ce7d39c184e17965e64b2739516b62bd186ff4bf12bdedf570f1293e241f2404`.

The executable metaheuristic path uses the public Deb feasibility-rules GA
through `IContinuousConstrainedOptimizationProblem`. Continuous mathematical
variables remain continuous, integers are rounded deterministically, binaries
use a 0.5 threshold, fixed variables are removed from latent search dimensions,
and unbounded variables require explicit finite search-bound overrides.

External constrained inequalities are normalized so feasibility means a
non-positive residual: `lhs-rhs` for `<=`, `rhs-lhs` for `>=`; equalities use
`lhs-rhs`.

A stochastic feasible incumbent is never promoted to an exact result:
`IsOptimal=false`, and best bound / optimality gaps remain absent.

The executable matheuristic path implements the public
`IExactRepairMatheuristicDomain` contract and uses the pinned Local Branching
optimizer. Exact-repair submodels are cloned from the source model and support
OriginalObjective mode with fixings, tightened bounds, allowed-active binary
indices, exact binary Hamming-radius constraints and objective cutoffs.

The caller supplies exact and relaxation solve delegates, so LotSizingDataModel
continues to own solver selection and mathematical solution normalization.
Returned delegate values are independently checked against the generated
subproblem before becoming a matheuristic point.

Distance/proximity exact-repair objective modes and semi-continuous/semi-integer
latent encodings are explicitly deferred.
