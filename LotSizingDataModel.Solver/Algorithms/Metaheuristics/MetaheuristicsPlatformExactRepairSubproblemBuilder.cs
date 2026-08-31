using LotSizingDataModel.Solver.Modeling;
using MetaheuristicsPlatform.Matheuristics;

namespace LotSizingDataModel.Solver.Algorithms.Metaheuristics;

public sealed class MetaheuristicsPlatformExactRepairSubproblemBuilder
{
    public MathematicalModel BuildExact(
        MathematicalModel sourceModel,
        ExactRepairRequest request)
    {
        ArgumentNullException.ThrowIfNull(
            sourceModel);

        ArgumentNullException.ThrowIfNull(
            request);

        sourceModel.EnsureValid();

        EnsureSupportedMode(
            request);

        MathematicalModel result =
            sourceModel.Clone();

        MathematicalVariable[] variables =
            result.Variables
                .OrderBy(
                    variable =>
                        variable.Id)
                .ToArray();

        ApplyBounds(
            variables,
            request);

        ApplyAllowedActiveIndices(
            variables,
            request);

        int nextConstraintId =
            result.Constraints.Count == 0
                ? 1
                : result.Constraints.Max(
                      constraint =>
                          constraint.Id) + 1;

        if (request.HammingRadius.HasValue)
        {
            nextConstraintId =
                AddHammingConstraint(
                    result,
                    variables,
                    request,
                    nextConstraintId);
        }

        if (request.ObjectiveCutoff.HasValue)
        {
            AddObjectiveCutoff(
                result,
                request.ObjectiveCutoff.Value,
                nextConstraintId);
        }

        result.EnsureValid();

        return result;
    }

    public MathematicalModel BuildLocalBranchingSubproblem(
        MathematicalModel sourceModel,
        IReadOnlyList<double> referenceValues,
        int hammingRadius,
        int nodeLimit)
    {
        ArgumentNullException.ThrowIfNull(
            referenceValues);

        return BuildExact(
            sourceModel,
            new ExactRepairRequest
            {
                Mode =
                    MatheuristicSolveMode.OriginalObjective,

                ReferenceValues =
                    referenceValues,

                HammingRadius =
                    hammingRadius,

                NodeLimit =
                    nodeLimit
            });
    }

    public MathematicalModel BuildRelaxation(
        MathematicalModel sourceModel,
        ExactRepairRequest request)
    {
        MathematicalModel result =
            BuildExact(
                sourceModel,
                request);

        foreach (MathematicalVariable variable
                 in result.Variables)
        {
            if (variable.VariableType is
                    MathematicalVariableType.Integer or
                    MathematicalVariableType.Binary)
            {
                variable.VariableType =
                    MathematicalVariableType.Continuous;
            }
        }

        result.EnsureValid();

        return result;
    }

    private static void EnsureSupportedMode(
        ExactRepairRequest request)
    {
        if (request.NodeLimit <= 0)
        {
            throw new InvalidOperationException(
                "An exact-repair node limit must be strictly positive.");
        }

        if (request.Mode !=
            MatheuristicSolveMode.OriginalObjective)
        {
            throw new NotSupportedException(
                $"Alpha.37 exact-repair bridge supports OriginalObjective mode only; received '{request.Mode}'.");
        }

        if (request.DistanceLimit.HasValue ||
            request.TargetValues is not null)
        {
            throw new NotSupportedException(
                "Distance-target exact-repair modes are deferred beyond alpha.37.");
        }
    }

    private static void ApplyBounds(
        IReadOnlyList<MathematicalVariable> variables,
        ExactRepairRequest request)
    {
        foreach (KeyValuePair<int, MatheuristicVariableBound> bound
                 in request.Bounds)
        {
            MathematicalVariable variable =
                GetVariable(
                    variables,
                    bound.Key);

            EnsureBoundInsideSource(
                variable,
                bound.Value.Lower,
                bound.Value.Upper);

            variable.LowerBound =
                bound.Value.Lower;

            variable.UpperBound =
                bound.Value.Upper;
        }

        foreach (KeyValuePair<int, double> fixing
                 in request.FixedValues)
        {
            MathematicalVariable variable =
                GetVariable(
                    variables,
                    fixing.Key);

            EnsureBoundInsideSource(
                variable,
                fixing.Value,
                fixing.Value);

            EnsureValueMatchesType(
                variable,
                fixing.Value);

            variable.LowerBound =
                fixing.Value;

            variable.UpperBound =
                fixing.Value;
        }
    }

    private static void ApplyAllowedActiveIndices(
        IReadOnlyList<MathematicalVariable> variables,
        ExactRepairRequest request)
    {
        if (request.AllowedActiveIndices is null)
        {
            return;
        }

        var allowed =
            new HashSet<int>(
                request.AllowedActiveIndices);

        for (int index = 0;
             index < variables.Count;
             index++)
        {
            MathematicalVariable variable =
                variables[index];

            if (variable.VariableType !=
                    MathematicalVariableType.Binary ||
                allowed.Contains(
                    index))
            {
                continue;
            }

            EnsureBoundInsideSource(
                variable,
                0.0,
                0.0);

            variable.LowerBound =
                0.0;

            variable.UpperBound =
                0.0;
        }
    }

