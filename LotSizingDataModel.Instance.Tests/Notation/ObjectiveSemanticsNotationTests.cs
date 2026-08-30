using LotSizingDataModel.Core.DecisionModel.Objectives;
using LotSizingDataModel.Instance.Descriptors;
using LotSizingDataModel.Instance.Notation;

namespace LotSizingDataModel.Instance.Tests.Notation;

public sealed class ObjectiveSemanticsNotationTests
{
    [Theory]
    [InlineData("Obj:Econ")]
    [InlineData("Obj:Fin")]
    [InlineData("Obj:Sust")]
    [InlineData("Obj:Service")]
    [InlineData("Obj:Multi")]
    public void Parser_RoundTripsObjectiveGamma(string gamma)
    {
        string text =
            $"1,SL,Net:UNK | Dem,Prod | {gamma}";

        UniversalLotSizingNotation notation =
            new UniversalNotationParser().Parse(text);

        Assert.Equal(
            text,
            notation.Render());
    }

    [Fact]
    public void Generator_DoesNotConfuseFinancialConstraintWithFinancialObjective()
    {
        var descriptor =
            new LotSizingProblemDescriptor
            {
                ObjectiveFinance =
                    new ObjectiveFinanceDescriptor
                    {
                        HasFinancialConstraints = true,
                        HasMultipleObjectives = false,
                        ObjectiveCriterionCount = 1,
                        PrimaryObjectiveKind =
                            OptimizationObjectiveKind.Economic,
                        AggregationMode =
                            ObjectiveAggregationMode.Single
                    }
            };

        string text =
            new UniversalNotationGenerator()
                .Generate(descriptor)
                .Render();

        Assert.Contains(
            "Fin",
            text,
            StringComparison.Ordinal);

        Assert.EndsWith(
            "Obj:Econ",
            text,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_UsesExplicitSingleObjectiveKind()
    {
        var descriptor =
            new LotSizingProblemDescriptor
            {
                ObjectiveFinance =
                    new ObjectiveFinanceDescriptor
                    {
                        ObjectiveCriterionCount = 1,
                        PrimaryObjectiveKind =
                            OptimizationObjectiveKind.Sustainability
                    }
            };

        Assert.EndsWith(
            "Obj:Sust",
            new UniversalNotationGenerator()
                .Generate(descriptor)
                .Render(),
            StringComparison.Ordinal);
    }
}
