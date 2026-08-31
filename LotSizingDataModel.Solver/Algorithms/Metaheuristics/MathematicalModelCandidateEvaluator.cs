using LotSizingDataModel.Solver.Execution;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Algorithms.Metaheuristics;

public sealed class MathematicalModelCandidateEvaluator
{
    private readonly MathematicalModel _model;
    private readonly MathematicalVariable[] _variables;
    private readonly Dictionary<int, int> _indexByVariableId;

    public MathematicalModelCandidateEvaluator(
        MathematicalModel model)
    {
        ArgumentNullException.ThrowIfNull(
            model);

        model.EnsureValid();

        _model =
            model;

        _variables =
            model.Variables
                .OrderBy(
                    variable =>
                        variable.Id)
                .ToArray();

        if (_variables.Length == 0)
        {
            throw new InvalidOperationException(
                "A metaheuristic mathematical model requires at least one variable.");
        }

        _indexByVariableId =
            _variables
                .Select(
                    (variable, index) =>
                        new
                        {
                            variable.Id,
                            Index = index
                        })
                .ToDictionary(
                    entry =>
                        entry.Id,
                    entry =>
                        entry.Index);
    }

    public IReadOnlyList<MathematicalVariable> Variables =>
        _variables;

    public int Dimension =>
        _variables.Length;

    public double EvaluateObjective(
        IReadOnlyList<double> values)
    {
        return EvaluateExpression(
            _model.Objective.Expression,
            values);
    }

    public double EvaluateExpression(
        LinearExpression expression,
        IReadOnlyList<double> values)
    {
        ArgumentNullException.ThrowIfNull(
            expression);

        EnsureDimension(
            values);

        double result =
            expression.Constant;

        foreach (LinearTerm term
                 in expression.Terms)
        {
            if (!_indexByVariableId.TryGetValue(
                    term.VariableId,
                    out int index))
            {
                throw new InvalidOperationException(
                    $"Expression references unknown variable identifier '{term.VariableId}'.");
            }

            result +=
                term.Coefficient *
                values[index];
        }

        if (!double.IsFinite(
                result))
        {
            throw new InvalidOperationException(
                "Candidate expression evaluation must be finite.");
        }

        return result;
    }

    public double EvaluateCanonicalConstraintResidual(
        LinearConstraint constraint,
        IReadOnlyList<double> values)
    {
        ArgumentNullException.ThrowIfNull(
            constraint);

        double leftHandSide =
            EvaluateExpression(
                constraint.LeftHandSide,
                values);

        return constraint.Sense switch
        {
            MathematicalConstraintSense.LessThanOrEqual =>
                leftHandSide -
                constraint.RightHandSide,

            MathematicalConstraintSense.GreaterThanOrEqual =>
                constraint.RightHandSide -
                leftHandSide,

            MathematicalConstraintSense.Equal =>
                leftHandSide -
                constraint.RightHandSide,

            _ =>
                throw new NotSupportedException(
                    $"Constraint sense '{constraint.Sense}' is not supported by the metaheuristic bridge.")
        };
    }

    public bool IsIntegerFeasible(
        IReadOnlyList<double> values,
        double tolerance = 1.0e-7)
    {
        EnsureDimension(
            values);

        EnsureTolerance(
            tolerance);

        for (int index = 0;
             index < _variables.Length;
             index++)
        {
            MathematicalVariable variable =
                _variables[index];

            double value =
                values[index];

            if (!double.IsFinite(
                    value))
            {
                return false;
            }

            if (value <
                    variable.LowerBound -
                    tolerance ||
                value >
                    variable.UpperBound +
                    tolerance)
            {
                return false;
            }

            switch (variable.VariableType)
            {
                case MathematicalVariableType.Continuous:
                    break;

                case MathematicalVariableType.Integer:
                    if (Math.Abs(
                            value -
                            Math.Round(
                                value,
                                MidpointRounding.AwayFromZero)) >
                        tolerance)
                    {
                        return false;
                    }

                    break;

                case MathematicalVariableType.Binary:
                    if (Math.Abs(value) >
                            tolerance &&
                        Math.Abs(value - 1.0) >
                            tolerance)
                    {
                        return false;
                    }

                    break;

                case MathematicalVariableType.SemiContinuous:
                case MathematicalVariableType.SemiInteger:
                    throw new NotSupportedException(
                        "Semi-continuous and semi-integer domains are not enabled in alpha.37.");

                default:
                    throw new NotSupportedException(
                        $"Variable type '{variable.VariableType}' is not supported by alpha.37.");
            }
        }

        return true;
    }

    public bool IsConstraintFeasible(
        IReadOnlyList<double> values,
        double equalityTolerance = 1.0e-6)
    {
        EnsureDimension(
            values);

        EnsureTolerance(
            equalityTolerance);

        foreach (LinearConstraint constraint
                 in _model.Constraints)
        {
            if (!constraint.IsEnabled)
            {
                continue;
            }

            double residual =
                EvaluateCanonicalConstraintResidual(
                    constraint,
                    values);

            if (constraint.Sense ==
                MathematicalConstraintSense.Equal)
            {
                if (Math.Abs(
                        residual) >
                    equalityTolerance)
                {
                    return false;
                }
            }
            else if (residual >
                     equalityTolerance)
            {
                return false;
            }
        }

        return true;
    }

    public bool IsFeasible(
        IReadOnlyList<double> values,
        double equalityTolerance = 1.0e-6)
    {
        return IsIntegerFeasible(
                   values,
                   equalityTolerance) &&
               IsConstraintFeasible(
                   values,
                   equalityTolerance);
    }

    public double[] ExtractCompleteValues(
        MathematicalModelSolveResult solveResult)
    {
        ArgumentNullException.ThrowIfNull(
            solveResult);

        var values =
            new double[_variables.Length];

        for (int index = 0;
             index < _variables.Length;
             index++)
        {
            MathematicalVariable variable =
                _variables[index];

            MathematicalVariableValue? value =
                solveResult.FindVariableValue(
                    variable.Id);

            if (value is null)
            {
                throw new InvalidOperationException(
                    $"Solver result does not contain variable identifier '{variable.Id}'.");
            }

            values[index] =
                value.Value;
        }

        return values;
    }

    private void EnsureDimension(
        IReadOnlyList<double> values)
    {
        ArgumentNullException.ThrowIfNull(
            values);

        if (values.Count !=
            Dimension)
        {
            throw new ArgumentException(
                $"Expected candidate dimension {Dimension}, received {values.Count}.",
                nameof(values));
        }
    }

    private static void EnsureTolerance(
        double tolerance)
    {
        if (!double.IsFinite(
                tolerance) ||
            tolerance < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(tolerance));
        }
    }
}
