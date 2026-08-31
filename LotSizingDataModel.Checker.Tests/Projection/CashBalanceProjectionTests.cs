using LotSizingDataModel.Checker.Projection;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Checker.Tests.Projection;

public sealed class CashBalanceProjectionTests
{
    [Fact]
    public void Projector_ReconstructsCashBalanceFromNormalizedSolution()
    {
        var solution =
            new LotSizingSolution(2);

        solution.SetCashBalance(1, 12.5);
        solution.SetCashBalance(2, -3.0);

        var model =
            new MathematicalModel
            {
                Name = "cash-projection"
            };

        model.Variables.Add(
            new MathematicalVariable
            {
                Id = 1,
                Name = "cash1",
                DomainKey =
                    new MathematicalDomainKeyBuilder(
                        MathematicalDecisionCategory.CashBalance)
                        .Add(
                            MathematicalDomainKeySegment.Period,
                            1)
                        .Build(),
                VariableType =
                    MathematicalVariableType.Continuous,
                LowerBound =
                    double.NegativeInfinity,
                UpperBound =
                    double.PositiveInfinity
            });

        var result =
            new MathematicalSolutionValueProjector()
                .Project(
                    model,
                    solution);

        Assert.Empty(result.Issues);
        Assert.Equal(
            12.5,
            result.Values[1],
            12);
    }
}
