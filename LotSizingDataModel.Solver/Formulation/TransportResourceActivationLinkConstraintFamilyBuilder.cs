using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>Links each departure to the shared resource activation and existing per-lane item setup.</summary>
public sealed class TransportResourceActivationLinkConstraintFamilyBuilder : StandardLotSizingConstraintFamilyBuilderBase
{
    public override string ConstraintFamilyId => "transportResourceActivationLink";
    public override bool IsEnabled(LotSizingInstance instance, StandardLotSizingFormulationOptions options) =>
        options.IncludeTransport && (options.IncludeResourceActivation || options.IncludeTransportSetups);

    protected override ValueTask BuildConstraintsAsync(LotSizingInstance instance, MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options, CancellationToken cancellationToken)
    {
        foreach (var characteristic in instance.SupplyChain.TransportCharacteristics)
        {
            var resource = instance.SupplyChain.TransportResources.Single(r => r.Id == characteristic.TransportResourceId);
            bool activate = options.IncludeResourceActivation && resource.FixedUsageCost is not null;
            bool setup = options.IncludeTransportSetups && (characteristic.FixedSetupCost is not null || characteristic.SetupTime is not null);
            if (!activate && !setup) continue;
            foreach (var lane in instance.SupplyChain.GetTransportLanes(resource.Id))
            for (int period = 1; period <= instance.PlanningHorizon; period++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                double bound = QuantityBound(resource, characteristic, period, options);
                var quantity = context.GetVariable(StandardFormulationVariableKeyFactory.CreateTransportKey(
                    characteristic.ItemId, resource.Id, lane.Origin, lane.Destination, period));
                if (activate)
                {
                    string key = new MathematicalDomainKeyBuilder(MathematicalDecisionCategory.TransportResourceActivation)
                        .Add(MathematicalDomainKeySegment.TransportResource, resource.Id)
                        .Add(MathematicalDomainKeySegment.Period, period).Build();
                    AddConstraint(context, $"transportResourceActivationLink_i{characteristic.ItemId}_r{resource.Id}_l{lane.Id}_t{period}",
                        new LinearExpressionBuilder().Add(quantity).Subtract(context.GetVariable(key), bound).Build(),
                        MathematicalConstraintSense.LessThanOrEqual, 0, description: "Departure requires the shared resource activation.");
                }
                if (setup)
                {
                    var key = new MathematicalDomainKeyBuilder(MathematicalDecisionCategory.TransportSetup)
                        .Add(MathematicalDomainKeySegment.Item, characteristic.ItemId)
                        .Add(MathematicalDomainKeySegment.TransportResource, resource.Id);
                    StandardFormulationDomainKeyFactory.AddOriginWarehouse(key, lane.Origin);
                    StandardFormulationDomainKeyFactory.AddDestinationWarehouse(key, lane.Destination);
                    AddConstraint(context, $"transportSetupLink_i{characteristic.ItemId}_r{resource.Id}_l{lane.Id}_t{period}",
                        new LinearExpressionBuilder().Add(quantity).Subtract(context.GetVariable(key.Add(MathematicalDomainKeySegment.Period, period).Build()), bound).Build(),
                        MathematicalConstraintSense.LessThanOrEqual, 0, description: "Departure requires the existing per-lane item transport setup.");
                }
            }
        }
        return ValueTask.CompletedTask;
    }

    private static double QuantityBound(TransportResource resource, TransportCharacteristic characteristic, int period,
        StandardLotSizingFormulationOptions options)
    {
        double unit = characteristic.UnitCapacityConsumption?[period] ?? 1;
        double bound = double.PositiveInfinity;
        if (unit > 0)
        {
            if (resource.CapacityConstraint is not null)
                bound = (resource.CapacityConstraint[period] + (options.IncludeAdditionalCapacity ? resource.AdditionalCapacity?[period] ?? 0 : 0)) / unit;
            if (characteristic.CapacityConstraint is not null)
                bound = Math.Min(bound, (characteristic.CapacityConstraint[period] + (options.IncludeAdditionalCapacity ? characteristic.AdditionalCapacity?[period] ?? 0 : 0)) / unit);
        }
        if (double.IsFinite(bound)) return bound;
        return options.TransportActivationBigM ?? throw new InvalidOperationException(
            $"Transport activation for item {characteristic.ItemId}, resource {resource.Id}, period {period} requires a finite capacity-derived quantity bound or explicit TransportActivationBigM.");
    }
}
