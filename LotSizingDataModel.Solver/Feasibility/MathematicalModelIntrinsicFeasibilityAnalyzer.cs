using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Feasibility;

/// <summary>
/// Proves intrinsic infeasibility from variable-bound interval
/// arithmetic. Absence of a proof is reported as Unknown.
/// </summary>
public sealed class MathematicalModelIntrinsicFeasibilityAnalyzer
{
    private const double DefaultTolerance = 1.0e-9;

    public IntrinsicFeasibilityAnalysisResult Analyze(
        MathematicalModel model,
        double tolerance = DefaultTolerance)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (!double.IsFinite(tolerance) ||
            tolerance < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(tolerance));
        }

        model.EnsureValid();

        var variables =
            model.Variables.ToDictionary(
                variable => variable.Id);

        var diagnostics =
            new List<IntrinsicFeasibilityDiagnostic>();

        int evaluated = 0;

        foreach (LinearConstraint constraint
                 in model.Constraints)
        {
            if (!constraint.IsEnabled)
            {
                continue;
            }

            evaluated++;

            LinearExpressionBoundInterval interval =
                EvaluateInterval(
                    constraint.LeftHandSide,
                    variables);

            IntrinsicFeasibilityDiagnostic? diagnostic =
                TryProveConstraintInfeasible(
                    constraint,
                    interval,
                    tolerance);

            if (diagnostic is not null)
            {
                diagnostics.Add(diagnostic);
            }
        }

        return new IntrinsicFeasibilityAnalysisResult(
            diagnostics.Count > 0
                ? IntrinsicFeasibilityStatus.Infeasible
                : IntrinsicFeasibilityStatus.Unknown,
            diagnostics,
            evaluated);
    }

    public LinearExpressionBoundInterval EvaluateInterval(
        LinearExpression expression,
        IReadOnlyDictionary<int, MathematicalVariable> variables)
    {
        ArgumentNullException.ThrowIfNull(expression);
        ArgumentNullException.ThrowIfNull(variables);

        double minimum = expression.Constant;
        double maximum = expression.Constant;

        foreach (LinearTerm term
                 in expression.Terms)
        {
            if (term.Coefficient == 0.0)
            {
                continue;
            }

            if (!variables.TryGetValue(
                    term.VariableId,
                    out MathematicalVariable? variable))
            {
                throw new InvalidOperationException(
                    $"Expression references unknown variable identifier '{term.VariableId}'.");
            }

            double lowContribution;
            double highContribution;

            if (term.Coefficient > 0.0)
            {
                lowContribution =
                    term.Coefficient *
                    variable.LowerBound;

                highContribution =
                    term.Coefficient *
                    variable.UpperBound;
            }
            else
            {
                lowContribution =
                    term.Coefficient *
                    variable.UpperBound;

                highContribution =
                    term.Coefficient *
                    variable.LowerBound;
            }

            minimum += lowContribution;
            maximum += highContribution;

            if (double.IsNaN(minimum) ||
                double.IsNaN(maximum))
            {
                return new LinearExpressionBoundInterval(
                    double.NegativeInfinity,
                    double.PositiveInfinity);
            }
        }

        return new LinearExpressionBoundInterval(
            minimum,
            maximum);
    }

    private static IntrinsicFeasibilityDiagnostic?
        TryProveConstraintInfeasible(
            LinearConstraint constraint,
            LinearExpressionBoundInterval interval,
            double tolerance)
    {
        double rhs =
            constraint.RightHandSide;

        switch (constraint.Sense)
        {
            case MathematicalConstraintSense.LessThanOrEqual:
                if (interval.Minimum >
                    rhs + tolerance)
                {
                    return CreateDiagnostic(
                        "LSDM-FEAS-001",
                        constraint,
                        interval,
                        "Minimum attainable left-hand side exceeds the <= right-hand side.");
                }

                return null;

            case MathematicalConstraintSense.GreaterThanOrEqual:
                if (interval.Maximum <
                    rhs - tolerance)
                {
                    return CreateDiagnostic(
                        "LSDM-FEAS-002",
                        constraint,
                        interval,
                        "Maximum attainable left-hand side is below the >= right-hand side.");
                }

                return null;

            case MathematicalConstraintSense.Equal:
                if (rhs <
                    interval.Minimum - tolerance)
                {
                    return CreateDiagnostic(
                        "LSDM-FEAS-003",
                        constraint,
                        interval,
                        "Equality right-hand side is below the attainable left-hand-side interval.");
                }

                if (rhs >
                    interval.Maximum + tolerance)
                {
                    return CreateDiagnostic(
                        "LSDM-FEAS-004",
                        constraint,
                        interval,
                        "Equality right-hand side is above the attainable left-hand-side interval.");
                }

                return null;

            default:
                throw new NotSupportedException(
                    $"Constraint sense '{constraint.Sense}' is not supported by intrinsic feasibility analysis.");
        }
    }

    private static IntrinsicFeasibilityDiagnostic
        CreateDiagnostic(
            string code,
            LinearConstraint constraint,
            LinearExpressionBoundInterval interval,
            string reason)
    {
        return new IntrinsicFeasibilityDiagnostic(
            code,
            $"Constraint '{constraint.Name}' is intrinsically infeasible: {reason}",
            constraint.Id,
            interval.Minimum,
            interval.Maximum,
            constraint.RightHandSide);
    }
}
