namespace LotSizingDataModel.Solver.Algorithms.Uls;

/// <summary>
/// Immutable external exact-solve evidence retained before
/// projection back into LotSizingDataModel.
/// </summary>
public sealed class UlsAlgorithmsExactBridgeResult
{
    public UlsAlgorithmsExactBridgeResult(
        UlsAlgorithmsExactMethod method,
        string solverId,
        string solverName,
        string solverVersion,
        double objectiveValue,
        IReadOnlyList<double> productionQuantities,
        IReadOnlyList<double> endingInventories,
        IReadOnlyList<bool> setupDecisions,
        TimeSpan solveDuration)
    {
        Method = method;
        SolverId = solverId;
        SolverName = solverName;
        SolverVersion = solverVersion;
        ObjectiveValue = objectiveValue;
        ProductionQuantities = productionQuantities.ToArray();
        EndingInventories = endingInventories.ToArray();
        SetupDecisions = setupDecisions.ToArray();
        SolveDuration = solveDuration;

        EnsureValid();
    }

    public UlsAlgorithmsExactMethod Method
    {
        get;
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

    public double ObjectiveValue
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
        if (string.IsNullOrWhiteSpace(SolverId) ||
            string.IsNullOrWhiteSpace(SolverName) ||
            string.IsNullOrWhiteSpace(SolverVersion))
        {
            throw new InvalidOperationException(
                "External exact-solver provenance must be complete.");
        }

        if (double.IsNaN(ObjectiveValue) ||
            double.IsInfinity(ObjectiveValue))
        {
            throw new InvalidOperationException(
                "The external exact objective value must be finite.");
        }

        if (Horizon <= 0 ||
            EndingInventories.Length != Horizon ||
            SetupDecisions.Length != Horizon)
        {
            throw new InvalidOperationException(
                "The external exact solution vectors must be complete and use one common horizon.");
        }

        if (SolveDuration < TimeSpan.Zero)
        {
            throw new InvalidOperationException(
                "External exact solve duration cannot be negative.");
        }
    }
}
