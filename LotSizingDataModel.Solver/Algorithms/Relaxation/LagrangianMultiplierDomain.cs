using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Algorithms.Relaxation;

/// <summary>
/// Encodes the multiplier-domain convention that guarantees a
/// valid lower bound for minimization models when the residual is
/// defined as left-hand side minus right-hand side.
/// </summary>
public static class LagrangianMultiplierDomain
{
    public static void EnsureValid(
        MathematicalConstraintSense sense,
        double multiplier)
    {
        if (double.IsNaN(multiplier) ||
            double.IsInfinity(multiplier))
        {
            throw new InvalidOperationException(
                "A Lagrangian multiplier must be finite.");
        }

        switch (sense)
        {
            case MathematicalConstraintSense.LessThanOrEqual:
                if (multiplier < 0.0)
                {
                    throw new InvalidOperationException(
                        "A relaxed <= constraint requires a non-negative multiplier in a minimization model.");
                }

                break;

            case MathematicalConstraintSense.GreaterThanOrEqual:
                if (multiplier > 0.0)
                {
                    throw new InvalidOperationException(
                        "A relaxed >= constraint requires a non-positive multiplier in a minimization model.");
                }

                break;

            case MathematicalConstraintSense.Equal:
                break;

            default:
                throw new NotSupportedException(
                    $"Constraint sense '{sense}' cannot be Lagrangian-relaxed.");
        }
    }

    public static double Project(
        MathematicalConstraintSense sense,
        double value)
    {
        if (double.IsNaN(value) ||
            double.IsInfinity(value))
        {
            throw new InvalidOperationException(
                "A projected multiplier value must be finite.");
        }

        return sense switch
        {
            MathematicalConstraintSense.LessThanOrEqual =>
                Math.Max(
                    0.0,
                    value),

            MathematicalConstraintSense.GreaterThanOrEqual =>
                Math.Min(
                    0.0,
                    value),

            MathematicalConstraintSense.Equal =>
                value,

            _ =>
                throw new NotSupportedException(
                    $"Constraint sense '{sense}' cannot be Lagrangian-relaxed.")
        };
    }
}
