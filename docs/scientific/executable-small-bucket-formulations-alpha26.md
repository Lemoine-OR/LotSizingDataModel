# Executable Small-Bucket Scheduling I — alpha.26

alpha.26 makes the canonical DLSP and CSLP classes solver-executable.

PLSP remains `Classifiable` because its within-bucket two-product / one-change
structure requires a distinct transition formulation.

## Mathematical state

For routing/item `r` and bucket `t`:

- `x[r,t] >= 0` — production quantity;
- `y[r,t] in {0,1}` — persistent setup state;
- `z[r,t] in {0,1}` — setup-state start, mathematical only.

DLSP additionally uses:

- `p[r,t] in {0,1}` — full-bucket production activation, mathematical only.

The existing normalized `ProductionDecision.Setups` persists `y`.

`z` and `p` are exactly derivable formulation auxiliaries and are therefore not
added as business decisions.

## Single setup state

For every bucket:

`sum_r y[r,t] <= 1`.

## Setup-state start

With no initial setup state:

`z[r,1] = y[r,1]`.

For `t > 1`:

`z[r,t] = 1` iff `y[r,t]=1` and `y[r,t-1]=0`.

The MILP linearizes this relation with one lower and two upper inequalities.

This permits the same setup state to persist across idle buckets without
charging another setup-start cost.

## CSLP production

Let `a[r,t]` be unit capacity consumption and `U[t]` the single scheduling
resource capacity.

`a[r,t] x[r,t] <= U[t] y[r,t]`.

Production is continuous up to available bucket capacity.

## DLSP production

`a[r,t] x[r,t] = U[t] p[r,t]`

and

`p[r,t] <= y[r,t]`.

Thus production is zero or exactly full bucket capacity, while an existing
setup state may persist through an idle bucket.

## Setup costs

`FixedSetupCost[r,t]` multiplies `z[r,t]`, not `y[r,t]`.

This is intentionally different from the generic standard lot-sizing
formulation, where a setup activation is a period-local production activation.

## First executable scope

The alpha.26 technical applicability contract rejects, until a dedicated
interaction is implemented:

- setup times;
- start-up cost/time;
- minimum/maximum/multiple lot-size constraints;
- additional production capacity;
- explicit initial setup state;
- sequence-dependent changeover cost/time;
- explicit maximum setup-transition constraints;
- multi-site production;
- `GroupingConstraint`.

The last point is essential: the project `GroupingConstraint` remains a
minimum spacing rule between setup occurrences and is not reinterpreted as a
constraint on persistent setup states.
