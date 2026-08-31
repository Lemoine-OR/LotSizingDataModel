using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Solver.Formulation;

internal static class InitialInventoryDecisionDomainKeyFactory
{
    public static string Create(Inventory inventory)
    {
        ArgumentNullException.ThrowIfNull(inventory);

        var builder =
            new MathematicalDomainKeyBuilder(
                MathematicalDecisionCategory.InitialInventory)
                .Add(
                    MathematicalDomainKeySegment.Item,
                    inventory.ItemId);

        StandardFormulationDomainKeyFactory.AddWarehouse(
            builder,
            inventory.Warehouse);

        return builder.Build();
    }
}
