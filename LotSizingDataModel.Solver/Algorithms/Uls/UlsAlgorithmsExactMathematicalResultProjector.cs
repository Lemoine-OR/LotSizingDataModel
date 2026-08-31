using LotSizingDataModel.Solver.Common;
using LotSizingDataModel.Solver.Execution;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Algorithms.Uls;

/// <summary>
/// Projects a validated ULSAlgorithms exact result onto the
/// already-built canonical LotSizingDataModel mathematical model.
/// </summary>
public sealed class UlsAlgorithmsExactMathematicalResultProjector
{
    public MathematicalModelSolveResult Project(
        MathematicalModel model,
        UlsAlgorithmsExactProblemData problem,
        UlsAlgorithmsExactBridgeResult externalResult,
        string runName = "")
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(problem);
        ArgumentNullException.ThrowIfNull(externalResult);

        if (externalResult.Horizon != problem.Horizon)
        {
            throw new InvalidOperationException(
                "External exact result horizon does not match the extracted ULS problem.");
        }

        var result =
            new MathematicalModelSolveResult
            {
                RunName =
                    runName ?? string.Empty,

                FormulationId =
                    "ulsalgorithms-exact",

                SolverKind =
                    SolverKind.Unknown,

                SolverName =
                    $"ULSAlgorithms/{externalResult.SolverId}",

                SolverVersion =
                    externalResult.SolverVersion,

                TerminationReason =
                    SolverTerminationReason.Optimal,

                HasFeasibleSolution =
                    true,

                IsOptimal =
                    true,

                ObjectiveValue =
                    externalResult.ObjectiveValue,

                BestBound =
                    externalResult.ObjectiveValue,

                RelativeGap =
                    0.0,

                AbsoluteGap =
                    0.0,

                SolveDuration =
                    externalResult.SolveDuration
            };

        int projectedCount =
            0;

        foreach (MathematicalVariable variable
                 in model.Variables)
        {
            MathematicalDomainKey key =
                MathematicalDomainKey.Parse(
                    variable.DomainKey);

            int period =
                key.GetRequiredInt32(
                    "period");

            if (period < 1 ||
                period > problem.Horizon)
            {
                throw new InvalidOperationException(
                    $"Variable '{variable.Name}' has period {period}, outside the exact ULS horizon.");
            }

            int index =
                period - 1;

            double value =
                key.Category switch
                {
                    MathematicalDecisionCategory.Production =>
                        externalResult.ProductionQuantities[index],

                    MathematicalDecisionCategory.Setup =>
                        externalResult.SetupDecisions[index]
                            ? 1.0
                            : 0.0,

                    MathematicalDecisionCategory.Inventory =>
                        externalResult.EndingInventories[index],

                    MathematicalDecisionCategory.Delivery =>
                        problem.Demands[index],

                    _ =>
                        throw new NotSupportedException(
                            $"Cannot project external ULS result onto category '{key.Category}'.")
                };

            result.AddVariableValue(
                new MathematicalVariableValue(
                    variable.Id,
                    value,
                    variable.Name,
                    variable.DomainKey));

            projectedCount++;
        }

        if (projectedCount !=
            model.VariableCount)
        {
            throw new InvalidOperationException(
                "External exact projection is incomplete.");
        }

        result.AddDiagnostic(
            $"Exact external method={externalResult.SolverId}; ULSAlgorithms={externalResult.SolverVersion}; sourceCommit={UlsAlgorithmsExactMethodCatalog.SourceCommit}.");

        result.EnsureValid();

        return result;
    }
}
