using LotSizingDataModel.Instance.Classification;
using LotSizingDataModel.Instance.Descriptors;

namespace LotSizingDataModel.Instance.Tests.Descriptors;

public sealed class ProductionCapacityRegimeTests
{
    [Fact]
    public void NoProduction_IsNotApplicable()
    {
        LotSizingProblemDescriptor descriptor =
            LotSizingProblemDescriptor.FromLegacyFeatures(
                new LotSizingProblemFeatures
                {
                    ItemCount = 1,
                    PlanningHorizon = 3
                });

        Assert.Equal(
            ProductionCapacityRegime.NotApplicable,
            descriptor.ProductionCapacityRegime);
    }

    [Fact]
    public void ProductionWithoutCapacity_IsUncapacitated()
    {
        LotSizingProblemDescriptor descriptor =
            LotSizingProblemDescriptor.FromLegacyFeatures(
                new LotSizingProblemFeatures
                {
                    ItemCount = 1,
                    PlanningHorizon = 3,
                    HasProduction = true
                });

        Assert.Equal(
            ProductionCapacityRegime.Uncapacitated,
            descriptor.ProductionCapacityRegime);
    }

    [Fact]
    public void NonVaryingProductionCapacity_IsConstant()
    {
        LotSizingProblemDescriptor descriptor =
            LotSizingProblemDescriptor.FromLegacyFeatures(
                new LotSizingProblemFeatures
                {
                    ItemCount = 1,
                    PlanningHorizon = 3,
                    HasProduction = true,
                    HasProductionCapacityConstraints = true
                });

        Assert.Equal(
            ProductionCapacityRegime.Constant,
            descriptor.ProductionCapacityRegime);
    }

    [Fact]
    public void VaryingProductionCapacity_IsTimeVarying()
    {
        LotSizingProblemDescriptor descriptor =
            LotSizingProblemDescriptor.FromLegacyFeatures(
                new LotSizingProblemFeatures
                {
                    ItemCount = 1,
                    PlanningHorizon = 3,
                    HasProduction = true,
                    HasProductionCapacityConstraints = true,
                    HasTimeVaryingProductionCapacity = true
                });

        Assert.Equal(
            ProductionCapacityRegime.TimeVarying,
            descriptor.ProductionCapacityRegime);
    }
}
