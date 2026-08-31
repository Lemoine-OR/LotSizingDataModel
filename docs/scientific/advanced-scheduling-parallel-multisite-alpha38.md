# Advanced Scheduling — Parallel / Multi-site Coordination — alpha.38

## Scope

Alpha.38 closes a first executable layer of `EXT-SCHED-15` without changing
the meaning of existing routings or setup states.

The historical `ProductionRouting.WorkCenters` collection continues to mean
the work centers used by that routing. Multiple work centers inside one routing
are **not** automatically reinterpreted as alternative parallel machines.

Parallel / multi-site structure is instead identified from multiple
`ProductionRouting` objects producing the same item.

## Topology analysis

`ParallelSchedulingTopologyAnalyzer` groups routings by `ItemId` and records:

- exact routing identifiers;
- exact plant identifiers;
- whether the alternatives span multiple sites;
- whether multiple alternative routings coexist inside one plant.

This is derived information only. No historical XML semantics are rewritten.

## Executable coordination

`ParallelRoutingSetupStartModelDecorator` adds an optional upper bound on the
number of simultaneous setup-start occurrences among alternative routings of
the same item.

Two coordination scopes are supported:

- `AcrossAllSites`: one constraint coordinates all routings of the item;
- `WithinEachPlant`: routing setup-starts are coordinated independently by
  plant.

The decorator is source-preserving: it clones `MathematicalModel`.

## Critical semantic guard

The decorator consumes only canonical variables whose domain-key category is
`setup` and which contain both `routing` and `period`.

It deliberately ignores persistent setup-state auxiliaries.

Therefore alpha.38 does not change the validated GroupingConstraint invariant:
GroupingConstraint remains a minimum spacing between **setup-start
occurrences**, never a restriction on persistent setup state.

## What alpha.38 does not claim

This milestone does not yet reinterpret the work-center list as machine
alternatives and does not split one routing's production quantity among
parallel work centers.

A complete parallel-machine quantity/capacity formulation requires explicit
resource-assignment semantics, including sequence and setup-time allocation,
and must not be approximated by the existing sequential routing semantics.

The current milestone nevertheless provides an executable multi-routing /
multi-site coordination mechanism that composes with existing capacities,
additional capacity, lot-size restrictions and scheduling formulations.
