using LotSizingDataModel.Checker.Common;
using LotSizingDataModel.Checker.Configuration;
using LotSizingDataModel.Checker.Contracts;
using LotSizingDataModel.Checker.Projection;
using LotSizingDataModel.Checker.Results;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Checker.Objective;

/// <summary>
/// Independently evaluates the objective of a normalized
/// <see cref="LotSizingSolution"/> against a
/// <see cref="MathematicalModel"/>.
/// </summary>
/// <remarks>
/// <para>
/// The checker does not duplicate any lot-sizing cost formula.
/// Candidate business values are projected onto mathematical variables
/// by <see cref="IMathematicalSolutionValueProjector"/>, then the
/// objective expression already contained in the mathematical model is
/// evaluated directly.
/// </para>
/// <para>
/// The recomputed value is compared first with
/// <see cref="SolutionCheckOptions.ReportedObjectiveValueOverride"/> when
/// supplied, otherwise with <c>solution.Evaluation.ObjectiveValue</c>. A
/// missing reported value is not an inconsistency: the independently
/// recomputed value remains available in the returned result.
/// </para>
/// </remarks>
public sealed class MathematicalObjectiveChecker :
    IMathematicalObjectiveChecker
{
    private readonly IMathematicalSolutionValueProjector _projector;

    /// <summary>
    /// Initializes the checker with the default mathematical
    /// solution projector.
    /// </summary>
    public MathematicalObjectiveChecker()
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
    public MathematicalObjectiveChecker(
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
                    SolutionCheckLevel.Full,
                IsStructurallyValid =
                    true,
                AreVariableDomainsValid =
                    true,
                IsFeasible =
                    true,
                IsObjectiveConsistent =
                    false
            };

        try
        {
            MathematicalSolutionProjectionResult projection =
                _projector.Project(
                    model,
                    solution);

            AddProjectionIssues(
                projection,
                result);

            if (!projection.IsSuccessful)
            {
                return result;
            }

            double recomputedObjective =
                EvaluateObjective(
                    model,
                    projection);

            result.RecomputedObjectiveValue =
                recomputedObjective;

            double? reportedObjective =
                options.ReportedObjectiveValueOverride ??
                solution.Evaluation?.ObjectiveValue;

            result.ReportedObjectiveValue =
                reportedObjective;

            if (!reportedObjective.HasValue)
            {
                result.IsObjectiveConsistent =
                    true;

                return result;
            }

            if (!double.IsFinite(
                    reportedObjective.Value))
            {
                result.AddIssue(
                    new SolutionCheckIssue
                    {
                        Severity =
                            SolutionCheckSeverity.Error,
                        Kind =
                            SolutionCheckIssueKind.ObjectiveMismatch,
                        Message =
                            "The objective value reported by the " +
                            "candidate solution is not finite."
                    });

                return result;
            }

            CompareObjectiveValues(
                reportedObjective.Value,
                recomputedObjective,
                options,
                result);

            return result;
        }
        catch (Exception exception)
        {
            result.IsObjectiveConsistent =
                false;

            result.AddIssue(
                new SolutionCheckIssue
                {
                    Severity =
                        SolutionCheckSeverity.Error,
                    Kind =
                        SolutionCheckIssueKind.CheckFailure,
                    Message =
                        "Objective-value checking failed: " +
                        exception.Message
                });

            return result;
        }
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

    private static double EvaluateObjective(
        MathematicalModel model,
        MathematicalSolutionProjectionResult projection)
    {
        double objectiveValue =
            model.Objective.Expression.Constant;

        foreach (
            LinearTerm term
            in model.Objective.Expression.Terms)
        {
            if (!projection.TryGetValue(
                    term.VariableId,
                    out double variableValue))
            {
                throw new InvalidOperationException(
                    $"No projected value exists for mathematical " +
                    $"variable '{term.VariableId}' while evaluating " +
                    "the objective.");
            }

            objectiveValue +=
                term.Coefficient *
                variableValue;
        }

        if (!double.IsFinite(
                objectiveValue))
        {
            throw new InvalidOperationException(
                "The recomputed objective value is not finite.");
        }

        return objectiveValue;
    }

    private static void CompareObjectiveValues(
        double reportedObjective,
        double recomputedObjective,
        SolutionCheckOptions options,
        SolutionCheckResult result)
    {
        double absoluteDifference =
            Math.Abs(
                reportedObjective -
                recomputedObjective);

        double scale =
            Math.Max(
                1.0,
                Math.Max(
                    Math.Abs(
                        reportedObjective),
                    Math.Abs(
                        recomputedObjective)));

        double relativeDifference =
            absoluteDifference /
            scale;

        double comparisonTolerance =
            Math.Max(
                options.ObjectiveAbsoluteTolerance,
                options.ObjectiveRelativeTolerance *
                scale);

        result.ObjectiveDifference =
            absoluteDifference;

        result.ObjectiveRelativeDifference =
            relativeDifference;

        result.ObjectiveComparisonTolerance =
            comparisonTolerance;

        result.IsObjectiveConsistent =
            absoluteDifference <=
            comparisonTolerance;

        if (result.IsObjectiveConsistent)
        {
            return;
        }

        result.AddIssue(
            new SolutionCheckIssue
            {
                Severity =
                    SolutionCheckSeverity.Error,
                Kind =
                    SolutionCheckIssueKind.ObjectiveMismatch,
                ActualValue =
                    recomputedObjective,
                ExpectedValue =
                    reportedObjective,
                Violation =
                    absoluteDifference,
                Message =
                    "Objective values are inconsistent. " +
                    $"Reported={reportedObjective:G17}; " +
                    $"recomputed={recomputedObjective:G17}; " +
                    $"absolute difference={absoluteDifference:G17}; " +
                    $"relative difference={relativeDifference:G17}; " +
                    $"comparison tolerance={comparisonTolerance:G17}."
            });
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
}
