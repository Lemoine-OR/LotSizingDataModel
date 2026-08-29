using LotSizingDataModel.Core;
using LotSizingDataModel.Core.DecisionModel.Constraints;
using LotSizingDataModel.Core.DecisionModel.Costs;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance.Classification;
using LotSizingDataModel.Instance.Descriptors;
using LotSizingDataModel.Instance.ProblemClasses;

namespace LotSizingDataModel.Instance.Tests.Classification;

public sealed class StartUpFeatureExtractionTests
{
    [Fact]
    public void Extractor_DetectsStartUpCostAndStartUpTimeFromCoreData()
    {
        var chain =
            new SupplyChain
            {
                PlanningHorizon = 3
            };

        chain.ProductionCharacteristics.Add(
            new ProductionCharacteristic(
                itemId: 1,
                plantId: 1,
                workCenterId: 1)
            {
                StartUpCost =
                    new StartUpCost(3, 7.0),
                StartUpTime =
                    new StartUpTime(3, 1.5)
            });

        LotSizingProblemFeatures features =
            LotSizingProblemFeatureExtractor.Extract(chain);

        Assert.True(features.HasStartUpCosts);
        Assert.True(features.HasStartUpTimes);

        LotSizingProblemDescriptor descriptor =
            LotSizingProblemDescriptor.FromLegacyFeatures(features);

        Assert.True(descriptor.Setup.HasStartUpCosts);
        Assert.True(descriptor.Setup.HasStartUpTimes);

        IReadOnlyList<LotSizingProblemClassExtensionKind> extensions =
            new LotSizingProblemClassExtensionAnalyzer()
                .Analyze(descriptor);

        Assert.Contains(
            LotSizingProblemClassExtensionKind.StartUpCosts,
            extensions);

        Assert.Contains(
            LotSizingProblemClassExtensionKind.StartUpTimes,
            extensions);
    }

    [Fact]
    public void TypedDescriptorLegacyRoundTrip_PreservesBothStartUpFlags()
    {
        var features =
            new LotSizingProblemFeatures
            {
                HasStartUpCosts = true,
                HasStartUpTimes = true
            };

        LotSizingProblemFeatures roundTrip =
            LotSizingProblemDescriptor
                .FromLegacyFeatures(features)
                .ToLegacyFeatures();

        Assert.True(roundTrip.HasStartUpCosts);
        Assert.True(roundTrip.HasStartUpTimes);
    }
}
