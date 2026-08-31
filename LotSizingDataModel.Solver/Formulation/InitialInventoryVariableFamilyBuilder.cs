using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Builds explicit period-zero inventory variables.
/// </summary>
public sealed class InitialInventoryVariableFamilyBuilder :
    StandardLotSizingVariableFamilyBuilderBase
{
    public override string FamilyId =>
        MathematicalDecisionCategory.InitialInventory;

    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(options);

        return instance.SupplyChain.Inventories.Any(
            inventory =>
                inventory.InitialInventoryDecisionMode ==
                InitialInventoryDecisionMode.VariableDecision);
    }

    protected override ValueTask BuildFamilyAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        foreach (Inventory inventory in instance.SupplyChain.Inventories)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (inventory.InitialInventoryDecisionMode !=
                InitialInventoryDecisionMode.VariableDecision)
            {
                continue;
            }

            if (inventory.InitialInventory != 0.0)
            {
                throw new InvalidOperationException(
                    "A variable initial-inventory decision cannot coexist " +
                    "with a non-zero fixed InitialInventory value.");
            }

            AddNonNegativeContinuousVariable(
                context,
                $"I0_i{inventory.ItemId}_w{inventory.Warehouse.ReferenceId}",
                InitialInventoryDecisionDomainKeyFactory.Create(inventory),
                double.PositiveInfinity,
                "Inventory available before period 1.");
        }

        return ValueTask.CompletedTask;
    }
}
