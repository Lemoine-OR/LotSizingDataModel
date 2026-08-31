using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Algorithms.Relaxation;

/// <summary>
/// Performs one projected dual-ascent subgradient update for the
/// alpha.36 minimization convention.
/// </summary>
public static class LagrangianSubgradientUpdater
{
    public static double Update(
        double currentMultiplier,
        double residual,
        double stepSize,
        MathematicalConstraintSense sense)
    {
        LagrangianMultiplierDomain.EnsureValid(
            sense,
            currentMultiplier);

        if (double.IsNaN(residual) ||
            double.IsInfinity(residual))
        {
            throw new InvalidOperationException(
                "A Lagrangian subgradient residual must be finite.");
        }

        if (double.IsNaN(stepSize) ||
            double.IsInfinity(stepSize) ||
            stepSize < 0.0)
        {
            throw new InvalidOperationException(
                "A Lagrangian subgradient step size must be finite and non-negative.");
        }

        double candidate =
            currentMultiplier +
            stepSize *
            residual;

        if (double.IsNaN(candidate) ||
            double.IsInfinity(candidate))
        {
            throw new InvalidOperationException(
                "The Lagrangian subgradient update produced a non-finite multiplier.");
        }

        return LagrangianMultiplierDomain.Project(
            sense,
            candidate);
    }
}
