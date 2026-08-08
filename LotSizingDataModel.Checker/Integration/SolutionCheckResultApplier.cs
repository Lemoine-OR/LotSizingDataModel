using LotSizingDataModel.Checker.Common;
using LotSizingDataModel.Checker.Configuration;
using LotSizingDataModel.Checker.Contracts;
using LotSizingDataModel.Checker.Results;
using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Instance.Results;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solution.Common;

namespace LotSizingDataModel.Checker.Integration;

/// <summary>
/// Applies independent checker results back to the serializable solution
/// evaluation and to an optional known-result record.
/// </summary>
/// <remarks>
/// The applier deliberately does not change optimality information, solver
/// bounds, or solver optimality gaps. The checker establishes feasibility and
/// objective consistency, not optimality.
/// </remarks>
public sealed class SolutionCheckResultApplier :
    ISolutionCheckResultApplier
{
    private const string ObjectiveValueSource =
        "Recomputed by LotSizingSolutionChecker from candidate " +
        "decision-variable values";

    /// <inheritdoc/>
    public SolutionCheckApplicationResult Apply(
        LotSizingSolution solution,
        SolutionCheckResult checkResult,
        SolutionVerificationOptions options,
        DateTime evaluatedAtUtc,
        KnownResult? knownResult = null)
    {
        ArgumentNullException.ThrowIfNull(solution);
        ArgumentNullException.ThrowIfNull(checkResult);
        ArgumentNullException.ThrowIfNull(options);

        options.EnsureValid();

        if (evaluatedAtUtc.Kind == DateTimeKind.Local)
        {
            evaluatedAtUtc = evaluatedAtUtc.ToUniversalTime();
        }
        else if (evaluatedAtUtc.Kind == DateTimeKind.Unspecified)
        {
            evaluatedAtUtc = DateTime.SpecifyKind(
                evaluatedAtUtc,
                DateTimeKind.Utc);
        }

        var applicationResult =
            new SolutionCheckApplicationResult();

        if (options.ApplyToSolutionEvaluation)
        {
            ApplyToSolutionEvaluation(
                solution,
                checkResult,
                options,
                evaluatedAtUtc);

            applicationResult.SolutionEvaluationUpdated =
                true;
        }

        if (knownResult is not null)
        {
            ApplyToKnownResult(
                knownResult,
                checkResult,
                options,
                applicationResult);
        }

        return applicationResult;
    }

    private static void ApplyToSolutionEvaluation(
        LotSizingSolution solution,
        SolutionCheckResult result,
        SolutionVerificationOptions options,
        DateTime evaluatedAtUtc)
    {
        solution.Evaluation.EvaluatedAtUtc =
            evaluatedAtUtc;

        solution.Evaluation.EvaluatorName =
            options.EvaluatorName;

        solution.Evaluation.EvaluatorVersion =
            options.EvaluatorVersion;

        if (CanPublishFeasibility(result))
        {
            bool feasible =
                result.IsStructurallyValid &&
                result.AreVariableDomainsValid &&
                result.IsFeasible;

            solution.Evaluation.FeasibilityStatus =
                feasible
                    ? FeasibilityStatus.Feasible
                    : FeasibilityStatus.Infeasible;

            solution.Evaluation.MaximumConstraintViolation =
                result.MaximumConstraintViolation;

            solution.Evaluation.TotalConstraintViolation =
                result.TotalConstraintViolation;

            solution.Evaluation.ViolatedConstraintCount =
                result.ViolatedConstraintCount;
        }
        else
        {
            solution.Evaluation.FeasibilityStatus =
                FeasibilityStatus.NotEvaluated;

            solution.Evaluation.MaximumConstraintViolation =
                null;

            solution.Evaluation.TotalConstraintViolation =
                null;

            solution.Evaluation.ViolatedConstraintCount =
                null;
        }

        if (result.ObjectiveCheckCompleted &&
            result.RecomputedObjectiveValue.HasValue)
        {
            solution.Evaluation.ObjectiveValue =
                result.RecomputedObjectiveValue.Value;

            solution.Evaluation.ObjectiveValueSource =
                ObjectiveValueSource;
        }
        else
        {
            solution.Evaluation.ObjectiveValue =
                null;

            solution.Evaluation.ObjectiveValueSource =
                string.Empty;
        }
    }

    private static void ApplyToKnownResult(
        KnownResult knownResult,
        SolutionCheckResult result,
        SolutionVerificationOptions options,
        SolutionCheckApplicationResult applicationResult)
    {
        if (options.UpdateKnownResultFeasibility &&
            CanPublishFeasibility(result))
        {
            bool feasible =
                result.IsStructurallyValid &&
                result.AreVariableDomainsValid &&
                result.IsFeasible;

            knownResult.FeasibilityStatus =
                feasible
                    ? FeasibilityStatus.Feasible
                    : FeasibilityStatus.Infeasible;

            applicationResult.KnownResultFeasibilityUpdated =
                true;
        }

        if (options.PromoteFullyVerifiedKnownResult &&
            result.Level == SolutionCheckLevel.Full &&
            result.ObjectiveCheckCompleted &&
            result.IsValid)
        {
            knownResult.VerificationStatus =
                KnownResultVerificationStatus.AutomaticallyVerified;

            applicationResult.KnownResultPromoted =
                true;
        }
    }

    private static bool CanPublishFeasibility(
        SolutionCheckResult result)
    {
        return
            result.StructuralCheckCompleted &&
            result.VariableDomainCheckCompleted &&
            result.FeasibilityCheckCompleted;
    }
}
