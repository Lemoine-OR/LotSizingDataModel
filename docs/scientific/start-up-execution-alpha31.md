# v1.2.0-alpha.31 — Start-Up Execution & Scheduling Interaction

## Scope

This milestone executes the previously represented `StartUpCost` (`SU`) and
`StartUpTime` (`SUT`) concepts without conflating them with generic setup or
sequence-dependent changeover parameters.

A mathematical production start-up event is represented by the dedicated
category `auxiliaryProductionStartUp`.

## Event semantics

For an ordinary period-indexed formulation, the start-up variable is the exact
0-to-1 transition of the routing setup binary:

- `a[r,1] = y[r,1]`;
- `a[r,t] >= y[r,t] - y[r,t-1]`;
- `a[r,t] <= y[r,t]`;
- `a[r,t] <= 1 - y[r,t-1]`.

Small-bucket and GLSP scheduling already expose exact setup-start occurrences.
Their start-up variable is distinct but synchronized exactly with the
corresponding setup-start occurrence. This allows independent accounting of:

- `FixedSetupCost`;
- `SetupTime`;
- `StartUpCost`;
- `StartUpTime`;
- sequence-dependent changeover cost;
- sequence-dependent changeover time.

No parameter is reused as an alias for another mechanism.

## Executable support matrix

| Formulation | StartUpCost | StartUpTime |
|---|---|---|
| Standard MILP | Executable | KnownUnsupported |
| DLSP small-bucket | Executable | KnownUnsupported |
| CSLP small-bucket | Executable | Executable |
| PLSP small-bucket | Executable | Executable |
| GLSP macro/micro | Executable | Executable |

`StartUpTime` remains unsupported for DLSP because the current all-or-nothing
full-bucket equality has no validated residual-capacity reformulation for
additional start-up time. The milestone does not silently approximate this
case.

## Capacity and objective

Where executable:

- `StartUpCost[r,t] * a[r,t]` is added independently to the economic objective;
- `StartUpTime[r,t] * a[r,t]` is added independently to scheduling capacity.

For GLSP, all micro-period contributions are aggregated into the containing
macro-period capacity constraint. Sequence-dependent changeover contributions
remain separate and additive.

## Solution and checker

The start-up variable is mathematical-only. It is not duplicated as mutable
business state in `LotSizingSolution`.

The generic mathematical mapper accepts the category without creating duplicate
solution data. `MathematicalSolutionValueProjector` independently reconstructs
the start-up event from normalized production/setup state or normalized GLSP
micro-schedules.

## Guard

A formulation is not marked executable merely because `SU` or `SUT` appears in
notation. Executable status requires exact mathematical event semantics,
objective/capacity propagation where relevant, and an independent checker
projection.
