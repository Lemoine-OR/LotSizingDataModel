# Formulation Scientific Compatibility — alpha.17

`alpha.17` connects the scientific classification stack to the
solver-independent mathematical-formulation layer.

The existing formulation API already has a technical gate:

`IMathematicalModelFormulation.CanBuild(instance)`

This is necessary but not sufficient as a scientific contract. A formulation
may technically attempt to build an instance even when some modeled feature
has no verified formulation family.

The new bridge therefore separates:

1. **scientific compatibility**;
2. **technical `CanBuild` compatibility**.

## Scientific compatibility states

- `Compatible`: canonical problem core is supported and every classified
  extension has verified support;
- `Incompatible`: a canonical class or extension is known unsupported;
- `Undetermined`: the capability profile does not yet establish support;
- `Blocked`: upstream scientific classification is blocked.

Automatic scientific selection only accepts `Compatible`.

`Undetermined` is never silently promoted to compatible.

## Standard formulation profile

The `standard` formulation profile supports all six currently executable
canonical lot-sizing classes.

Verified extension support is grounded in the builders registered by
`StandardLotSizingFormulationFactory` and associated formulation code,
including:

- initial inventory;
- safety stock;
- backlogging;
- setup times in work-center capacity;
- production lead times;
- minimum lot sizes;
- lot-size multiples;
- additional production/warehouse/transport capacity;
- procurement and supplier lead time;
- transportation and transport lead time;
- distribution;
- transport/warehouse capacity;
- multi-site physical resource enumeration.

Known unsupported extensions currently include:

- start-up costs;
- maximum lot size;
- supplier capacity;
- financial constraints;
- multiple objectives.

Unlisted capabilities remain `Undetermined`. For example alpha.17 deliberately
leaves generic `LostSales` undetermined even though the standard formulation
has shortage variables for specific distribution-sourcing semantics; the
feature-level equivalence has not yet been proven for every source case.

## Scientific selection

`ScientificFormulationSelectionService` can work from:

- a precomputed `ScientificClassificationResult`; or
- a concrete `LotSizingInstance`.

The instance overload additionally checks the formulation's existing
`CanBuild(instance)` contract.

This service does not remove or replace the existing technical formulation
selection service. It is a scientific preflight/filter layered before actual
model construction.
