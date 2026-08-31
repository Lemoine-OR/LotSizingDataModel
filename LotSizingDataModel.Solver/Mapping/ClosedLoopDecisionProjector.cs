using LotSizingDataModel.Instance.ClosedLoop;
using LotSizingDataModel.Solver.Execution;
using LotSizingDataModel.Solver.Formulation;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Mapping;

public sealed class ClosedLoopDecisionProjector
{
    public IReadOnlyList<ClosedLoopDecisionSnapshot> Project(
        IReadOnlyList<ClosedLoopReturnStream> streams,
        MathematicalModel model,
        MathematicalModelSolveResult solveResult)
    {
        ArgumentNullException.ThrowIfNull(
            streams);

        ArgumentNullException.ThrowIfNull(
            model);

        ArgumentNullException.ThrowIfNull(
            solveResult);

        var snapshots =
            new List<ClosedLoopDecisionSnapshot>();

        foreach (ClosedLoopReturnStream stream
                 in streams.OrderBy(
                     candidate =>
                         candidate.Id))
        {
            stream.EnsureValid();

            for (int period = 1;
                 period <= stream.PlanningHorizon;
                 period++)
            {
                MathematicalVariable recoveryVariable =
                    GetRequiredVariable(
                        model,
                        ClosedLoopVariableKeyFactory
                            .CreateRecoveryKey(
                                stream.Id,
                                stream.ItemId,
                                stream.DistributionCenterId,
                                period));

                MathematicalVariable disposalVariable =
                    GetRequiredVariable(
                        model,
                        ClosedLoopVariableKeyFactory
                            .CreateDisposalKey(
                                stream.Id,
                                stream.ItemId,
                                stream.DistributionCenterId,
                                period));

                MathematicalVariableValue recoveryValue =
                    solveResult.FindVariableValue(
                        recoveryVariable.Id) ??
                    throw new InvalidOperationException(
                        $"Closed-loop recovery value for stream '{stream.Id}', period {period}, is missing.");

                MathematicalVariableValue disposalValue =
                    solveResult.FindVariableValue(
                        disposalVariable.Id) ??
                    throw new InvalidOperationException(
                        $"Closed-loop disposal value for stream '{stream.Id}', period {period}, is missing.");

                snapshots.Add(
                    new ClosedLoopDecisionSnapshot(
                        stream.Id,
                        period,
                        recoveryValue.Value,
                        disposalValue.Value,
                        stream.RecoveryYield *
                        recoveryValue.Value));
            }
        }

        return snapshots;
    }

    private static MathematicalVariable GetRequiredVariable(
        MathematicalModel model,
        string domainKey)
    {
        MathematicalVariable[] matches =
            model.Variables
                .Where(
                    variable =>
                        string.Equals(
                            variable.DomainKey,
                            domainKey,
                            StringComparison.Ordinal))
                .ToArray();

        if (matches.Length != 1)
        {
            throw new InvalidOperationException(
                $"Expected exactly one closed-loop variable with domain key '{domainKey}'; found {matches.Length}.");
        }

        return matches[0];
    }
}
