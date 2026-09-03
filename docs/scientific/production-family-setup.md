# Production family setup semantics

## Scope

`ProductionSetupFamily` represents a shared setup activation associated with
several manufactured items on one work center.

It is explicitly distinct from:

- a commercial product family;
- a bill-of-material grouping;
- `GroupingConstraint`;
- an item-level setup;
- a production campaign.

## Mathematical semantics

For family `f`, member item `i`, and period `t`:

```text
y[i,t] <= w[f,t]
```

where `y` is the existing item/routing setup activation and `w` is the
shared family setup activation.

If the family has a positive setup time `v[f,t]`, work-center capacity includes:

```text
... + v[f,t] * w[f,t] <= capacity[t]
```

No family setup cost is introduced by this scope.

## Normalized solution semantics

Because this scope has:

- no family setup cost;
- no family state persistence;
- no independent constraint that rewards `w=1`;

the canonical normalized family setup activation is derivable as:

```text
w[f,t] = OR(y[i,t] for member items i produced on the family work center)
```

The mathematical MILP still contains `w[f,t]` to linearize the shared setup
relation. `LotSizingSolution` does not duplicate the derived binary series.

If future scientific models add family setup costs, carry-over, minimum run
lengths, or other independent family-state semantics, this design decision must
be revisited explicitly.

## LSI

Current tokens:

```text
SET.FAM
SET.FAM.T
```

The historical base family remains unchanged. For example a capacitated
multi-level instance remains `MLCLSP` with `SET.FAM=1`.

## Membership

An item may belong to more than one setup family, including on the same work
center. In that case all applicable family setups are activated and all
corresponding setup times consume capacity. This is mathematically coherent and
avoids imposing a MULTILSB-specific exclusivity rule on the generic model.

## Backward compatibility

The family collection is empty by default and is not serialized when empty.
Existing instance XML therefore does not acquire an empty
`productionSetupFamilies` node merely because the model supports the extension.
