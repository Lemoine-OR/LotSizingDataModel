using System;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Links aggregate warehouse inventory load to binary warehouse activation.
/// </summary>
public sealed class WarehouseActivationLinkConstraintFamilyBuilder :
    StandardLotSizingConstraintFamilyBuilderBase
{
    /// <summary>Gets the family identifier.</summary>
    public override string ConstraintFamilyId => "warehouseActivationLink";

    /// <summary>Determines whether the family is enabled.</summary>
    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(options);

        if (!options.IncludeResourceActivation)
        {
            return false;
        }

        foreach (var entry in StandardFormulationResourceEnumerator
                     .EnumerateWarehouses(instance.SupplyChain))
        {
            if (entry.Warehouse.FixedUsageCost is not null &&
                entry.Warehouse.CapacityConstraint is not null)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Builds warehouse activation-link constraints.</summary>
    protected override ValueTask BuildConstraintsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        foreach (var entry in StandardFormulationResourceEnumerator
                     .EnumerateWarehouses(instance.SupplyChain))
        {
            if (entry.Warehouse.FixedUsageCost is null ||
                entry.Warehouse.CapacityConstraint is null)
            {
                continue;
            }

            for (int period = 1; period <= instance.PlanningHorizon; period++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var expression = new LinearExpressionBuilder();

                foreach (Inventory inventory in instance.SupplyChain.Inventories)
                {
                    if (!StandardFormulationDomainKeyFactory.AreSameWarehouse(
                            inventory.Warehouse,
                            entry.Reference))
                    {
                        continue;
                    }

                    expression.Add(
                        context.GetVariable(
                            StandardFormulationVariableKeyFactory
                                .CreateInventoryKey(
                                    MathematicalDecisionCategory.Inventory,
                                    inventory.ItemId,
                                    inventory.Warehouse,
                                    period)),
                        inventory.UnitCapacityConsumption?[period] ?? 1.0);
                }

                var activationKeyBuilder =
                    new MathematicalDomainKeyBuilder(
                        MathematicalDecisionCategory.WarehouseActivation);

                StandardFormulationDomainKeyFactory.AddWarehouse(
                    activationKeyBuilder,
                    entry.Reference);

                string activationKey =
                    activationKeyBuilder
                        .Add(MathematicalDomainKeySegment.Period, period)
                        .Build();

                expression.Subtract(
                    context.GetVariable(activationKey),
                    entry.Warehouse.CapacityConstraint[period]);

                AddConstraint(
                    context,
                    $"warehouseActivationLink_w" +
                    $"{entry.Reference.ReferenceId}_t{period}",
                    expression.Build(),
                    MathematicalConstraintSense.LessThanOrEqual,
                    0.0,
                    description:
                        "Warehouse inventory load requires activation.");
            }
        }

        return ValueTask.CompletedTask;
    }
}
