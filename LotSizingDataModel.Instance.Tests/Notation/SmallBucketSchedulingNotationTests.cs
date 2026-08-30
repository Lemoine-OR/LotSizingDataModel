using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Instance.Descriptors;
using LotSizingDataModel.Instance.Notation;

namespace LotSizingDataModel.Instance.Tests.Notation;

public sealed class SmallBucketSchedulingNotationTests
{
    [Fact]
    public void DlspSemantics_RenderWithExplicitSmallBucketTokens()
    {
        var descriptor =
            new LotSizingProblemDescriptor
            {
                Scheduling =
                    new SchedulingDescriptor
                    {
                        HasIntegratedScheduling = true,
                        BucketMode =
                            SchedulingBucketMode.SmallBucket,
                        SchedulingResourceCount = 1,
                        SmallBucketProductionMode =
                            SmallBucketProductionMode.AllOrNothing,
                        HasMaximumProducedItemCountConstraint = true,
                        MaximumProducedItemCountPerBucket = 1
                    }
            };

        string notation =
            new UniversalNotationGenerator()
                .Generate(descriptor)
                .Render();

        Assert.Contains(
            "SchedRes:1",
            notation,
            StringComparison.Ordinal);

        Assert.Contains(
            "SBProd:0F",
            notation,
            StringComparison.Ordinal);

        Assert.Contains(
            "BucketItems:1",
            notation,
            StringComparison.Ordinal);

        Assert.Equal(
            notation,
            new UniversalNotationParser()
                .Parse(notation)
                .Render());
    }

    [Fact]
    public void PlspSemantics_RenderAtMostTwoItemsAndOneTransition()
    {
        var descriptor =
            new LotSizingProblemDescriptor
            {
                Scheduling =
                    new SchedulingDescriptor
                    {
                        HasIntegratedScheduling = true,
                        BucketMode =
                            SchedulingBucketMode.SmallBucket,
                        SchedulingResourceCount = 1,
                        SmallBucketProductionMode =
                            SmallBucketProductionMode.Continuous,
                        HasMaximumProducedItemCountConstraint = true,
                        MaximumProducedItemCountPerBucket = 2,
                        HasMaximumSetupCountConstraints = true,
                        MaximumSetupTransitionsPerBucket = 1
                    }
            };

        string notation =
            new UniversalNotationGenerator()
                .Generate(descriptor)
                .Render();

        Assert.Contains(
            "SBProd:Cont",
            notation,
            StringComparison.Ordinal);

        Assert.Contains(
            "BucketItems:2",
            notation,
            StringComparison.Ordinal);

        Assert.Contains(
            "SetupTrans:1",
            notation,
            StringComparison.Ordinal);
    }
}
