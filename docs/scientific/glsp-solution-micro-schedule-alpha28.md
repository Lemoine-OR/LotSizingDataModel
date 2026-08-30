# GLSP Solution Micro-Schedule — alpha.28

`LotSizingSolution` gains optional `WorkCenterSchedulingDecision` containers.

Each `ProductionMicroPeriodDecision` stores normalized facts only:

- `(macroPeriod, microPeriodIndex)`;
- setup item;
- optional production routing;
- non-negative production quantity.

No micro-period duration is persisted because variable GLSP micro-period length
is a mathematical consequence, not independent business data.

The structural checker verifies the work center, the `MicroPeriodCount[t]`
grid, item references, routing references, routing-item consistency and
routing/work-center consistency.
