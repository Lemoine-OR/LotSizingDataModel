using LotSizingDataModel.Solution.Common;
using LotSizingDataModel.Solver.Execution;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Checker.Feasibility;

/// <summary>
/// Independently evaluates a mathematical solver result against
/// the solver-independent model.
/// </summary>
public sealed class MathematicalModelSolveResultFeasibilityChecker
{
    public MathematicalFeasibilityCheckResult Check(
        MathematicalModel model,
        MathematicalModelSolveResult solveResult,
        double tolerance = 1.0e-6)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(solveResult);

        if (!double.IsFinite(tolerance) ||
            tolerance < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(tolerance));
        }

        model.EnsureValid();

        var diagnostics =
            new List<MathematicalFeasibilityDiagnostic>();

        if (!solveResult.HasFeasibleSolution)
        {
            diagnostics.Add(
                new MathematicalFeasibilityDiagnostic(
                    "LSDM-FEAS-SOL-000",
                    "Solver result does not claim a feasible incumbent; no candidate can be checked."));

            return new MathematicalFeasibilityCheckResult(
                FeasibilityStatus.NotEvaluated,
                diagnostics);
        }

        var values =
            new Dictionary<int, double>();

        bool complete = true;

        foreach (MathematicalVariable variable
                 in model.Variables)
        {
            MathematicalVariableValue? value =
                solveResult.FindVariableValue(
                    variable.Id);

            if (value is null)
            {
                complete = false;

                diagnostics.Add(
                    new MathematicalFeasibilityDiagnostic(
                        "LSDM-FEAS-SOL-001",
                        $"Missing value for variable '{variable.Name}' ({variable.Id})."));

                continue;
            }

            values[variable.Id] =
                value.Value;

            CheckVariableDomain(
                variable,
                value.Value,
                tolerance,
                diagnostics);
        }

        if (diagnostics.Any(
                diagnostic =>
                    diagnostic.Code is
                        "LSDM-FEAS-SOL-002" or
                        "LSDM-FEAS-SOL-003"))
        {
            return new MathematicalFeasibilityCheckResult(
                FeasibilityStatus.Infeasible,
                diagnostics);
        }

        if (!complete)
        {
            return new MathematicalFeasibilityCheckResult(
                FeasibilityStatus.PartiallyEvaluated,
                diagnostics);
        }

        foreach (LinearConstraint constraint
                 in model.Constraints)
        {
            if (!constraint.IsEnabled)
            {
                continue;
            }

            double leftHandSide =
                constraint.LeftHandSide.Constant;

            foreach (LinearTerm term
                     in constraint.LeftHandSide.Terms)
            {
                leftHandSide +=
                    term.Coefficient *
                    values[term.VariableId];
            }

            bool satisfied =
                constraint.Sense switch
                {
                    MathematicalConstraintSense.LessThanOrEqual =>
                        leftHandSide <=
                            constraint.RightHandSide +
                            tolerance,

                    MathematicalConstraintSense.GreaterThanOrEqual =>
                        leftHandSide >=
                            constraint.RightHandSide -
                            tolerance,

                    MathematicalConstraintSense.Equal =>
                        Math.Abs(
                            leftHandSide -
                            constraint.RightHandSide) <=
                        tolerance,

                    _ =>
                        throw new NotSupportedException(
                            $"Constraint sense '{constraint.Sense}' is not supported by the checker.")
                };

            if (!satisfied)
            {
                diagnostics.Add(
                    new MathematicalFeasibilityDiagnostic(
                        "LSDM-FEAS-SOL-004",
                        $"Constraint '{constraint.Name}' ({constraint.Id}) is violated: lhs={leftHandSide:G17}, rhs={constraint.RightHandSide:G17}, sense={constraint.Sense}."));
            }
        }

        return new MathematicalFeasibilityCheckResult(
            diagnostics.Any(
                diagnostic =>
                    diagnostic.Code ==
                    "LSDM-FEAS-SOL-004")
                ? FeasibilityStatus.Infeasible
                : FeasibilityStatus.Feasible,
            diagnostics);
    }

    private static void CheckVariableDomain(
        MathematicalVariable variable,
        double value,
        double tolerance,
        ICollection<MathematicalFeasibilityDiagnostic> diagnostics)
    {
        if (!double.IsFinite(value) ||
            value <
                variable.LowerBound -
                tolerance ||
            value >
                variable.UpperBound +
                tolerance)
        {
            diagnostics.Add(
                new MathematicalFeasibilityDiagnostic(
                    "LSDM-FEAS-SOL-002",
                    $"Variable '{variable.Name}' ({variable.Id}) violates its bounds."));

            return;
        }

        bool integralViolation =
            variable.VariableType switch
            {
                MathematicalVariableType.Integer or
                MathematicalVariableType.SemiInteger =>
                    Math.Abs(
                        value -
                        Math.Round(
                            value,
                            MidpointRounding.AwayFromZero)) >
                    tolerance,

                MathematicalVariableType.Binary =>
                    Math.Abs(value) > tolerance &&
                    Math.Abs(value - 1.0) > tolerance,

                _ =>
                    false
            };

        if (integralViolation)
        {
            diagnostics.Add(
                new MathematicalFeasibilityDiagnostic(
                    "LSDM-FEAS-SOL-003",
                    $"Variable '{variable.Name}' ({variable.Id}) violates its discrete domain."));
        }
    }
}
