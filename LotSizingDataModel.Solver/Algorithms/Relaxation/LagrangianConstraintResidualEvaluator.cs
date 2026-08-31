using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Algorithms.Relaxation;

/// <summary>
/// Evaluates the canonical Lagrangian residual
/// left-hand side minus right-hand side.
/// </summary>
public static class LagrangianConstraintResidualEvaluator
{
    public static double Evaluate(
        LinearConstraint constraint,
        IReadOnlyDictionary<int, double> variableValues)
    {
        ArgumentNullException.ThrowIfNull(
            constraint);

        ArgumentNullException.ThrowIfNull(
            variableValues);

        double value =
            constraint.LeftHandSide.Constant -
            constraint.RightHandSide;

        foreach (LinearTerm term
                 in constraint.LeftHandSide.Terms)
        {
            if (!variableValues.TryGetValue(
                    term.VariableId,
                    out double variableValue))
            {
                throw new KeyNotFoundException(
                    $"No value is available for variable identifier '{term.VariableId}'.");
            }

            if (double.IsNaN(variableValue) ||
                double.IsInfinity(variableValue))
            {
                throw new InvalidOperationException(
                    $"Variable identifier '{term.VariableId}' has a non-finite value.");
            }

            value +=
                term.Coefficient *
                variableValue;
        }

        if (double.IsNaN(value) ||
            double.IsInfinity(value))
        {
            throw new InvalidOperationException(
                "The evaluated Lagrangian residual is not finite.");
        }

        return value;
    }
}
