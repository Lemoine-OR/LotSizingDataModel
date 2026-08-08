using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Adds inventory holding or usage costs to the standard
/// objective.
/// </summary>
public sealed class InventoryCostObjectiveTermBuilder :
    StandardLotSizingObjectiveTermBuilderBase
{
    /// <summary>
    /// Gets the objective-term family identifier.
    /// </summary>
    public override string TermFamilyId =>
        "inventoryCost";

    /// <summary>
    /// Builds inventory cost terms.
    /// </summary>
    protected override ValueTask BuildTermsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        LinearExpressionBuilder expressionBuilder,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        foreach (
            Inventory inventory
            in instance.SupplyChain.Inventories)
        {
            for (
                int period = 1;
                period <= instance.PlanningHorizon;
                period++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var keyBuilder =
                    new MathematicalDomainKeyBuilder(
                        MathematicalDecisionCategory.Inventory)
                        .Add(
                            MathematicalDomainKeySegment.Item,
                            inventory.ItemId);

                StandardFormulationDomainKeyFactory.AddWarehouse(
                    keyBuilder,
                    inventory.Warehouse);

                AddCostTerm(
                    context,
                    expressionBuilder,
                    keyBuilder
                        .Add(
                            MathematicalDomainKeySegment.Period,
                            period)
                        .Build(),
                    inventory.UnitUsageCost?[period] ??
                        0.0);
            }
        }

        return ValueTask.CompletedTask;
    }
}
