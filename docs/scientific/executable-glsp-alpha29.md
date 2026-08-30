# Executable GLSP — alpha.29

Scientific basis:

- Fleischmann, B. and Meyr, H. (1997), *The general lotsizing and scheduling problem*, OR Spectrum 19(1), 11–21, DOI 10.1007/BF01539800.
- Meyr, H. (2000), *Simultaneous lotsizing and scheduling by combining local search with dual reoptimization*, EJOR 120(2), 311–326.
- Guimarães, L., Klabjan, D. and Almada-Lobo, B. (2014), *Modeling lotsizing and scheduling problems with sequence dependent setups*, EJOR 239(3), 644–662.

The formulation keeps aggregate macro production `X[r,t]` for the established physical flow layer and adds micro production `x[r,s]`, setup state `y[r,s]` and exact nontrivial changeover `z[i,j,s]`.

`X[r,t] = sum_{s in S_t} x[r,s]`.

For every micro-period: `sum_r y[r,s] = 1` and production is linked to the active state.

For adjacent slots, including macro-period boundaries, `z[i,j,s]` is the exact binary product of the previous state `i` and current state `j`.

Macro capacity includes both micro production consumption and sequence-dependent changeover time. The objective adds sequence-dependent changeover cost.

No changeover is generated before the first modeled micro-period. Explicit initial setup state is therefore deliberately unsupported in alpha.29 rather than approximated.

Solver mapping writes normalized `WorkCenterSchedulingDecision` values; changeovers remain mathematical-only. The independent checker reconstructs micro production, setup states and changeovers from the normalized micro schedule.
