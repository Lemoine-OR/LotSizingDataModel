# Generic Problem Semantics — alpha.13

This increment closes only gaps already supported by real LotSizingDataModel
semantics.

## Production-capacity regime

`ProductionCapacityRegime` is derived from current typed descriptors:

- `NotApplicable`;
- `Uncapacitated`;
- `Constant`;
- `TimeVarying`.

No serialized source flag is added.

Universal notation introduces `Uncap:P`, because omission cannot mean
"explicitly uncapacitated" under positive-constraint matching.

## Minimum-lot temporal behavior

`UniversalTemporalParameter.MinimumLotSize` adds:

`TP:MinLot=<Z|C|NI|ND|G>`

Thus Wolsey `LB(C)` maps exactly to:

`MinLot,TP:MinLot=C`

## Deliberately deferred Wolsey semantics

The following remain explicit mapping gaps because they require genuine model
extensions rather than notation-only flags:

- Wagner-Whitin cost condition;
- DLSI/DLS zero-or-full-capacity production;
- variable versus fixed/no initial-stock decision;
- start-up times;
- additional sales;
- exact machine/bucket/count semantics;
- sequence-dependent changeover times/costs.

Those future extensions must follow the full propagation contract:
Core -> descriptors -> notation -> mathematical model -> solver -> solution ->
checkers -> tests -> documentation.
