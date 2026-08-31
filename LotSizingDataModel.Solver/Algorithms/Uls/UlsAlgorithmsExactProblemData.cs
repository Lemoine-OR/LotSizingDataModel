namespace LotSizingDataModel.Solver.Algorithms.Uls;

/// <summary>
/// Normalized deterministic uncapacitated single-item problem
/// passed to ULSAlgorithms.
/// </summary>
public sealed class UlsAlgorithmsExactProblemData
{
    public UlsAlgorithmsExactProblemData(
        IReadOnlyList<double> demands,
        IReadOnlyList<double> setupCosts,
        IReadOnlyList<double> unitProductionCosts,
        IReadOnlyList<double> holdingCosts)
    {
        ArgumentNullException.ThrowIfNull(demands);
        ArgumentNullException.ThrowIfNull(setupCosts);
        ArgumentNullException.ThrowIfNull(unitProductionCosts);
        ArgumentNullException.ThrowIfNull(holdingCosts);

        Demands = demands.ToArray();
        SetupCosts = setupCosts.ToArray();
        UnitProductionCosts = unitProductionCosts.ToArray();
        HoldingCosts = holdingCosts.ToArray();

        EnsureValid();
    }

    public double[] Demands
    {
        get;
    }

    public double[] SetupCosts
    {
        get;
    }

    public double[] UnitProductionCosts
    {
        get;
    }

    public double[] HoldingCosts
    {
        get;
    }

    public int Horizon =>
        Demands.Length;

    public void EnsureValid()
    {
        if (Horizon <= 0)
        {
            throw new InvalidOperationException(
                "A ULSAlgorithms exact problem requires a positive planning horizon.");
        }

        if (SetupCosts.Length != Horizon ||
            UnitProductionCosts.Length != Horizon ||
            HoldingCosts.Length != Horizon)
        {
            throw new InvalidOperationException(
                "Demand, setup-cost, production-cost and holding-cost vectors must have identical horizons.");
        }

        ValidateVector(
            Demands,
            nameof(Demands));

        ValidateVector(
            SetupCosts,
            nameof(SetupCosts));

        ValidateVector(
            UnitProductionCosts,
            nameof(UnitProductionCosts));

        ValidateVector(
            HoldingCosts,
            nameof(HoldingCosts));
    }

    private static void ValidateVector(
        IReadOnlyList<double> values,
        string name)
    {
        for (int index = 0;
             index < values.Count;
             index++)
        {
            double value =
                values[index];

            if (double.IsNaN(value) ||
                double.IsInfinity(value) ||
                value < 0.0)
            {
                throw new InvalidOperationException(
                    $"{name}[{index}] must be finite and non-negative.");
            }
        }
    }
}
