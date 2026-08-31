using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Solver.Formulation;

internal static class ClosedLoopVariableKeyFactory
{
    public static string CreateRecoveryKey(
        int streamId,
        int itemId,
        int distributionCenterId,
        int period)
    {
        return CreateKey(
            ClosedLoopMathematicalDecisionCategory
                .RecoveryInput,
            streamId,
            itemId,
            distributionCenterId,
            period);
    }

    public static string CreateDisposalKey(
        int streamId,
        int itemId,
        int distributionCenterId,
        int period)
    {
        return CreateKey(
            ClosedLoopMathematicalDecisionCategory
                .Disposal,
            streamId,
            itemId,
            distributionCenterId,
            period);
    }

    private static string CreateKey(
        string category,
        int streamId,
        int itemId,
        int distributionCenterId,
        int period)
    {
        return new MathematicalDomainKeyBuilder(
                category)
            .Add(
                ClosedLoopMathematicalDomainKeySegment
                    .ReturnStream,
                streamId)
            .Add(
                MathematicalDomainKeySegment.Item,
                itemId)
            .Add(
                MathematicalDomainKeySegment
                    .DistributionCenter,
                distributionCenterId)
            .Add(
                MathematicalDomainKeySegment.Period,
                period)
            .Build();
    }
}
