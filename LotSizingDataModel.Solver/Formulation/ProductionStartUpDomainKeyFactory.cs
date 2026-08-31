using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Builds canonical mathematical domain keys for production start-up events.
/// </summary>
/// <remarks>
/// Start-up is deliberately distinct from setup state, setup-start occurrence,
/// and sequence-dependent changeover. The same physical transition may trigger
/// several independent accounting/capacity terms, but each term keeps its own
/// parameter family and mathematical category.
/// </remarks>
internal static class ProductionStartUpDomainKeyFactory
{
    public static string CreateStandardKey(
        int routingId,
        int period) =>
        new MathematicalDomainKeyBuilder(
            MathematicalDecisionCategory.AuxiliaryProductionStartUp)
            .Add(MathematicalDomainKeySegment.Routing, routingId)
            .Add(MathematicalDomainKeySegment.Period, period)
            .Build();

    public static string CreateSmallBucketKey(
        ProductionSchedulingProfile profile,
        ProductionRouting routing,
        int period)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(routing);

        var builder =
            new MathematicalDomainKeyBuilder(
                MathematicalDecisionCategory.AuxiliaryProductionStartUp)
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
            builder.Add(
                MathematicalDomainKeySegment.SetupReset,
                1);
        }

        return builder.Build();
    }

    public static string CreateGlspKey(
        int plantId,
        int workCenterId,
        int routingId,
        int itemId,
        ProductionMicroPeriodReference microPeriod,
        int fixedPredecessorItemId,
        bool resetBoundary)
    {
        var builder =
            new MathematicalDomainKeyBuilder(
                MathematicalDecisionCategory.AuxiliaryProductionStartUp)
                .Add(MathematicalDomainKeySegment.Plant, plantId)
                .Add(MathematicalDomainKeySegment.WorkCenter, workCenterId)
                .Add(MathematicalDomainKeySegment.Period, microPeriod.MacroPeriod)
                .Add(MathematicalDomainKeySegment.MicroPeriod, microPeriod.MicroPeriodIndex)
                .Add(MathematicalDomainKeySegment.Routing, routingId)
                .Add(MathematicalDomainKeySegment.Item, itemId);

        if (fixedPredecessorItemId > 0)
        {
            builder.Add(
                MathematicalDomainKeySegment.FromItem,
                fixedPredecessorItemId);
        }

        if (resetBoundary)
        {
            builder.Add(
                MathematicalDomainKeySegment.SetupReset,
                1);
        }

        return builder.Build();
    }
}
