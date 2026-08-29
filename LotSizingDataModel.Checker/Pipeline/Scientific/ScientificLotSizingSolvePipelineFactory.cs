using LotSizingDataModel.Solver.Execution;
using LotSizingDataModel.Solver.Formulation;

namespace LotSizingDataModel.Checker.Pipeline.Scientific;

/// <summary>
/// Creates the scientific end-to-end wrapper around an already configured
/// technical solver service and its formulation registry.
/// </summary>
public static class ScientificLotSizingSolvePipelineFactory
{
    public static ScientificLotSizingSolvePipeline Create(
        ILotSizingSolverService solverService,
        MathematicalModelFormulationRegistry formulationRegistry) =>
            new(
                solverService,
                formulationRegistry);
}
