using LotSizingDataModel.Checker.Projection;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solution.Decisions;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Checker.Tests.Projection;

public sealed class SmallBucketAuxiliaryProjectionTests
{
    [Fact]
    public void Projector_DerivesSetupStartFromPersistedSetupStates()
    {
        var solution =
            new LotSizingSolution(3);

        var production =
            new ProductionDecision(
                routingId: 1,
                planningHorizon: 3);

        production.SetSetupActivated(1, true);
        production.SetSetupActivated(2, true);
        production.SetSetupActivated(3, false);

        solution.ProductionDecisions.Add(
            production);

        var model =
            new MathematicalModel
            {
                Name = "projection"
            };

        model.Variables.Add(
            Variable(
                1,
                MathematicalDecisionCategory
                    .AuxiliarySchedulingSetupStart,
                period: 1));

        model.Variables.Add(
            Variable(
                2,
                MathematicalDecisionCategory
                    .AuxiliarySchedulingSetupStart,
                period: 2));

        MathematicalSolutionProjectionResult result =
            new MathematicalSolutionValueProjector()
                .Project(
                    model,
                    solution);

        Assert.Empty(
            result.Issues);

        Assert.Equal(
            1.0,
            result.Values[1]);

        Assert.Equal(
            0.0,
            result.Values[2]);
    }

    [Fact]
    public void Projector_DerivesProductionActivationFromPositiveQuantity()
    {
        var solution =
            new LotSizingSolution(2);

        var production =
            new ProductionDecision(
                routingId: 1,
                planningHorizon: 2);

        production.SetQuantity(
            period: 2,
            quantity: 3.0);

        solution.ProductionDecisions.Add(
            production);

        var model =
            new MathematicalModel
            {
                Name = "production-activation-projection"
            };

        model.Variables.Add(
            Variable(
                1,
                MathematicalDecisionCategory
                    .AuxiliarySmallBucketProductionActivation,
                period: 1));

        model.Variables.Add(
            Variable(
                2,
                MathematicalDecisionCategory
                    .AuxiliarySmallBucketProductionActivation,
                period: 2));

        MathematicalSolutionProjectionResult result =
            new MathematicalSolutionValueProjector()
                .Project(
                    model,
                    solution);

        Assert.Empty(
            result.Issues);

        Assert.Equal(
            0.0,
            result.Values[1]);

        Assert.Equal(
            1.0,
            result.Values[2]);
    }

    private static MathematicalVariable Variable(
        int id,
        string category,
        int period) =>
            new()
            {
                Id = id,
                Name = $"v{id}",
                DomainKey =
                    new MathematicalDomainKeyBuilder(category)
                        .Add(
                            MathematicalDomainKeySegment.Routing,
                            1)
                        .Add(
                            MathematicalDomainKeySegment.Period,
                            period)
                        .Build(),
                VariableType =
                    MathematicalVariableType.Binary,
                LowerBound = 0.0,
                UpperBound = 1.0
            };
}
