using ULSAlgorithms.Results;

namespace LotSizingDataModel.Solver.Algorithms.Uls;

/// <summary>
/// Validated feasible result returned by one pinned
/// ULSAlgorithms heuristic.
/// </summary>
public sealed class UlsAlgorithmsHeuristicBridgeResult
{
    public UlsAlgorithmsHeuristicBridgeResult(
        string solverId,
        string solverName,
        string solverVersion,
        UlsSolveStatus externalStatus,
        double objectiveValue,
        double setupCost,
        double productionCost,
        double holdingCost,
        IReadOnlyList<double> productionQuantities,
        IReadOnlyList<double> endingInventories,
        IReadOnlyList<bool> setupDecisions,
        TimeSpan solveDuration)
    {
        SolverId =
            solverId?.Trim() ??
            string.Empty;

        SolverName =
            solverName?.Trim() ??
            string.Empty;

        SolverVersion =
            solverVersion?.Trim() ??
            string.Empty;

        ExternalStatus =
            externalStatus;

        ObjectiveValue =
            objectiveValue;

        SetupCost =
            setupCost;

        ProductionCost =
            productionCost;

        HoldingCost =
            holdingCost;

        ProductionQuantities =
            productionQuantities.ToArray();

        EndingInventories =
            endingInventories.ToArray();

        SetupDecisions =
            setupDecisions.ToArray();

        SolveDuration =
            solveDuration;

        EnsureValid();
    }

    public string SolverId
    {
        get;
    }

    public string SolverName
    {
        get;
    }

    public string SolverVersion
    {
        get;
    }

    public UlsSolveStatus ExternalStatus
    {
        get;
    }

    public double ObjectiveValue
    {
        get;
    }

    public double SetupCost
    {
        get;
    }

    public double ProductionCost
    {
        get;
    }

    public double HoldingCost
    {
        get;
    }

    public double[] ProductionQuantities
    {
        get;
    }

    public double[] EndingInventories
    {
        get;
    }

    public bool[] SetupDecisions
    {
        get;
    }

    public TimeSpan SolveDuration
    {
        get;
    }

    public int Horizon =>
        ProductionQuantities.Length;

    public void EnsureValid()
    {
        if (string.IsNullOrWhiteSpace(
                SolverId) ||
            string.IsNullOrWhiteSpace(
                SolverName) ||
            string.IsNullOrWhiteSpace(
                SolverVersion))
        {
            throw new InvalidOperationException(
                "Heuristic solver provenance must be complete.");
        }

        if (ExternalStatus !=
            UlsSolveStatus.Feasible)
        {
            throw new InvalidOperationException(
                $"A heuristic bridge result must be Feasible, not '{ExternalStatus}'.");
        }

        foreach (double value in new[]
                 {
                     ObjectiveValue,
                     SetupCost,
                     ProductionCost,
                     HoldingCost
                 })
        {
            if (double.IsNaN(value) ||
                double.IsInfinity(value))
            {
                throw new InvalidOperationException(
                    "Heuristic objective/cost values must be finite.");
            }
        }

        if (Horizon <= 0 ||
            EndingInventories.Length != Horizon ||
            SetupDecisions.Length != Horizon)
        {
            throw new InvalidOperationException(
                "Heuristic result vectors must be complete and use one common horizon.");
        }

        if (SolveDuration < TimeSpan.Zero)
        {
            throw new InvalidOperationException(
                "Heuristic solve duration cannot be negative.");
        }
    }
}
