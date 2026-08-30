# Executable PLSP — alpha.27

Scientific basis: Drexl and Haase (1995), *Proportional lotsizing and
scheduling*, International Journal of Production Economics 40(1), 73–87,
DOI 10.1016/0925-5273(95)00040-U.

The canonical PLSP is a small-bucket model with at most one setup operation
inside one period. Consequently, at most two items may be produced: the item
whose setup state enters the period and the item whose setup state leaves it.

## Variables

For routing/item `r`, period `t`:

- `x[r,t] >= 0`: production quantity;
- `y[r,t] in {0,1}`: setup state carried out of period `t`;
- `z[r,t] in {0,1}`: setup start, mathematical only;
- `q[r,t] in {0,1}`: positive-production activation, mathematical only.

## State

`sum_r y[r,t] = 1`.

With no modeled initial setup state, `y[r,0]=0`.

The alpha.26 exact setup-start linearization remains valid:
`z[r,t]=1` iff the outgoing state becomes active from an inactive incoming
state.

## Production admissibility

For period 1:

`q[r,1] <= y[r,1]`.

For `t>1`:

`q[r,t] <= y[r,t-1] + y[r,t]`.

And:

`a[r,t] x[r,t] <= U[t] q[r,t]`.

The ordinary work-center capacity family still enforces:

`sum_r a[r,t] x[r,t] <= U[t]`.

Thus only the incoming and outgoing setup-state items may be produced, and the
aggregate production still fits in the bucket.

## Item cardinality

The generic period-dependent business constraint is now enforced for all
small-bucket formulations:

`sum_r q[r,t] <= MaximumProducedItemCount[t]`.

This closes the alpha.26 gap where only the maximum value over the whole
horizon was used for applicability.

## Setup-transition limit

The PLSP representation intrinsically permits at most one transition per
bucket. When `MaximumSetupCount[t]=0`, alpha.27 explicitly imposes:

`y[r,t]=y[r,t-1]` for every `r`.

The first period requires `MaximumSetupCount[1]=1` while no initial setup state
is modeled.

## Objective

Fixed setup cost is charged on `z`, not on persistent state `y`.

## Still unsupported in executable PLSP

- initial setup state;
- setup times / start-up times;
- sequence-dependent changeover costs/times;
- multi-site production;
- production additional capacity;
- minimum/maximum/multiple lot-size extensions;
- GroupingConstraint.

GroupingConstraint remains a spacing rule on setup occurrences and is never
reinterpreted as a PLSP setup-state restriction.