    private static int AddHammingConstraint(
        MathematicalModel model,
        IReadOnlyList<MathematicalVariable> variables,
        ExactRepairRequest request,
        int constraintId)
    {
        if (request.ReferenceValues is null)
        {
            throw new InvalidOperationException(
                "A Hamming-radius request requires reference values.");
        }

        if (request.ReferenceValues.Count !=
            variables.Count)
        {
            throw new InvalidOperationException(
                "Hamming reference dimension does not match the mathematical model.");
        }

        int radius =
            request.HammingRadius ??
            throw new InvalidOperationException(
                "A Hamming radius is required.");

        if (radius < 0)
        {
            throw new InvalidOperationException(
                "A Hamming radius cannot be negative.");
        }

        var expression =
            new LinearExpression();

        int referenceOneCount =
            0;

        for (int index = 0;
             index < variables.Count;
             index++)
        {
            MathematicalVariable variable =
                variables[index];

            if (variable.VariableType !=
                MathematicalVariableType.Binary)
            {
                continue;
            }

            double reference =
                request.ReferenceValues[index];

            bool isZero =
                Math.Abs(reference) <=
                1.0e-7;

            bool isOne =
                Math.Abs(reference - 1.0) <=
                1.0e-7;

            if (!isZero &&
                !isOne)
            {
                throw new InvalidOperationException(
                    $"Binary Hamming reference at index {index} is not zero or one.");
            }

            if (isOne)
            {
                referenceOneCount++;

                expression.AddTerm(
                    variable.Id,
                    -1.0);
            }
            else
            {
                expression.AddTerm(
                    variable.Id,
                    1.0);
            }
        }

        model.AddConstraint(
            new LinearConstraint(
                constraintId,
                "metaheuristicLocalBranchingHamming",
                expression,
                MathematicalConstraintSense.LessThanOrEqual,
                radius -
                    referenceOneCount));

        return constraintId + 1;
    }

    private static void AddObjectiveCutoff(
        MathematicalModel model,
        double cutoff,
        int constraintId)
    {
        if (!double.IsFinite(
                cutoff))
        {
            throw new InvalidOperationException(
                "A matheuristic objective cutoff must be finite.");
        }

        LinearExpression expression =
            model.Objective.Expression.Clone();

        double constant =
            expression.Constant;

        expression.AddConstant(
            -constant);

        MathematicalConstraintSense sense =
            model.Objective.Sense switch
            {
                ObjectiveSense.Minimize =>
                    MathematicalConstraintSense.LessThanOrEqual,

                ObjectiveSense.Maximize =>
                    MathematicalConstraintSense.GreaterThanOrEqual,

                _ =>
                    throw new NotSupportedException(
                        $"Objective sense '{model.Objective.Sense}' cannot define a matheuristic objective cutoff.")
            };

        model.AddConstraint(
            new LinearConstraint(
                constraintId,
                "metaheuristicObjectiveCutoff",
                expression,
                sense,
                cutoff -
                    constant));
    }

    private static MathematicalVariable GetVariable(
        IReadOnlyList<MathematicalVariable> variables,
        int index)
    {
        if (index < 0 ||
            index >=
                variables.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(index));
        }

        return variables[index];
    }

    private static void EnsureBoundInsideSource(
        MathematicalVariable variable,
        double lower,
        double upper)
    {
        if (!double.IsFinite(lower) ||
            !double.IsFinite(upper) ||
            lower >
                upper)
        {
            throw new InvalidOperationException(
                $"Invalid exact-repair bounds for variable '{variable.Name}'.");
        }

        if (lower <
                variable.LowerBound ||
            upper >
                variable.UpperBound)
        {
            throw new InvalidOperationException(
                $"Exact-repair bounds for variable '{variable.Name}' exceed its source domain.");
        }
    }

    private static void EnsureValueMatchesType(
        MathematicalVariable variable,
        double value)
    {
        switch (variable.VariableType)
        {
            case MathematicalVariableType.Continuous:
                return;

            case MathematicalVariableType.Integer:
                if (value !=
                    Math.Truncate(
                        value))
                {
                    throw new InvalidOperationException(
                        $"Integer variable '{variable.Name}' cannot be fixed to {value:G17}.");
                }

                return;

            case MathematicalVariableType.Binary:
                if (value != 0.0 &&
                    value != 1.0)
                {
                    throw new InvalidOperationException(
                        $"Binary variable '{variable.Name}' can only be fixed to zero or one.");
                }

                return;

            default:
                throw new NotSupportedException(
                    $"Variable type '{variable.VariableType}' is outside the alpha.37 exact-repair bridge.");
        }
    }
}
