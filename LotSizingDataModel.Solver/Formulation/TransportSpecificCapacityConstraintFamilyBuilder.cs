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

/// <summary>
/// Builds item-specific transport capacity constraints.
/// </summary>
public sealed class TransportSpecificCapacityConstraintFamilyBuilder :
    StandardLotSizingConstraintFamilyBuilderBase
{
    /// <summary>
    /// Gets the unique constraint-family identifier.
    /// </summary>
    public override string ConstraintFamilyId =>
        "transportSpecificCapacity";

    /// <summary>
    /// Determines whether item-specific transport capacities
    /// exist.
    /// </summary>
    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(options);

        return
            options.IncludeTransport &&
            instance.SupplyChain.TransportCharacteristics.Any(
                characteristic =>
                    characteristic.CapacityConstraint is not null);
    }

    /// <summary>
    /// Builds item-specific transport capacity constraints.
    /// </summary>
    protected override ValueTask BuildConstraintsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        foreach (TransportCharacteristic characteristic
                 in instance.SupplyChain.TransportCharacteristics)
        {
            if (characteristic.CapacityConstraint is null)
            {
                continue;
            }

            TransportResource resource =
                instance.SupplyChain.TransportResources
                    .First(
                        candidate =>
                            candidate.Id ==
                            characteristic.TransportResourceId);

            for (int period = 1;
                 period <= instance.PlanningHorizon;
                 period++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var expression =
                    new LinearExpressionBuilder();

                foreach (TransportLane lane in resource.Lanes)
                {
                    string transportKey =
                        CreateKey(
                            MathematicalDecisionCategory.Transport,
                            characteristic.ItemId,
                            resource.Id,
                            lane,
                            period);

                    expression.Add(
                        context.GetVariable(transportKey),
                        characteristic.UnitCapacityConsumption?[period] ??
                            1.0);

                    double setupTime =
                        characteristic.SetupTime?[period] ??
                        0.0;

                    if (setupTime > 0.0)
                    {
                        string setupKey =
                            CreateKey(
                                MathematicalDecisionCategory.TransportSetup,
                                characteristic.ItemId,
                                resource.Id,
                                lane,
                                period);

                        if (context.VariableRegistry.TryGet(
                                setupKey,
                                out MathematicalVariable? setupVariable) &&
                            setupVariable is not null)
                        {
                            expression.Add(
                                setupVariable,
                                setupTime);
                        }
                    }

                    string additionalKey =
                        CreateKey(
                            MathematicalDecisionCategory
                                .TransportAdditionalCapacity,
                            characteristic.ItemId,
                            resource.Id,
                            lane,
                            period);

                    if (context.VariableRegistry.TryGet(
                            additionalKey,
                            out MathematicalVariable? additionalVariable) &&
                        additionalVariable is not null)
                    {
                        expression.Subtract(
                            additionalVariable);
                    }
                }

                AddConstraint(
                    context,
                    $"transportSpecificCapacity_i" +
                    $"{characteristic.ItemId}_r{resource.Id}_t{period}",
                    expression.Build(),
                    MathematicalConstraintSense.LessThanOrEqual,
                    characteristic.CapacityConstraint[period],
                    description:
                        "Item-specific transport capacity " +
                        "constraint.");
            }
        }

        return ValueTask.CompletedTask;
    }

    private static string CreateKey(
        string category,
        int itemId,
        int resourceId,
        TransportLane lane,
        int period)
    {
        var keyBuilder =
            new MathematicalDomainKeyBuilder(category)
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

        return keyBuilder
            .Add(
                MathematicalDomainKeySegment.Period,
                period)
            .Build();
    }
}
