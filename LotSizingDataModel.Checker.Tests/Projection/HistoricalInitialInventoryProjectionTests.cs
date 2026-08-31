using LotSizingDataModel.Checker.Projection;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solution.Decisions;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Checker.Tests.Projection;

public sealed class HistoricalInitialInventoryProjectionTests
{
    [Fact]
    public void Projector_ReconstructsPeriodZeroInventoryFromNormalizedSolution()
    {
        var solution = new LotSizingSolution(1);
        var decision =
            new InventoryDecision(
                1,
                WarehouseReference.ForPlantWarehouse(1),
                1)
            {
                InitialInventoryLevel = 4.5
            };

        solution.AddInventoryDecision(decision);

        var model =
            new MathematicalModel
            {
                Name = "initial-inventory-projection"
            };

        model.Variables.Add(
            new MathematicalVariable
            {
                Id = 1,
                Name = "I0",
                DomainKey =
                    new MathematicalDomainKeyBuilder(
                        MathematicalDecisionCategory.InitialInventory)
                        .Add(
                            MathematicalDomainKeySegment.Item,
                            1)
                        .Add(
                            MathematicalDomainKeySegment.Plant,
                            1)
                        .Build(),
                VariableType =
                    MathematicalVariableType.Continuous,
                LowerBound = 0.0,
                UpperBound = double.PositiveInfinity
            });

        var result =
            new MathematicalSolutionValueProjector()
                .Project(
                    model,
                    solution);

        Assert.Empty(result.Issues);
        Assert.Equal(
            4.5,
            result.Values[1],
            12);
    }
}
