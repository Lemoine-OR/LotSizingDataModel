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


## alpha.21 start-up extensions

The standard formulation explicitly reports both `StartUpCosts` and
`StartUpTimes` as `KnownUnsupported`.

This is intentional. Core/Instance can now represent and detect the
corresponding parameters, but the standard formulation does not yet contain a
binary transition variable for "a sequence of setups starts in period t".
Consequently scientific formulation selection rejects such an instance before
technical model construction instead of silently ignoring the extension.


## alpha.22 verified support

The standard formulation now has verified support for:

- `MaximumLotSize`;
- `SupplierCapacity`.

Evidence:

- `MaximumLotSizeConstraintFamilyBuilder`;
- `SupplierCapacityConstraintFamilyBuilder`;
- registration in `StandardLotSizingFormulationFactory`.

Both extensions move from `KnownUnsupported` to `supportedExtensions`.

Supplier capacity at this stage is supplier-item-destination capacity, not
aggregate shared supplier capacity.


## alpha.23 finance and objective semantics

`FinancialConstraints` moves to verified support for the currently represented
financial semantic: `PeriodicOperatingExpenditureBudget`.

The standard formulation additionally declares supported objective kinds.
Current verified objective support is:

- `Economic`.

A single `Financial`, `Sustainability`, or `ServiceLevel` objective is
scientifically incompatible with the standard formulation.

`MultipleObjectives` remains `KnownUnsupported` until a real weighted-sum or
lexicographic execution layer exists.

This prevents "one objective with many cost terms" from being mislabeled as
multiobjective optimization.
