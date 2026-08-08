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
/// Builds item-specific transported-quantity variables on each
/// lane of an allowed transport resource.
/// </summary>
public sealed class TransportVariableFamilyBuilder :
    StandardLotSizingVariableFamilyBuilderBase
{
    /// <summary>
    /// Gets the transport variable-family identifier.
    /// </summary>
    public override string FamilyId =>
        MathematicalDecisionCategory.Transport;

    /// <summary>
    /// Determines whether transport variables are enabled.
    /// </summary>
    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(options);

        return
            options.IncludeTransport &&
            instance.SupplyChain.TransportCharacteristics.Count > 0 &&
            instance.SupplyChain.TransportResources.Count > 0;
    }

    /// <summary>
    /// Builds transported-quantity variables.
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
            cancellationToken.ThrowIfCancellationRequested();

            TransportResource resource =
                instance.SupplyChain.TransportResources
                    .FirstOrDefault(
                        candidate =>
                            candidate.Id ==
                            characteristic.TransportResourceId)
                ?? throw new InvalidOperationException(
                    $"Transport resource " +
                    $"{characteristic.TransportResourceId} " +
                    "referenced by a transport characteristic " +
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

                    string domainKey =
                        CreateTransportKey(
                            MathematicalDecisionCategory.Transport,
                            characteristic.ItemId,
                            resource,
                            lane)
                        .Add(
                            MathematicalDomainKeySegment.Period,
                            period)
                        .Build();

                    AddNonNegativeContinuousVariable(
                        context,
                        $"T_i{characteristic.ItemId}_r{resource.Id}" +
                        $"_o{lane.Origin.ReferenceId}" +
                        $"_d{lane.Destination.ReferenceId}_t{period}",
                        domainKey,
                        double.PositiveInfinity,
                        $"Transported quantity of item " +
                        $"{characteristic.ItemId} using resource " +
                        $"{resource.Id} in period {period}.");
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
