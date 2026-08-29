# Maximum Lot Size Semantics — alpha.22

`MaximumLotSize` is a period-dependent routing parameter.

For routing `r` and period `t`:

`x[r,t] <= Qmax[r,t]`

A represented value of zero is active and forbids production in that period.
Only a null `MaximumLotSize` means that the extension is absent.

The semantic is independent from production-resource capacity, minimum lot
size and lot-size multiple.

Universal notation:

- `MaxLot`;
- `TP:MaxLot=<pattern>`.
