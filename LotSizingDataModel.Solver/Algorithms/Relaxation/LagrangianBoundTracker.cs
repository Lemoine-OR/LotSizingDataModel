namespace LotSizingDataModel.Solver.Algorithms.Relaxation;

/// <summary>
/// Tracks the best dual lower bound and primal upper bound for a
/// minimization Lagrangian relaxation.
/// </summary>
public sealed class LagrangianBoundTracker
{
    private const double ConsistencyTolerance = 1.0e-7;

    public double? BestDualLowerBound
    {
        get;
        private set;
    }

    public double? BestPrimalUpperBound
    {
        get;
        private set;
    }

    public double? AbsoluteGap =>
        BestDualLowerBound.HasValue &&
        BestPrimalUpperBound.HasValue
            ? Math.Max(
                0.0,
                BestPrimalUpperBound.Value -
                BestDualLowerBound.Value)
            : null;

    public double? RelativeGap
    {
        get
        {
            if (!AbsoluteGap.HasValue ||
                !BestPrimalUpperBound.HasValue)
            {
                return null;
            }

            return AbsoluteGap.Value /
                   Math.Max(
                       1.0,
                       Math.Abs(
                           BestPrimalUpperBound.Value));
        }
    }

    public void RegisterDualLowerBound(
        double value)
    {
        EnsureFinite(
            value,
            nameof(value));

        if (BestPrimalUpperBound.HasValue &&
            value >
                BestPrimalUpperBound.Value +
                ScaledTolerance(
                    BestPrimalUpperBound.Value))
        {
            throw new InvalidOperationException(
                "A claimed Lagrangian dual lower bound exceeds the current primal upper bound.");
        }

        if (!BestDualLowerBound.HasValue ||
            value >
                BestDualLowerBound.Value)
        {
            BestDualLowerBound =
                value;
        }
    }

    public void RegisterPrimalUpperBound(
        double value)
    {
        EnsureFinite(
            value,
            nameof(value));

        if (BestDualLowerBound.HasValue &&
            value <
                BestDualLowerBound.Value -
                ScaledTolerance(
                    BestDualLowerBound.Value))
        {
            throw new InvalidOperationException(
                "A claimed primal upper bound is below the current Lagrangian dual lower bound.");
        }

        if (!BestPrimalUpperBound.HasValue ||
            value <
                BestPrimalUpperBound.Value)
        {
            BestPrimalUpperBound =
                value;
        }
    }

    private static double ScaledTolerance(
        double value)
    {
        return ConsistencyTolerance *
               Math.Max(
                   1.0,
                   Math.Abs(value));
    }

    private static void EnsureFinite(
        double value,
        string valueName)
    {
        if (double.IsNaN(value) ||
            double.IsInfinity(value))
        {
            throw new InvalidOperationException(
                $"{valueName} must be finite.");
        }
    }
}
