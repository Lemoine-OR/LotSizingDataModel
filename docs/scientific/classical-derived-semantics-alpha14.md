# Classical Derived Semantics — alpha.14

This is the first deliberately larger thematic increment after the notation
foundation.

It introduces generic derived mathematical conditions rather than historical
tokens.

## 1. Non-speculative production/holding costs

The generic adjacent condition is

`h_t + p_t - p_(t+1) >= 0`

for each supplied adjacent interval.

`NonSpeculativeCostConditionAnalyzer` computes all transformed holding margins
and applies the same explicit absolute/relative tolerance policy as temporal
pattern analysis.

Universal notation:

`Cost:NS`

Wolsey's historical `PROB=WW` maps to this generic condition. The universal
grammar does not contain a token named `WW`.

## 2. Zero-or-full-capacity production

When the modeled positive-production lower bound is the minimum lot size and
the upper bound is production capacity, equality

`MinLot_t = Capacity_t > 0`

forces positive production to equal full capacity.

`ZeroOrFullCapacityProductionAnalyzer` verifies this relation period by period.

Universal notation:

`ProdMode:0F`

This closes the zero/full-capacity dimension of Wolsey `DLSI` and `DLS`.
Their different initial-stock decision semantics remain explicit historical
mapping gaps.

## 3. Tri-state actual semantics

A partial specification is positive: omission means unconstrained.

Therefore a required derived condition needs three actual states:

- `Unknown` -> matcher returns `Incomplete`;
- `Satisfied` -> requirement is satisfied;
- `NotSatisfied` -> matcher returns `Contradiction`.

`UniversalDerivedSemantics` carries these analyses together with existing
generic temporal qualifiers.

## 4. No nominal flags

Neither scientific condition is added as a serialized Core flag.

Both are based on actual numerical series and have executable analyzers. This
preserves the project rule:

Scientific concept -> real data semantics -> typed analysis -> notation ->
matcher -> tests -> documentation.
