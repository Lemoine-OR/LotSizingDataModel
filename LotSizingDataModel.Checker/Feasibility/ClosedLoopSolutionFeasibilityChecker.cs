using LotSizingDataModel.Instance;
using LotSizingDataModel.Instance.ClosedLoop;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solution.Common;
using LotSizingDataModel.Solution.Decisions;

namespace LotSizingDataModel.Checker.Feasibility;

/// <summary>
/// Independent semantic checker for normalized closed-loop
/// solution decisions.
/// </summary>
public sealed class ClosedLoopSolutionFeasibilityChecker
{
    public MathematicalFeasibilityCheckResult Check(
        LotSizingInstance instance,
        LotSizingSolution solution,
        double tolerance = 1.0e-6)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(solution);

        if (!double.IsFinite(tolerance) ||
            tolerance < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(tolerance));
        }

        var diagnostics =
            new List<MathematicalFeasibilityDiagnostic>();

        bool partial = false;

        foreach (ClosedLoopReturnStream stream
                 in instance.ClosedLoopReturnStreams)
        {
            ClosedLoopDecision[] decisions =
                solution.ClosedLoopDecisions
                    .Where(
                        candidate =>
                            candidate.ReturnStreamId ==
                            stream.Id)
                    .ToArray();

            if (decisions.Length == 0)
            {
                partial = true;

                diagnostics.Add(
                    new MathematicalFeasibilityDiagnostic(
                        "LSDM-FEAS-CL-001",
                        $"Closed-loop decision for return stream '{stream.Id}' is missing."));

                continue;
            }

            if (decisions.Length > 1)
            {
                diagnostics.Add(
                    new MathematicalFeasibilityDiagnostic(
                        "LSDM-FEAS-CL-002",
                        $"Closed-loop return stream '{stream.Id}' has duplicate solution decisions."));

                continue;
            }

            ClosedLoopDecision decision =
                decisions[0];

            if (decision.PlanningHorizon !=
                instance.PlanningHorizon)
            {
                diagnostics.Add(
                    new MathematicalFeasibilityDiagnostic(
                        "LSDM-FEAS-CL-003",
                        $"Closed-loop decision for stream '{stream.Id}' has an inconsistent planning horizon."));

                continue;
            }

            for (int period = 1;
                 period <= instance.PlanningHorizon;
                 period++)
            {
                double recovery =
                    decision.RecoveryInputs[period];

                double disposal =
                    decision.DisposalQuantities[period];

                double recovered =
                    decision.RecoveredOutputs[period];

                if (Math.Abs(
                        recovery +
                        disposal -
                        stream.ReturnQuantity[period]) >
                    tolerance)
                {
                    diagnostics.Add(
                        new MathematicalFeasibilityDiagnostic(
                            "LSDM-FEAS-CL-004",
                            $"Return conservation is violated for stream '{stream.Id}', period {period}."));
                }

                if (Math.Abs(
                        recovered -
                        stream.RecoveryYield *
                        recovery) >
                    tolerance)
                {
                    diagnostics.Add(
                        new MathematicalFeasibilityDiagnostic(
                            "LSDM-FEAS-CL-005",
                            $"Recovery-yield identity is violated for stream '{stream.Id}', period {period}."));
                }

                if (stream.RecoveryCapacity is not null &&
                    recovery >
                        stream.RecoveryCapacity[period] +
                        tolerance)
                {
                    diagnostics.Add(
                        new MathematicalFeasibilityDiagnostic(
                            "LSDM-FEAS-CL-006",
                            $"Recovery capacity is violated for stream '{stream.Id}', period {period}."));
                }
            }
        }

        var knownStreamIds =
            instance.ClosedLoopReturnStreams
                .Select(
                    stream =>
                        stream.Id)
                .ToHashSet();

        foreach (ClosedLoopDecision decision
                 in solution.ClosedLoopDecisions)
        {
            if (!knownStreamIds.Contains(
                    decision.ReturnStreamId))
            {
                diagnostics.Add(
                    new MathematicalFeasibilityDiagnostic(
                        "LSDM-FEAS-CL-007",
                        $"Solution contains unknown closed-loop return stream '{decision.ReturnStreamId}'."));
            }
        }

        bool infeasible =
            diagnostics.Any(
                diagnostic =>
                    diagnostic.Code !=
                    "LSDM-FEAS-CL-001");

        FeasibilityStatus status =
            infeasible
                ? FeasibilityStatus.Infeasible
                : partial
                    ? FeasibilityStatus.PartiallyEvaluated
                    : FeasibilityStatus.Feasible;

        return new MathematicalFeasibilityCheckResult(
            status,
            diagnostics);
    }
}
