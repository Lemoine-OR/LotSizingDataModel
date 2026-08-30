# Periodic Operating Expenditure Budget — alpha.23

alpha.23 introduces a real executable financial constraint:

`PeriodicOperatingExpenditureBudget`.

For each period `t`, the standard formulation builds:

`sum(c_j x_j : c_j > 0 and period(x_j)=t) <= B_t`

where the coefficients `c_j` are taken directly from the already assembled
economic objective.

## Scope

This is an **operating expenditure envelope**, not a complete treasury/cash
flow model.

Included:

- positive economic objective coefficients;
- only variables carrying a canonical `period` domain-key segment.

Excluded by definition:

- negative revenue coefficients: revenue does not replenish this envelope;
- positive objective terms without a period segment;
- financing flows, payment delays, borrowing, interest and balance-sheet
  mechanics.

These excluded concepts require a future dedicated cash-flow model.

The constraint deliberately reuses the mathematical objective instead of
duplicating every production, inventory, procurement, transport and penalty
cost equation.

Universal beta feature remains:

`Fin`

A financial constraint does **not** imply a financial objective.
