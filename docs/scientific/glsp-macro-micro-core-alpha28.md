# GLSP Macro/Micro Scheduling Core — alpha.28

Scientific basis:

- Fleischmann, B. and Meyr, H. (1997), *The general lotsizing and
  scheduling problem*, OR Spectrum 19(1), 11–21.
- Drexl, A. and Kimms, A. (1997), *Lot sizing and scheduling — Survey and
  extensions*, European Journal of Operational Research 99(2), 221–235,
  DOI 10.1016/S0377-2217(97)00030-1.

The ordinary planning horizon remains the macro-period horizon.
`MicroPeriodCount[t]` is the sole instance source of the ordered micro-period
slots within macro-period `t`.

A slot is `(macroPeriod, microPeriodIndex)`. No fixed duration is stored:
classical GLSP micro-period lengths are endogenous while macro-period capacity
is fixed.

Canonical alpha.28 GLSP membership requires integrated scheduling,
`MacroMicro`, an explicit grid, variable micro-period length, one item/setup
state per micro-period, multiple items, single level, one scheduling resource,
deterministic demand, production and shared production capacity.

Sequence-dependent changeover semantics and setup-state variants remain
extensions.

GLSP becomes `Classifiable`, not `Executable`.
