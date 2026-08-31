# v1.2.0-alpha.33 — Multiobjective & Financial Execution II

## Scope

This milestone executes the already represented `ObjectivePolicy` aggregation
modes without weakening the distinction between objective kinds and
formulations.

The executable criteria in alpha.33 are:

- **Economic** — the existing economic mathematical objective;
- **Financial** — minimization of negative terminal cash balance, equivalent to
  maximizing terminal cash.

`Sustainability` and `ServiceLevel` remain explicitly unsupported because the
required generic business data and exact executable criterion semantics are not
yet present. The roadmap's conceptual label "Service" maps to the established
Core identifier `OptimizationObjectiveKind.ServiceLevel`; alpha.33 does not
invent a second `Service` enum member.

## WeightedSum

`ObjectivePolicy.ExecutionCriteria` provides explicit:

- criterion kind;
- strictly positive weight;
- priority;
- lexicographic absolute tolerance.

WeightedSum creates one scalar minimization objective:

`sum_k weight[k] * normalizedCriterion[k]`.

No default weight is inferred from criterion order.

## Lexicographic

Criteria are ordered by explicit unique priorities.

Each stage is solved to **proven optimality**. Before the next stage, the
previous criterion receives a preservation constraint:

`criterion <= optimum + absoluteTolerance`.

A merely feasible or gap-limited previous stage is not accepted as exact
lexicographic execution.

## Financial cash flow

`CashFlowPolicy` is separate from the periodic OPEX budget. It contains:

- initial cash balance;
- receipt delay;
- disbursement delay;
- fixed net cash flow by period;
- minimum cash balance by period;
- solvency enforcement flag.

Decision-dependent monetary flows are derived from the normalized economic
objective:

- positive coefficient -> disbursement;
- negative coefficient -> receipt.

The domain-key period provides the economic event period; receipt/disbursement
delay determines the cash due period.

The financial horizon is extended by the maximum delay so post-planning-horizon
payments/collections created by in-horizon decisions remain represented.

The financial criterion is terminal cash at this extended horizon.

## OPEX and scheduling

When `PeriodicOperatingExpenditureBudget` is active, alpha.33 adds a complete
periodic positive-cost envelope derived from the economic objective itself.

Therefore setup, start-up and sequence-dependent changeover costs are included
whenever they are real positive objective terms carrying a period domain key.

This avoids maintaining an error-prone hard-coded list of cost families.

Initial-inventory period-zero cost is intentionally outside periodic OPEX.

## Solution and checker

Cash balances are mapped into `LotSizingSolution.CashBalances`.

`MathematicalSolutionValueProjector` independently reconstructs mathematical
cash variables from this normalized trace.

## Explicit gaps

- Sustainability objective: KnownUnsupported.
- ServiceLevel objective: KnownUnsupported.
- SalesOption execution remains the alpha.32 open item.
- The standard `LotSizingSolverService` remains the scalar legacy path.
  `MultiObjectiveLotSizingSolverService` is the executable multiobjective path.
- Resolution-plan automatic dispatch to the new service may be consolidated
  with algorithm-adapter selection, but no unsupported policy is silently
  routed through the scalar service.

## Extension coverage

- EXT-OBJ-01 WeightedSum: executable.
- EXT-OBJ-02 Lexicographic: executable for supported criteria.
- EXT-OBJ-03 Financial: executable as terminal cash with CashFlowPolicy.
- EXT-OBJ-04 Sustainability: remains open.
- EXT-OBJ-05 Service: remains open.
- EXT-FIN-01 OPEX + scheduling: implemented through generic positive-cost
  envelope.
- EXT-FIN-02 cash-flow/treasury: executable foundation with timing and solvency.
