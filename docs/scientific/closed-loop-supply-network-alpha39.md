# Closed-loop Supply Network — alpha.39

## Scope

Alpha.39 introduces an explicit, serializable closed-loop extension at the
`LotSizingInstance` level while preserving the historical forward
`SupplyChain` schema.

A `ClosedLoopReturnStream` identifies:

- the returned item;
- the distribution center at which the return stream originates;
- the warehouse receiving recovered usable units;
- exogenous returned quantity per period;
- optional recovery capacity;
- collection, recovery and disposal unit costs;
- a recovery yield in ]0,1].

## Physical conservation

For every stream and period alpha.39 creates:

- `closedLoopRecoveryInput`;
- `closedLoopDisposal`.

The mandatory return-allocation equation is

`recoveryInput + disposal = returnedQuantity`.

Returned quantity is exogenous. It is not represented as a negative demand and
the forward transport model is not reversed implicitly.

## Recovery yield and inventory balance

Recovered usable output is

`recoveryYield * recoveryInput`.

The existing standard inventory balance uses the sign convention

`inventory(t) - inventory(t-1) - inflows + outflows = fixed inflows`.

Therefore alpha.39 injects recovered output with coefficient
`-recoveryYield` into the unique standard inventory balance associated with
the target item / warehouse / period.

The balance is located through the canonical inventory domain key and a
structural `+1` current-inventory coefficient check. Constraint-name prefix is
used only to restrict the search to the standard inventory-balance family, not
as the sole identity mechanism.

## Cost semantics

Collection cost applies to the exogenous returned quantity and is therefore an
objective constant.

Recovery and disposal costs multiply their respective decision variables.

This separates fixed return acquisition from endogenous allocation decisions.

## Source preservation

`ClosedLoopSupplyNetworkModelDecorator` clones the supplied
`MathematicalModel`. The forward model is never modified in place.

## Decision projection

`ClosedLoopDecisionProjector` exposes the mathematical decisions as
`ClosedLoopDecisionSnapshot` objects containing:

- recovery input;
- disposal quantity;
- recovered usable output.

The generic `LotSizingSolution` mapping and Checker integration are
deliberately deferred to alpha.40, whose roadmap purpose is feasibility and
checker expansion.

## Explicit boundaries

Alpha.39 does not infer return rates from sales or demand. Returned quantities
must be supplied explicitly.

It also does not reinterpret forward transport lanes as reverse lanes.
Reverse transport-resource modeling remains a separate future extension if
physical return transportation is required.
