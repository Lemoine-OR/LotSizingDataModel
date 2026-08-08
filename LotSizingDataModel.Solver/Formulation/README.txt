Generic production/setup Big-M estimator

Replace/add these files in LotSizingDataModel.Solver/Formulation:

ADD
  IProductionSetupBigMEstimator.cs
  ProductionSetupBigMEstimate.cs
  GenericProductionSetupBigMEstimator.cs

REPLACE
  ProductionSetupLinkConstraintFamilyBuilder.cs
  StandardLotSizingFormulationOptions.cs

Design
------
The estimator is generic: it does not know Dellaert-Jeunet and it does not
assume a serial BOM.

For a routing producing item i, it computes a full-horizon gross requirement:

  gross(i) = external demand(i)
           + sum over parents j using i [ quantity(i,j) * gross(j) ]

This works for arbitrary acyclic BOMs, including trees, assemblies and shared
components. All demand records for an item are aggregated.

The full horizon is deliberately used for every period. This is conservative
when backlog or lead times are present and is still dramatically tighter than
1e9 in ordinary lot-sizing instances.

The estimator also enlarges the bound for:
  - safety-stock requirements;
  - period-specific minimum lot size;
  - period-specific lot-size multiple.

If the structural calculation cannot produce a finite bound, the existing
ProductionSetupBigM option is used only as a fallback. Its default is reduced
from 1e9 to 1e6, but applications can configure it explicitly.

For the current DJ 5-item serial instance, gross requirement is 1105 for each
item, so the generated links should become approximately:

  x_r*_t* - 1105 y_r*_t* <= 0

instead of:

  x_r*_t* - 1000000000 y_r*_t* <= 0

The constraint description records the actual M and its derivation, so the
existing mathematical-model.txt exporter will show the chosen value.


Compatibility correction
------------------------
The actual SafetyStock type available in the current Core assembly is accessed
through its one-based time-series indexer:

    inventory.SafetyStock[period]

rather than GetMinimumInventory(period).

This pack therefore uses the indexer and compiles against the current project.
The missing Solver.dll / Solver.Cplex.dll metadata errors are downstream
errors caused by the Solver project failing to compile; they should disappear
once this source error is fixed.
