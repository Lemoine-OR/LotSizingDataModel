using LotSizingDataModel.Instance.Classification;
using LotSizingDataModel.Instance.Descriptors;
using LotSizingDataModel.Instance.Notation;
using LotSizingDataModel.Instance.Notation.Matching;

namespace LotSizingDataModel.Instance.Tests.Notation;

public sealed class UniversalSemanticConditionTests
{
    [Fact]
    public void ParserRenderer_CanonicalizesSemanticConditions()
    {
        string raw =
            "1,SL,Net:UNK | ProdMode:0F,Dem,Cost:NS,Prod,Uncap:P " +
            "| Obj:Econ";

        string canonical =
            new UniversalNotationParser()
                .Canonicalize(raw);

        Assert.Equal(
            "1,SL,Net:UNK | Dem,Prod,Uncap:P,Cost:NS,ProdMode:0F " +
            "| Obj:Econ",
            canonical);
    }

    [Fact]
    public void DerivedSemantics_SatisfiedConditionsAreGenerated()
    {
        LotSizingProblemDescriptor descriptor =
            CreateUncapacitatedDescriptor();

        var derived =
            new UniversalDerivedSemantics(
                conditionAssessments:
                    new[]
                    {
                        new UniversalSemanticConditionAssessment(
                            UniversalSemanticCondition
                                .NonSpeculativeProductionHoldingCosts,
                            UniversalConditionState.Satisfied)
                    });

        string notation =
            new UniversalNotationGenerator()
                .Generate(
                    descriptor,
                    derived)
                .Render();

        Assert.Contains(
            "Cost:NS",
            notation);
    }

    [Fact]
    public void RequiredConditionWithoutAnalysis_IsIncomplete()
    {
        LotSizingProblemDescriptor descriptor =
            CreateUncapacitatedDescriptor();

        UniversalNotationMatchResult result =
            new UniversalNotationMatcher()
                .Match(
                    descriptor,
                    "1,SL,Net:UNK | Dem,Prod,Uncap:P,Cost:NS | Obj:Econ");

        Assert.Equal(
            UniversalNotationMatchKind.Incomplete,
            result.Kind);

        Assert.Contains(
            result.Issues,
            issue =>
                issue.Code == "LSDM-MATCH-033");
    }

    [Fact]
    public void RequiredConditionKnownFalse_IsContradiction()
    {
        LotSizingProblemDescriptor descriptor =
            CreateUncapacitatedDescriptor();

        var derived =
            new UniversalDerivedSemantics(
                conditionAssessments:
                    new[]
                    {
                        new UniversalSemanticConditionAssessment(
                            UniversalSemanticCondition
                                .NonSpeculativeProductionHoldingCosts,
                            UniversalConditionState.NotSatisfied)
                    });

        UniversalNotationMatchResult result =
            new UniversalNotationMatcher()
                .Match(
                    descriptor,
                    "1,SL,Net:UNK | Dem,Prod,Uncap:P,Cost:NS | Obj:Econ",
                    derived);

        Assert.Equal(
            UniversalNotationMatchKind.Contradiction,
            result.Kind);

        Assert.Contains(
            result.Issues,
            issue =>
                issue.Code == "LSDM-MATCH-034");
    }

    [Fact]
    public void RequiredConditionKnownTrue_IsExactWhenSpecificationIsComplete()
    {
        LotSizingProblemDescriptor descriptor =
            CreateUncapacitatedDescriptor();

        var derived =
            new UniversalDerivedSemantics(
                conditionAssessments:
                    new[]
                    {
                        new UniversalSemanticConditionAssessment(
                            UniversalSemanticCondition
                                .NonSpeculativeProductionHoldingCosts,
                            UniversalConditionState.Satisfied)
                    });

        UniversalNotationMatchResult result =
            new UniversalNotationMatcher()
                .Match(
                    descriptor,
                    "1,SL,Net:UNK | Dem,Det,Prod,Uncap:P,Cost:NS | Obj:Econ",
                    derived);

        Assert.Equal(
            UniversalNotationMatchKind.Exact,
            result.Kind);

        Assert.Empty(
            result.Issues);
    }

    [Fact]
    public void RequiredConditionKnownTrue_IsCompatibleWhenSpecificationIsLessSpecific()
    {
        LotSizingProblemDescriptor descriptor =
            CreateUncapacitatedDescriptor();

        var derived =
            new UniversalDerivedSemantics(
                conditionAssessments:
                    new[]
                    {
                        new UniversalSemanticConditionAssessment(
                            UniversalSemanticCondition
                                .NonSpeculativeProductionHoldingCosts,
                            UniversalConditionState.Satisfied)
                    });

        UniversalNotationMatchResult result =
            new UniversalNotationMatcher()
                .Match(
                    descriptor,
                    "1,SL,Net:UNK | Dem,Prod,Uncap:P,Cost:NS | Obj:Econ",
                    derived);

        Assert.Equal(
            UniversalNotationMatchKind.Compatible,
            result.Kind);

        Assert.Empty(
            result.Issues);
    }

    [Fact]
    public void ConflictingActualConditionStates_AreRejected()
    {
        Assert.Throws<ArgumentException>(
            () =>
                new UniversalDerivedSemantics(
                    conditionAssessments:
                        new[]
                        {
                            new UniversalSemanticConditionAssessment(
                                UniversalSemanticCondition
                                    .ZeroOrFullCapacityProduction,
                                UniversalConditionState.Satisfied),

                            new UniversalSemanticConditionAssessment(
                                UniversalSemanticCondition
                                    .ZeroOrFullCapacityProduction,
                                UniversalConditionState.NotSatisfied)
                        }));
    }

    private static LotSizingProblemDescriptor
        CreateUncapacitatedDescriptor()
    {
        return LotSizingProblemDescriptor.FromLegacyFeatures(
            new LotSizingProblemFeatures
            {
                ItemCount = 1,
                PlanningHorizon = 4,
                HasDemand = true,
                HasDeterministicDemand = true,
                HasProduction = true
            });
    }
}
