using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Adds the period-zero unit cost of variable initial inventory.
/// </summary>
public sealed class InitialInventoryCostObjectiveTermBuilder :
    StandardLotSizingObjectiveTermBuilderBase
{
    public override string TermFamilyId =>
        "initialInventoryDecisionCost";

    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(options);

        return instance.SupplyChain.Inventories.Any(
            inventory =>
                inventory.InitialInventoryDecisionMode ==
                InitialInventoryDecisionMode.VariableDecision &&
                inventory.InitialInventoryDecisionUnitCost > 0.0);
    }

    protected override ValueTask BuildTermsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        LinearExpressionBuilder expressionBuilder,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        foreach (Inventory inventory in instance.SupplyChain.Inventories)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (inventory.InitialInventoryDecisionMode !=
                    InitialInventoryDecisionMode.VariableDecision ||
                inventory.InitialInventoryDecisionUnitCost == 0.0)
            {
                continue;
            }

            AddCostTerm(
                context,
                expressionBuilder,
                InitialInventoryDecisionDomainKeyFactory.Create(inventory),
                inventory.InitialInventoryDecisionUnitCost);
        }

        return ValueTask.CompletedTask;
    }
}
