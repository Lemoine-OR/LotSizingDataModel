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
/// Adds item-specific additional transport-capacity costs.
/// </summary>
public sealed class TransportAdditionalCapacityCostObjectiveTermBuilder :
    StandardLotSizingObjectiveTermBuilderBase
{
    /// <summary>
    /// Gets the objective-term family identifier.
    /// </summary>
    public override string TermFamilyId =>
        "transportAdditionalCapacityCost";

    /// <summary>
    /// Determines whether these costs are enabled.
    /// </summary>
    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(options);

        return
            options.IncludeTransport &&
            options.IncludeAdditionalCapacity;
    }

    /// <summary>
    /// Builds item-specific additional transport-capacity cost
    /// terms.
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
            if (characteristic.AdditionalCapacity is null)
            {
                continue;
            }

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

                    double upperBound =
                        characteristic.AdditionalCapacity[period];

                    if (options.RemoveStructurallyZeroVariables &&
                        double.IsFinite(upperBound) &&
                        upperBound <= options.StructuralZeroTolerance)
                    {
                        continue;
                    }

                    AddCostTerm(
                        context,
                        expressionBuilder,
                        CreateKey(
                            MathematicalDecisionCategory
                                .TransportAdditionalCapacity,
                            characteristic.ItemId,
                            resource.Id,
                            lane)
                        .Add(
                            MathematicalDomainKeySegment.Period,
                            period)
                        .Build(),
                        characteristic.AdditionalCapacityCost?[period] ??
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
