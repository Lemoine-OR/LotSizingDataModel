using LotSizingDataModel.Core.DecisionModel.Finance;
using LotSizingDataModel.Core.DecisionModel.Objectives;

namespace LotSizingDataModel.Core.Tests.DecisionModel;

public sealed class FinancialObjectivePolicyTests
{
    [Fact]
    public void PeriodicBudget_IsNonNegative()
    {
        var budget =
            new PeriodicOperatingExpenditureBudget(
                3,
                100.0);

        Assert.Equal(
            100.0,
            budget.GetBudget(2));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => budget.SetBudget(1, -1.0));
    }

    [Fact]
    public void SinglePolicy_RequiresExactlyOneEnabledCriterion()
    {
        var policy =
            new OptimizationObjectivePolicy
            {
                AggregationMode =
                    ObjectiveAggregationMode.Single
            };

        policy.Criteria.Add(
            new OptimizationObjectiveCriterion
            {
                Kind =
                    OptimizationObjectiveKind.Economic
            });

        policy.EnsureValid();

        policy.Criteria.Add(
            new OptimizationObjectiveCriterion
            {
                Kind =
                    OptimizationObjectiveKind.ServiceLevel
            });

        Assert.Throws<InvalidOperationException>(
            policy.EnsureValid);
    }

    [Fact]
    public void WeightedSumPolicy_RequiresRealMultipleCriteria()
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
                Weight = 0.7
            });

        policy.Criteria.Add(
            new OptimizationObjectiveCriterion
            {
                Kind =
                    OptimizationObjectiveKind.ServiceLevel,
                Weight = 0.3
            });

        policy.EnsureValid();

        Assert.True(
            policy.HasMultipleEnabledCriteria);

        Assert.Equal(
            OptimizationObjectiveKind.Economic,
            policy.PrimaryObjectiveKind);
    }
}
