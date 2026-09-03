# Setup transition semantics - Pack 08

Pack 08 separates three concepts that must not be conflated:

1. item setup (`SetupTime`, `FixedSetupCost`);
2. start-up of a setup sequence (`StartUpTime`, `StartUpCost`);
3. transition-state semantics (`SetupCarryOverPolicy`, directed changeovers).

The local model already contains start-up feature support. Pack 08 preserves it
and adds the missing generic transition-state representation.

## Carry-over

`Allowed` means a setup state may persist across planning-period boundaries.
`Forbidden` means no such persistence is represented.
`Unspecified` means no semantic claim is made.

LSI token:

```text
SET.CO
```

## Sequence-dependent setup

A directed `ProductionChangeover` represents a transition:

```text
fromItem -> toItem
```

on one work center. Time and cost are independent optional parameters.

LSI tokens:

```text
SET.SD.T
SET.SD.C
```

## Execution boundary

The standard big-bucket formulation must not silently ignore carry-over or
sequence-dependent changeovers. Until the scheduling formulation is extended,
`StandardLotSizingFormulation.CanBuild` returns false when either:

- carry-over is explicitly allowed; or
- at least one directed changeover is declared.

This is an admission guard, not a claim of solver support.

Pack 09 is responsible for executable scheduling semantics.
