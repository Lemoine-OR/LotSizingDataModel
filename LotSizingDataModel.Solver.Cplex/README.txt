CPLEX 20.1 API compatibility fix

This pack fixes the compile errors reported against the current CPLEX API.

1. ObjectiveSense ambiguity

ILOG.Concert also defines ObjectiveSense, while the generic solver model
defines LotSizingDataModel.Solver.Modeling.ObjectiveSense.

CplexModelTranslator now uses the explicit alias:

    using GenericObjectiveSense =
        global::LotSizingDataModel.Solver.Modeling.ObjectiveSense;

2. SolverProgressStage.Optimizing

The actual LotSizingDataModel.Solver.Common.SolverProgressStage enum does
not contain Optimizing. The CPLEX adapter now reports:

    SolverProgressStage.Searching

during CPLEX.Solve().

3. CplexSubStatus

The CPLEX API available to the project does not expose a CplexSubStatus
property. The adapter now uses:

    cplex.GetCplexStatus()

for the detailed CPLEX termination status.

4. BestObjValue

The API does not expose BestObjValue as a property. The adapter now uses:

    cplex.GetBestObjValue()

which is compatible with the older CPLEX .NET API family.

Replace:
  CplexModelTranslator.cs
  CplexSolverAdapter.cs
