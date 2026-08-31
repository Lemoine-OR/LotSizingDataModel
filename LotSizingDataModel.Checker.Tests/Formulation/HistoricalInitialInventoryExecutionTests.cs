using LotSizingDataModel.Core;
using LotSizingDataModel.Core.LogicalModel;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Formulation;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Checker.Tests.Formulation;

public sealed class HistoricalInitialInventoryExecutionTests
{
    [Fact]
    public async Task VariableInitialInventory_IsARealStandardDecision()
    {
        LotSizingInstance instance =
            CreateInventoryInstance(
                InitialInventoryDecisionMode.VariableDecision);

        Inventory inventory =
            Assert.Single(instance.SupplyChain.Inventories);

        inventory.InitialInventoryDecisionUnitCost = 3.0;

        var model =
            await StandardLotSizingFormulationFactory
                .CreateDefault()
                .BuildAsync(instance);

        var initial =
            Assert.Single(
                model.Variables,
                variable =>
                    variable.DomainKey.StartsWith(
                        MathematicalDecisionCategory.InitialInventory + "|",
                        StringComparison.Ordinal));

        Assert.Contains(
            model.Objective.Expression.Terms,
            term =>
                term.VariableId == initial.Id &&
                Math.Abs(term.Coefficient - 3.0) <= 1.0e-12);

        var balance =
            Assert.Single(
                model.Constraints,
                constraint =>
                    constraint.Name.StartsWith(
                        "inventoryBalance_",
                        StringComparison.Ordinal));

        Assert.Contains(
            balance.LeftHandSide.Terms,
            term =>
                term.VariableId == initial.Id &&
                Math.Abs(term.Coefficient + 1.0) <= 1.0e-12);
    }

    [Fact]
    public async Task AbsentFixedZero_DoesNotCreateInitialInventoryVariable()
    {
        LotSizingInstance instance =
            CreateInventoryInstance(
                InitialInventoryDecisionMode.AbsentFixedZero);

        var model =
            await StandardLotSizingFormulationFactory
                .CreateDefault()
                .BuildAsync(instance);

        Assert.DoesNotContain(
            model.Variables,
            variable =>
                variable.DomainKey.StartsWith(
                    MathematicalDecisionCategory.InitialInventory + "|",
                    StringComparison.Ordinal));
    }

    private static LotSizingInstance CreateInventoryInstance(
        InitialInventoryDecisionMode mode)
    {
        var chain = new SupplyChain(1);
        chain.Items.Add(new Item(1, "I1", 0));

        var plant =
            new Plant(
                1,
                "P1",
                new PlantWarehouse("P1-Warehouse"));

        chain.Plants.Add(plant);

        var inventory =
            Inventory.ForPlantWarehouse(
                1,
                1,
                0.0);

        inventory.InitialInventoryDecisionMode = mode;

        chain.Inventories.Add(inventory);

        return new LotSizingInstance(
            chain,
            "historical-initial-stock");
    }
}
