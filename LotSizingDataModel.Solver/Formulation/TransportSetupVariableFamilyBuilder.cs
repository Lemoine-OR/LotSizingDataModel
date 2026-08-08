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
/// Builds item-specific binary transport-setup variables.
/// </summary>
public sealed class TransportSetupVariableFamilyBuilder :
    StandardLotSizingVariableFamilyBuilderBase
{
    /// <summary>
    /// Gets the transport-setup variable-family identifier.
    /// </summary>
    public override string FamilyId =>
        MathematicalDecisionCategory.TransportSetup;

    /// <summary>
    /// Determines whether transport setup variables are enabled.
    /// </summary>
    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(options);

        return
            options.IncludeTransport &&
            options.IncludeTransportSetups &&
            instance.SupplyChain.TransportCharacteristics.Any(
                characteristic =>
                    characteristic.FixedSetupCost is not null ||
                    characteristic.SetupTime is not null);
    }

    /// <summary>
    /// Builds transport-setup variables.
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
            if (characteristic.FixedSetupCost is null &&
                characteristic.SetupTime is null)
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

                    string domainKey =
                        CreateTransportKey(
                            MathematicalDecisionCategory.TransportSetup,
                            characteristic.ItemId,
                            resource,
                            lane)
                        .Add(
                            MathematicalDomainKeySegment.Period,
                            period)
                        .Build();

                    AddBinaryVariable(
                        context,
                        $"YT_i{characteristic.ItemId}_r{resource.Id}" +
                        $"_o{lane.Origin.ReferenceId}" +
                        $"_d{lane.Destination.ReferenceId}_t{period}",
                        domainKey,
                        $"Transport setup for item " +
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
