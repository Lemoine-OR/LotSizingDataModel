using System.Diagnostics;
using ULSAlgorithms;
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Catalog;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;
using ULSAlgorithms.Validation;

namespace LotSizingDataModel.Solver.Algorithms.Uls;

/// <summary>
/// Executes one explicitly selected exact ULSAlgorithms method.
/// No method fallback is permitted.
/// </summary>
public sealed class UlsAlgorithmsExactBridge
{
    public UlsAlgorithmsExactBridgeResult Solve(
        UlsAlgorithmsExactProblemData problemData,
        UlsAlgorithmsExactMethod method,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            problemData);

        problemData.EnsureValid();

        string solverId =
            UlsAlgorithmsExactMethodCatalog.GetSolverId(
                method);

        UlsSolverDescriptor descriptor =
            UlsAlgorithmsExactMethodCatalog.GetExactDescriptor(
                method);

        IUlsSolver solver =
            UlsSolverFactory.Create(
                solverId);

        if (solver.Kind !=
            UlsSolverKind.Exact)
        {
            throw new InvalidOperationException(
                $"ULSAlgorithms factory returned a non-exact solver for '{solverId}'.");
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

        if (!solveResult.HasSolution ||
            solveResult.Solution is null)
        {
            throw new InvalidOperationException(
                $"Exact ULSAlgorithms solver '{solverId}' returned no solution. Status={solveResult.Status}. Message={solveResult.Message}");
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
                $"ULSAlgorithms validator rejected the exact solution from '{solverId}'.");
        }

        if (solution.Horizon !=
            problemData.Horizon)
        {
            throw new InvalidOperationException(
                $"ULSAlgorithms solver '{solverId}' returned horizon {solution.Horizon}, expected {problemData.Horizon}.");
        }

        double objectiveValue =
            solveResult.ObjectiveValue ??
            solution.TotalCost;

        double costDifference =
            Math.Abs(
                objectiveValue -
                validation.RecomputedTotalCost);

        double scale =
            Math.Max(
                1.0,
                Math.Abs(objectiveValue));

        if (costDifference >
            1.0e-7 * scale)
        {
            throw new InvalidOperationException(
                $"ULSAlgorithms exact objective mismatch for '{solverId}'.");
        }

        string solverVersion =
            ULSAlgorithmsInfo.InformationalVersion;

        if (string.IsNullOrWhiteSpace(
                solverVersion))
        {
            solverVersion =
                ULSAlgorithmsInfo.Version.ToString();
        }

        return new UlsAlgorithmsExactBridgeResult(
            method,
            descriptor.Id,
            solveResult.SolverName,
            solverVersion,
            objectiveValue,
            solution.ProductionQuantities.ToArray(),
            solution.EndingInventories.ToArray(),
            solution.SetupDecisions.ToArray(),
            stopwatch.Elapsed);
    }
}
