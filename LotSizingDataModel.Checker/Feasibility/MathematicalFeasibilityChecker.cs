using LotSizingDataModel.Checker.Common;
using LotSizingDataModel.Checker.Configuration;
using LotSizingDataModel.Checker.Contracts;
using LotSizingDataModel.Checker.Projection;
using LotSizingDataModel.Checker.Results;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Checker.Feasibility;

/// <summary>
/// Checks the mathematical feasibility of a normalized
/// <see cref="LotSizingSolution"/> against a
/// <see cref="MathematicalModel"/>.
/// </summary>
/// <remarks>
/// <para>
/// The checker does not duplicate any lot-sizing constraint logic.
/// Candidate business values are first projected to mathematical
/// variables by <see cref="IMathematicalSolutionValueProjector"/>,
/// then the constraints already present in the mathematical model
/// are evaluated directly.
/// </para>
/// <para>
/// This makes the mathematical formulation itself the single source
/// of truth for feasibility.
/// </para>
/// </remarks>
public sealed class MathematicalFeasibilityChecker :
    IMathematicalFeasibilityChecker
{
    private readonly IMathematicalSolutionValueProjector _projector;

    /// <summary>
    /// Initializes the checker with the default mathematical
    /// solution projector.
    /// </summary>
    public MathematicalFeasibilityChecker()
        : this(
            new MathematicalSolutionValueProjector())
    {
    }

    /// <summary>
    /// Initializes the checker with an explicit projector.
    /// </summary>
    /// <param name="projector">
    /// Component used to map business decisions to mathematical variables.
    /// </param>
    public MathematicalFeasibilityChecker(
        IMathematicalSolutionValueProjector projector)
    {
        _projector =
            projector ??
            throw new ArgumentNullException(
                nameof(projector));
    }

    /// <inheritdoc/>
    public SolutionCheckResult Check(
        MathematicalModel model,
        LotSizingSolution solution,
        SolutionCheckOptions options)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(solution);
        ArgumentNullException.ThrowIfNull(options);

        model.EnsureValid();
        options.EnsureValid();

        var result =
            new SolutionCheckResult
            {
                Level =
                    SolutionCheckLevel.Feasibility,
                IsStructurallyValid =
                    true,
                AreVariableDomainsValid =
                    true,
                IsFeasible =
                    true,
                IsObjectiveConsistent =
                    false
            };

        MathematicalSolutionProjectionResult projection =
            _projector.Project(
                model,
                solution);

        AddProjectionIssues(
            projection,
            result);

        if (!projection.IsSuccessful)
        {
            result.IsFeasible =
                false;

            return result;
        }

        foreach (
            LinearConstraint constraint
            in model.Constraints)
        {
            if (options.IgnoreDisabledConstraints &&
                !constraint.IsEnabled)
            {
                continue;
            }

            MathematicalConstraintCheckDetail detail =
                EvaluateConstraint(
                    constraint,
                    projection,
                    options.FeasibilityTolerance);

            if (detail.IsSatisfied)
            {
                continue;
            }

            result.ViolatedConstraintCount++;

            result.TotalConstraintViolation +=
                detail.Violation;

            result.MaximumConstraintViolation =
                Math.Max(
                    result.MaximumConstraintViolation,
                    detail.Violation);

            result.AddIssue(
                CreateConstraintViolationIssue(
                    detail));
        }

        result.IsFeasible =
            result.ViolatedConstraintCount == 0 &&
            !result.Issues.Any(
                issue =>
                    issue.Severity ==
                        SolutionCheckSeverity.Error &&
                    issue.Kind ==
                        SolutionCheckIssueKind.CheckFailure);

        return result;
    }

    private static void AddProjectionIssues(
        MathematicalSolutionProjectionResult projection,
        SolutionCheckResult result)
    {
        foreach (
            MathematicalSolutionProjectionIssue issue
            in projection.Issues)
        {
            result.AddIssue(
                new SolutionCheckIssue
                {
                    Severity =
                        SolutionCheckSeverity.Error,

                    Kind =
                        SolutionCheckIssueKind.MissingVariableValue,

                    DomainKey =
                        issue.DomainKey,

                    Message =
                        $"Variable projection failed" +
                        $"{FormatVariable(issue)}: " +
                        issue.Message
                });
        }
    }

    private static MathematicalConstraintCheckDetail EvaluateConstraint(
        LinearConstraint constraint,
        MathematicalSolutionProjectionResult projection,
        double tolerance)
    {
        double leftHandSide =
            constraint.LeftHandSide.Constant;

        foreach (
            LinearTerm term
            in constraint.LeftHandSide.Terms)
        {
            if (!projection.TryGetValue(
                    term.VariableId,
                    out double variableValue))
            {
                throw new InvalidOperationException(
                    $"No projected value exists for mathematical " +
                    $"variable '{term.VariableId}' while evaluating " +
                    $"constraint '{constraint.Name}'.");
            }

            leftHandSide +=
                term.Coefficient *
                variableValue;
        }

        if (!double.IsFinite(leftHandSide))
        {
            throw new InvalidOperationException(
                $"Constraint '{constraint.Name}' evaluated to a " +
                "non-finite left-hand-side value.");
        }

        double violation =
            ComputeViolation(
                leftHandSide,
                constraint.Sense,
                constraint.RightHandSide);

        return new MathematicalConstraintCheckDetail
        {
            ConstraintId =
                constraint.Id,

            ConstraintName =
                constraint.Name,

            DomainKey =
                string.IsNullOrWhiteSpace(
                    constraint.DomainKey)
                    ? null
                    : constraint.DomainKey,

            LeftHandSideValue =
                leftHandSide,

            Sense =
                constraint.Sense,

            RightHandSideValue =
                constraint.RightHandSide,

            Violation =
                violation,

            IsSatisfied =
                violation <= tolerance
        };
    }

    private static double ComputeViolation(
        double leftHandSide,
        MathematicalConstraintSense sense,
        double rightHandSide)
    {
        if (!double.IsFinite(rightHandSide))
        {
            throw new InvalidOperationException(
                "A mathematical constraint right-hand side must be finite.");
        }

        return sense switch
        {
            MathematicalConstraintSense.LessThanOrEqual =>
                Math.Max(
                    0.0,
                    leftHandSide - rightHandSide),

            MathematicalConstraintSense.Equal =>
                Math.Abs(
                    leftHandSide - rightHandSide),

            MathematicalConstraintSense.GreaterThanOrEqual =>
                Math.Max(
                    0.0,
                    rightHandSide - leftHandSide),

            _ =>
                throw new NotSupportedException(
                    $"Mathematical constraint sense '{sense}' " +
                    "is not supported by the feasibility checker.")
        };
    }

    private static SolutionCheckIssue CreateConstraintViolationIssue(
        MathematicalConstraintCheckDetail detail)
    {
        return new SolutionCheckIssue
        {
            Severity =
                SolutionCheckSeverity.Error,

            Kind =
                SolutionCheckIssueKind.ConstraintViolation,

            DomainKey =
                detail.DomainKey,

            ConstraintName =
                detail.ConstraintName,

            ActualValue =
                detail.LeftHandSideValue,

            ExpectedValue =
                detail.RightHandSideValue,

            Violation =
                detail.Violation,

            Message =
                $"Constraint '{detail.ConstraintName}' is violated. " +
                $"LHS={detail.LeftHandSideValue:G17}; " +
                $"sense={FormatSense(detail.Sense)}; " +
                $"RHS={detail.RightHandSideValue:G17}; " +
                $"violation={detail.Violation:G17}."
        };
    }

    private static string FormatVariable(
        MathematicalSolutionProjectionIssue issue)
    {
        if (issue.VariableId.HasValue &&
            !string.IsNullOrWhiteSpace(
                issue.VariableName))
        {
            return
                $" for variable '{issue.VariableName}' " +
                $"(id={issue.VariableId.Value})";
        }

        if (issue.VariableId.HasValue)
        {
            return
                $" for variable id={issue.VariableId.Value}";
        }

        return string.Empty;
    }

    private static string FormatSense(
        MathematicalConstraintSense sense)
    {
        return sense switch
        {
            MathematicalConstraintSense.LessThanOrEqual =>
                "<=",

            MathematicalConstraintSense.Equal =>
                "=",

            MathematicalConstraintSense.GreaterThanOrEqual =>
                ">=",

            _ =>
                sense.ToString()
        };
    }
}
