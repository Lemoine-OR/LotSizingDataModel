using LotSizingDataModel.Checker.Tests.Infrastructure;
using LotSizingDataModel.Core.DecisionModel.Finance;
using LotSizingDataModel.Solver.Formulation;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Checker.Tests.Formulation;

public sealed class PeriodicOperatingExpenditureBudgetIntegrationTests
{
    [Fact]
    public async Task StandardFormulation_BuildsOneBudgetConstraintPerPeriod()
    {
        ReferenceFixtureData data =
            ReferenceFixture.Load();

        data.Instance.SupplyChain
            .PeriodicOperatingExpenditureBudget =
                new PeriodicOperatingExpenditureBudget(
                    data.Instance.PlanningHorizon,
                    1_000_000.0);

        var formulation =
            StandardLotSizingFormulationFactory.CreateDefault();

        var model =
            await formulation.BuildAsync(
                data.Instance);

        var constraints =
            model.Constraints
                .Where(
                    constraint =>
                        constraint.Name.StartsWith(
                            "periodicOperatingExpenditureBudget_t",
                            StringComparison.Ordinal))
                .ToArray();

        Assert.Equal(
            data.Instance.PlanningHorizon,
            constraints.Length);

        Assert.All(
            constraints,
            constraint =>
            {
                Assert.True(
                    constraint.RightHandSide >= 0.0);

                foreach (
                    var term
                    in constraint.LeftHandSide.Terms)
                {
                    Assert.True(
                        term.Coefficient > 0.0);

                    var variable =
                        model.FindVariableById(
                            term.VariableId)!;

                    Assert.True(
                        MathematicalDomainKey.TryParse(
                            variable.DomainKey,
                            out var key));

                    Assert.True(
                        key!.Contains(
                            MathematicalDomainKeySegment.Period));
                }
            });
    }
}
