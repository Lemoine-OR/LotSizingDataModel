# Scheduling Bucket Semantics

`SchedulingBucketMode` is generic and independent from historical model names.

- `BigBucket`: a planning period may contain several lots/setups.
- `SmallBucket`: short scheduling buckets are modeled directly.
- `MacroMicro`: each planning macro-period is subdivided into explicit
  micro-periods.

`MicroPeriodCount[t]` records the number of micro-periods in macro-period `t`.

`MaximumSetupCount[t]` is a generic upper bound on setup transitions in the
planning period. alpha.24 does not equate this parameter automatically with
Wolsey SB1/SB2: their exact historical mapping remains open until the source
semantics are fully secured.

Universal beta tokens:

- `Sched`;
- `Bucket:BB`;
- `Bucket:SB`;
- `Bucket:MM`;
- `MaxSetup`.
