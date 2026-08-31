using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Solver.Formulation;

internal static class SmallBucketSchedulingDomainKeyFactory
{
    public static string CreateSetupStartKey(
        ProductionSchedulingProfile profile,
        ProductionRouting routing,
        int period)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(routing);

        var builder =
            new MathematicalDomainKeyBuilder(
                MathematicalDecisionCategory.AuxiliarySchedulingSetupStart)
                .Add(MathematicalDomainKeySegment.Routing, routing.Id)
                .Add(MathematicalDomainKeySegment.Item, routing.ItemId)
                .Add(MathematicalDomainKeySegment.Period, period);

        if (period == 1 && profile.HasInitialSetupState)
        {
            builder.Add(
                MathematicalDomainKeySegment.FromItem,
                profile.InitialSetupItemId);
        }

        if (period > 1 &&
            profile.SetupCarryOverPolicy == SetupCarryOverPolicy.Forbidden)
        {
            builder.Add(MathematicalDomainKeySegment.SetupReset, 1);
        }

        return builder.Build();
    }
}
