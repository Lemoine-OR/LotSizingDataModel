using LotSizingDataModel.Solver.Common;
using LotSizingDataModel.Solver.Execution;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Algorithms.Metaheuristics;

public sealed class MetaheuristicsPlatformMathematicalResultProjector
{
    public MathematicalModelSolveResult Project(
        MathematicalModel model,
        DebConstraintGaBridgeResult bridgeResult,
        string runName = "")
    {
        ArgumentNullException.ThrowIfNull(
            model);

        ArgumentNullException.ThrowIfNull(
            bridgeResult);

        if (!bridgeResult.IsFeasible)
        {
            throw new InvalidOperationException(
                "Only a feasible metaheuristic incumbent can be projected.");
        }

        MathematicalVariable[] variables =
            model.Variables
                .OrderBy(
                    variable =>
                        variable.Id)
                .ToArray();

        if (variables.Length !=
            bridgeResult.ModelVariableValues.Length)
        {
            throw new InvalidOperationException(
                "Metaheuristic incumbent dimension does not match the mathematical model.");
        }

        var result =
            new MathematicalModelSolveResult
            {
                RunName =
                    runName ?? string.Empty,

                FormulationId =
                    "metaheuristics-platform",

                SolverKind =
                    SolverKind.Unknown,

                SolverName =
                    $"MetaheuristicsPlatform/{bridgeResult.AlgorithmId}",

                SolverVersion =
                    MetaheuristicsPlatformBridgeProvenance.Version,

                TerminationReason =
                    SolverTerminationReason.Feasible,

                HasFeasibleSolution =
                    true,

                IsOptimal =
                    false,

                ObjectiveValue =
                    bridgeResult.ObjectiveValue,

                BestBound =
                    null,

                RelativeGap =
                    null,

                AbsoluteGap =
                    null,

                SolveDuration =
                    bridgeResult.Duration
            };

        for (int index = 0;
             index < variables.Length;
             index++)
        {
            MathematicalVariable variable =
                variables[index];

            result.AddVariableValue(
                new MathematicalVariableValue(
                    variable.Id,
                    bridgeResult.ModelVariableValues[index],
                    variable.Name,
                    variable.DomainKey));
        }

        result.AddDiagnostic(
            $"MetaheuristicsPlatform algorithm={bridgeResult.AlgorithmId}; feasible={bridgeResult.IsFeasible}; optimalityClaim=false; seed={bridgeResult.Seed}.");

        result.EnsureValid();

        return result;
    }
}
