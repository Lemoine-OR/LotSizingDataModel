namespace LotSizingDataModel.Solver.Algorithms.Relaxation;

/// <summary>
/// Associates one relaxed mathematical constraint with its
/// Lagrangian multiplier.
/// </summary>
public sealed class LagrangianMultiplier
{
    public LagrangianMultiplier(
        int constraintId,
        double value)
    {
        ConstraintId =
            constraintId;

        Value =
            value;

        EnsureValid();
    }

    public int ConstraintId
    {
        get;
    }

    public double Value
    {
        get;
    }

    public void EnsureValid()
    {
        if (ConstraintId <= 0)
        {
            throw new InvalidOperationException(
                "A Lagrangian multiplier must reference a strictly positive constraint identifier.");
        }

        if (double.IsNaN(Value) ||
            double.IsInfinity(Value))
        {
            throw new InvalidOperationException(
                "A Lagrangian multiplier must be finite.");
        }
    }
}
