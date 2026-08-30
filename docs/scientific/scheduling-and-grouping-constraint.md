# Scheduling Semantics vs GroupingConstraint

The existing `GroupingConstraint` is unchanged by alpha.24.

Its project invariant remains:

if the grouping value is `g` and a setup occurs in period `t`, no new setup may
occur in periods `t+1` through `t+g-1`; a setup is allowed again at `t+g`.

Equivalently, every sliding window of `g` setup binaries satisfies:

`sum(y) <= 1`.

This is a minimum spacing rule between production setups.

It is **not**:

- a maximum setup-count constraint;
- a small-bucket definition;
- a setup carry-over rule;
- a sequence-dependent changeover rule.

The new scheduling semantics must coexist with this rule without reinterpreting
it.
