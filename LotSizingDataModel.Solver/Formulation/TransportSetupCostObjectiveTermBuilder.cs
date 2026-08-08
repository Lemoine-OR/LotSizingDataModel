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
/// Adds item-specific transport setup costs.
/// </summary>
public sealed class TransportSetupCostObjectiveTermBuilder :
    StandardLotSizingObjectiveTermBuilderBase
{
    /// <summary>
    /// Gets the objective-term family identifier.
    /// </summary>
    public override string TermFamilyId =>
        "transportSetupCost";

    /// <summary>
    /// Determines whether transport setup costs are enabled.
    /// </summary>
    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(options);

        return
            options.IncludeTransport &&
            options.IncludeTransportSetups;
    }

    /// <summary>
    /// Builds transport setup-cost terms.
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
            if (characteristic.FixedSetupCost is null &&
                characteristic.SetupTime is null)
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

                    AddCostTerm(
                        context,
                        expressionBuilder,
                        CreateKey(
                            MathematicalDecisionCategory.TransportSetup,
                            characteristic.ItemId,
                            resource.Id,
                            lane)
                        .Add(
                            MathematicalDomainKeySegment.Period,
                            period)
                        .Build(),
                        characteristic.FixedSetupCost?[period] ??
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
