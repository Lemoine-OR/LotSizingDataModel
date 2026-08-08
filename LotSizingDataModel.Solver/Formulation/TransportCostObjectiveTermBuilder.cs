using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Adds item-specific transport usage costs to the standard
/// objective.
/// </summary>
public sealed class TransportCostObjectiveTermBuilder :
    StandardLotSizingObjectiveTermBuilderBase
{
    /// <summary>
    /// Gets the objective-term family identifier.
    /// </summary>
    public override string TermFamilyId =>
        "transportCost";

    /// <summary>
    /// Determines whether transport costs are enabled.
    /// </summary>
    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(options);

        return options.IncludeTransport;
    }

    /// <summary>
    /// Builds transported-quantity cost terms.
    /// </summary>
    protected override ValueTask BuildTermsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        LinearExpressionBuilder expressionBuilder,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        foreach (
            TransportCharacteristic characteristic
            in instance.SupplyChain.TransportCharacteristics)
        {
            TransportResource resource =
                instance.SupplyChain.TransportResources
                    .First(
                        candidate =>
                            candidate.Id ==
                            characteristic.TransportResourceId);

            foreach (
                TransportLane lane
                in resource.Lanes)
            {
                for (
                    int period = 1;
                    period <= instance.PlanningHorizon;
                    period++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    AddCostTerm(
                        context,
                        expressionBuilder,
                        CreateKey(
                            MathematicalDecisionCategory.Transport,
                            characteristic.ItemId,
                            resource.Id,
                            lane)
                        .Add(
                            MathematicalDomainKeySegment.Period,
                            period)
                        .Build(),
                        characteristic.UnitUsageCost?[period] ??
                            0.0);
                }
            }
        }

        return ValueTask.CompletedTask;
    }


    private static MathematicalDomainKeyBuilder CreateKey(
        string category,
        int itemId,
        int resourceId,
        LotSizingDataModel.Core.PhysicalModel.TransportLane lane)
    {
        var keyBuilder =
            new MathematicalDomainKeyBuilder(
                category)
                .Add(
                    MathematicalDomainKeySegment.Item,
                    itemId)
                .Add(
                    MathematicalDomainKeySegment.TransportResource,
                    resourceId);

        StandardFormulationDomainKeyFactory.AddOriginWarehouse(
            keyBuilder,
            lane.Origin);

        StandardFormulationDomainKeyFactory.AddDestinationWarehouse(
            keyBuilder,
            lane.Destination);

        return keyBuilder;
    }

}
