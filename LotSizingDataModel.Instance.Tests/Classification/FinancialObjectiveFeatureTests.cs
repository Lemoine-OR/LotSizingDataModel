using LotSizingDataModel.Core;
using LotSizingDataModel.Core.DecisionModel.Finance;
using LotSizingDataModel.Core.DecisionModel.Objectives;
using LotSizingDataModel.Instance.Classification;
using LotSizingDataModel.Instance.Descriptors;

namespace LotSizingDataModel.Instance.Tests.Classification;

public sealed class FinancialObjectiveFeatureTests
{
    [Fact]
    public void Extractor_DetectsBudgetAndObjectivePolicy()
    {
        var chain =
            new SupplyChain
            {
                PlanningHorizon = 3,
                PeriodicOperatingExpenditureBudget =
                    new PeriodicOperatingExpenditureBudget(
                        3,
                        1000.0),
                ObjectivePolicy =
                    CreateWeightedPolicy()
            };

        LotSizingProblemFeatures features =
            LotSizingProblemFeatureExtractor.Extract(chain);

        Assert.True(features.HasFinancialConstraints);
        Assert.True(features.HasMultipleObjectives);
        Assert.Equal(2, features.ObjectiveCriterionCount);
        Assert.Equal(
            OptimizationObjectiveKind.Economic,
            features.PrimaryObjectiveKind);
        Assert.Equal(
            ObjectiveAggregationMode.WeightedSum,
            features.ObjectiveAggregationMode);

        LotSizingProblemFeatures roundTrip =
            LotSizingProblemDescriptor
                .FromLegacyFeatures(features)
                .ToLegacyFeatures();

        Assert.Equal(
            features.ObjectiveCriterionCount,
            roundTrip.ObjectiveCriterionCount);

        Assert.Equal(
            features.PrimaryObjectiveKind,
            roundTrip.PrimaryObjectiveKind);

        Assert.Equal(
            features.ObjectiveAggregationMode,
            roundTrip.ObjectiveAggregationMode);
    }

    [Fact]
    public void NoExplicitPolicy_PreservesHistoricalEconomicSingleObjective()
    {
        LotSizingProblemFeatures features =
            LotSizingProblemFeatureExtractor.Extract(
                new SupplyChain());

        Assert.False(features.HasMultipleObjectives);
        Assert.Equal(1, features.ObjectiveCriterionCount);
        Assert.Equal(
            OptimizationObjectiveKind.Economic,
            features.PrimaryObjectiveKind);
        Assert.Equal(
            ObjectiveAggregationMode.Single,
            features.ObjectiveAggregationMode);
    }

    private static OptimizationObjectivePolicy CreateWeightedPolicy()
    {
        var policy =
            new OptimizationObjectivePolicy
            {
                AggregationMode =
                    ObjectiveAggregationMode.WeightedSum
            };

        policy.Criteria.Add(
            new OptimizationObjectiveCriterion
            {
                Kind =
                    OptimizationObjectiveKind.Economic,
                Weight = 0.7,
                Priority = 0
            });

        policy.Criteria.Add(
            new OptimizationObjectiveCriterion
            {
                Kind =
                    OptimizationObjectiveKind.ServiceLevel,
                Weight = 0.3,
                Priority = 1
            });

        return policy;
    }
}
