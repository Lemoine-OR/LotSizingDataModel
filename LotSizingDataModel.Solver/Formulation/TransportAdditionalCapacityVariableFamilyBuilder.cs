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
/// Builds item-specific additional transport-capacity variables.
/// </summary>
public sealed class TransportAdditionalCapacityVariableFamilyBuilder :
    StandardLotSizingVariableFamilyBuilderBase
{
    /// <summary>
    /// Gets the item-specific transport additional-capacity
    /// variable-family identifier.
    /// </summary>
    public override string FamilyId =>
        MathematicalDecisionCategory.TransportAdditionalCapacity;

    /// <summary>
    /// Determines whether item-specific additional transport
    /// capacity variables are enabled.
    /// </summary>
    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(options);

        return
            options.IncludeTransport &&
            options.IncludeAdditionalCapacity &&
            instance.SupplyChain.TransportCharacteristics.Any(
                characteristic =>
                    characteristic.AdditionalCapacity is not null);
    }

    /// <summary>
    /// Builds item-specific additional-capacity variables.
    /// </summary>
    protected override ValueTask BuildFamilyAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        int planningHorizon =
            instance.PlanningHorizon;

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
                    .FirstOrDefault(
                        candidate =>
                            candidate.Id ==
                            characteristic.TransportResourceId)
                ?? throw new InvalidOperationException(
                    $"Transport resource " +
                    $"{characteristic.TransportResourceId} " +
                    "does not exist.");

            foreach (
                TransportLane lane
                in resource.Lanes)
            {
                for (
                    int period = 1;
                    period <= planningHorizon;
                    period++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    double upperBound =
                        characteristic.AdditionalCapacity[period];

                    if (IsStructurallyZero(
                            upperBound,
                            options))
                    {
                        continue;
                    }

                    string domainKey =
                        CreateTransportKey(
                            MathematicalDecisionCategory
                                .TransportAdditionalCapacity,
                            characteristic.ItemId,
                            resource,
                            lane)
                        .Add(
                            MathematicalDomainKeySegment.Period,
                            period)
                        .Build();

                    AddNonNegativeContinuousVariable(
                        context,
                        $"OT_i{characteristic.ItemId}_r{resource.Id}" +
                        $"_o{lane.Origin.ReferenceId}" +
                        $"_d{lane.Destination.ReferenceId}_t{period}",
                        domainKey,
                        upperBound,
                        $"Additional transport capacity for item " +
                        $"{characteristic.ItemId}, resource " +
                        $"{resource.Id}, period {period}.");
                }
            }
        }

        return ValueTask.CompletedTask;
    }


    private static MathematicalDomainKeyBuilder CreateTransportKey(
        string category,
        int itemId,
        TransportResource resource,
        TransportLane lane)
    {
        var keyBuilder =
            new MathematicalDomainKeyBuilder(
                category)
                .Add(
                    MathematicalDomainKeySegment.Item,
                    itemId)
                .Add(
                    MathematicalDomainKeySegment.TransportResource,
                    resource.Id);

        StandardFormulationDomainKeyFactory.AddOriginWarehouse(
            keyBuilder,
            lane.Origin);

        StandardFormulationDomainKeyFactory.AddDestinationWarehouse(
            keyBuilder,
            lane.Destination);

        return keyBuilder;
    }

}
