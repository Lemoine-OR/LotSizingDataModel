# LSI explicit Core semantics

Pack 05 adds two optional semantic descriptors to the Core.

## Planning context

`SupplyChain.PlanningContext` stores semantics that cannot be inferred safely
from period-indexed data.

Current field:

```text
BucketMode = Unspecified | BigBucket | SmallBucket | MacroMicro | Hybrid
```

LSI mapping:

```text
Unspecified -> pi.BK=?
BigBucket   -> pi.BK=BB
SmallBucket -> pi.BK=SB
MacroMicro  -> pi.BK=MM
Hybrid      -> pi.BK=HYB
```

## Objective policy

`SupplyChain.ObjectivePolicy` explicitly declares business objective criteria.

Supported objective families:

```text
Economic
Financial
Sustainability
ServiceLevel
```

Supported aggregation:

```text
Single
WeightedSum
Lexicographic
```

LSI does not infer objective sense (`Minimize`/`Maximize`) because the current
business-level objective family alone is insufficient to determine a universal
mathematical sense. `gamma.SENSE` therefore remains `Unknown`.

## Compatibility

Both properties are nullable.

Old XML instances remain valid and produce:

```text
pi.BK=?
gamma{?}
```

until explicit semantics are supplied.
