using System.Diagnostics;
using ULSAlgorithms;
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Catalog;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;
using ULSAlgorithms.Validation;

namespace LotSizingDataModel.Solver.Algorithms.Uls;

/// <summary>
/// Executes exactly one pinned public ULSAlgorithms heuristic.
/// </summary>
public sealed class UlsAlgorithmsHeuristicBridge
{
    public UlsAlgorithmsHeuristicBridgeResult Solve(
        UlsAlgorithmsExactProblemData problemData,
        string solverId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            problemData);

        problemData.EnsureValid();

        UlsSolverDescriptor descriptor =
            UlsAlgorithmsHeuristicCatalog.GetRequired(
                solverId);

        IUlsSolver solver =
            UlsSolverFactory.Create(
                descriptor.Id);

        if (solver.Kind !=
            UlsSolverKind.Heuristic)
        {
            throw new InvalidOperationException(
                $"ULSAlgorithms factory returned a non-heuristic solver for '{descriptor.Id}'.");
        }

        var problem =
            new UlsProblem(
                problemData.Demands,
                problemData.SetupCosts,
                problemData.UnitProductionCosts,
                problemData.HoldingCosts);

        cancellationToken.ThrowIfCancellationRequested();

        var stopwatch =
            Stopwatch.StartNew();

        UlsSolveResult solveResult =
            solver.Solve(
                problem,
                cancellationToken);

        stopwatch.Stop();

        if (solveResult.Status !=
            UlsSolveStatus.Feasible)
        {
            throw new InvalidOperationException(
                $"Pinned heuristic '{descriptor.Id}' returned status '{solveResult.Status}' instead of Feasible.");
        }

        if (!solveResult.HasSolution ||
            solveResult.Solution is null)
        {
            throw new InvalidOperationException(
                $"Pinned heuristic '{descriptor.Id}' returned no feasible solution.");
        }

        UlsSolution solution =
            solveResult.Solution;

        UlsSolutionValidationResult validation =
            UlsSolutionValidator.Validate(
                problem,
                solution);

        if (!validation.IsFeasible)
        {
            throw new InvalidOperationException(
                $"ULSAlgorithms validator rejected heuristic '{descriptor.Id}'.");
        }

        if (solution.Horizon !=
            problemData.Horizon)
        {
            throw new InvalidOperationException(
                $"Heuristic '{descriptor.Id}' returned horizon {solution.Horizon}, expected {problemData.Horizon}.");
        }

        double objectiveValue =
            solveResult.ObjectiveValue ??
            solution.TotalCost;

        double reportedDifference =
            Math.Abs(
                objectiveValue -
                solution.TotalCost);

        double validatedDifference =
            Math.Abs(
                objectiveValue -
                validation.RecomputedTotalCost);

        double scale =
            Math.Max(
                1.0,
                Math.Abs(objectiveValue));

        if (reportedDifference >
                1.0e-7 * scale ||
            validatedDifference >
                1.0e-7 * scale)
        {
            throw new InvalidOperationException(
                $"Heuristic '{descriptor.Id}' returned an inconsistent objective value.");
        }

        string solverVersion =
            ULSAlgorithmsInfo.InformationalVersion;

        if (string.IsNullOrWhiteSpace(
                solverVersion))
        {
            solverVersion =
                ULSAlgorithmsInfo.Version.ToString();
        }

        return new UlsAlgorithmsHeuristicBridgeResult(
            descriptor.Id,
            solveResult.SolverName,
            solverVersion,
            solveResult.Status,
            objectiveValue,
            solution.SetupCost,
            solution.ProductionCost,
            solution.HoldingCost,
            solution.ProductionQuantities.ToArray(),
            solution.EndingInventories.ToArray(),
            solution.SetupDecisions.ToArray(),
            stopwatch.Elapsed);
    }
}
