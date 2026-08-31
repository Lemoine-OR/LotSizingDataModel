using LotSizingDataModel.Solver.Common;
using LotSizingDataModel.Solver.Execution;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Algorithms.Uls;

/// <summary>
/// Projects a validated feasible ULSAlgorithms heuristic result
/// onto a canonical LotSizingDataModel mathematical model.
/// </summary>
public sealed class UlsAlgorithmsHeuristicMathematicalResultProjector
{
    public MathematicalModelSolveResult Project(
        MathematicalModel model,
        UlsAlgorithmsExactProblemData problem,
        UlsAlgorithmsHeuristicBridgeResult heuristicResult,
        string runName = "")
    {
        ArgumentNullException.ThrowIfNull(
            model);

        ArgumentNullException.ThrowIfNull(
            problem);

        ArgumentNullException.ThrowIfNull(
            heuristicResult);

        if (heuristicResult.Horizon !=
            problem.Horizon)
        {
            throw new InvalidOperationException(
                "Heuristic result horizon does not match the canonical ULS problem.");
        }

        var result =
            new MathematicalModelSolveResult
            {
                RunName =
                    runName ?? string.Empty,

                FormulationId =
                    "ulsalgorithms-heuristic",

                SolverKind =
                    SolverKind.Unknown,

                SolverName =
                    $"ULSAlgorithms/{heuristicResult.SolverId}",

                SolverVersion =
                    heuristicResult.SolverVersion,

                TerminationReason =
                    SolverTerminationReason.Feasible,

                HasFeasibleSolution =
                    true,

                IsOptimal =
                    false,

                ObjectiveValue =
                    heuristicResult.ObjectiveValue,

                BestBound =
                    null,

                RelativeGap =
                    null,

                AbsoluteGap =
                    null,

                SolveDuration =
                    heuristicResult.SolveDuration
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
                    $"Variable '{variable.Name}' has period {period}, outside the heuristic ULS horizon.");
            }

            int index =
                period - 1;

            double value =
                key.Category switch
                {
                    MathematicalDecisionCategory.Production =>
                        heuristicResult.ProductionQuantities[index],

                    MathematicalDecisionCategory.Setup =>
                        heuristicResult.SetupDecisions[index]
                            ? 1.0
                            : 0.0,

                    MathematicalDecisionCategory.Inventory =>
                        heuristicResult.EndingInventories[index],

                    MathematicalDecisionCategory.Delivery =>
                        problem.Demands[index],

                    _ =>
                        throw new NotSupportedException(
                            $"Cannot project a heuristic ULS result onto category '{key.Category}'.")
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
                "Heuristic projection is incomplete.");
        }

        result.AddDiagnostic(
            $"Heuristic method={heuristicResult.SolverId}; status=Feasible; optimalityClaim=false; ULSAlgorithms={heuristicResult.SolverVersion}.");

        result.EnsureValid();

        return result;
    }
}
