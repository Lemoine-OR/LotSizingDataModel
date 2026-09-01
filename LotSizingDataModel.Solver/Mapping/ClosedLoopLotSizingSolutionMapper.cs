using LotSizingDataModel.Instance.ClosedLoop;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solution.Decisions;
using LotSizingDataModel.Solver.Execution;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Mapping;

/// <summary>
/// Maps alpha.39 mathematical closed-loop decisions into the
/// normalized LotSizingSolution extension.
/// </summary>
public sealed class ClosedLoopLotSizingSolutionMapper
{
    public void Map(
        IReadOnlyList<ClosedLoopReturnStream> streams,
        MathematicalModel model,
        MathematicalModelSolveResult solveResult,
        LotSizingSolution solution)
    {
        ArgumentNullException.ThrowIfNull(streams);
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(solveResult);
        ArgumentNullException.ThrowIfNull(solution);

        IReadOnlyList<ClosedLoopDecisionSnapshot> snapshots =
            new ClosedLoopDecisionProjector()
                .Project(
                    streams,
                    model,
                    solveResult);

        solution.ClosedLoopDecisions.Clear();

        foreach (IGrouping<int, ClosedLoopDecisionSnapshot> group
                 in snapshots.GroupBy(
                     snapshot =>
                         snapshot.ReturnStreamId))
        {
            var decision =
                new ClosedLoopDecision(
                    group.Key,
                    solution.PlanningHorizon);

            foreach (ClosedLoopDecisionSnapshot snapshot
                     in group)
            {
                decision.RecoveryInputs[snapshot.Period] =
                    snapshot.RecoveryInput;

                decision.DisposalQuantities[snapshot.Period] =
                    snapshot.DisposalQuantity;

                decision.RecoveredOutputs[snapshot.Period] =
                    snapshot.RecoveredOutput;
            }

            solution.ClosedLoopDecisions.Add(
                decision);
        }

        if (!solution.HasValidClosedLoopDecisions)
        {
            throw new InvalidOperationException(
                "Mapped closed-loop decisions are not internally valid.");
        }
    }
}
