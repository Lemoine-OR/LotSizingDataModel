# LSI operational extensions - Pack 06

Pack 06 closes two concrete Core gaps that were historically represented in
the classification vocabulary but not necessarily in the local source model.

## Maximum lot size

`ProductionRouting.MaximumLotSize` is an optional period-dependent parameter.

Semantics:

```text
0 <= production quantity <= MaximumLotSize[t]
```

when the corresponding routing/setup is active according to the formulation.

A null property means that the extension is not represented. A represented
value of zero is active and forbids production in that period.

LSI mapping:

```text
LOT.MAX=1
```

with a temporal profile when values are available.

## Supplier capacity

`SupplierDelivery.CapacityConstraint` represents the maximum procurement
quantity available from one supplier for the item/destination relationship.

A null property means uncapacitated procurement at that relationship.

LSI mapping:

```text
CAP.S=1
```

with a temporal profile when values are available.

## Lifecycle requirements

Both parameters participate in:

- planning-horizon detection;
- resize operations;
- clear/reset operations;
- property-change propagation where applicable;
- XML serialization;
- automatic feature extraction.

## Scientific boundary

This pack changes data-model expressiveness and classification only. It does
not claim that every solver formulation already executes these extensions.
Executable formulation support remains a separate capability question.
