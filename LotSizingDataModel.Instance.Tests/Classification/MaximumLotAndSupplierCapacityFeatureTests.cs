using LotSizingDataModel.Core;
using LotSizingDataModel.Core.DecisionModel.Constraints;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance.Classification;
using LotSizingDataModel.Instance.Descriptors;

namespace LotSizingDataModel.Instance.Tests.Classification;

public sealed class MaximumLotAndSupplierCapacityFeatureTests
{
    [Fact]
    public void Extractor_DetectsRealCoreParameters()
    {
        var chain =
            new SupplyChain
            {
                PlanningHorizon = 3
            };

        chain.ProductionRoutings.Add(
            new ProductionRouting
            {
                MaximumLotSize =
                    new MaximumLotSize(3, 20.0)
            });

        chain.SupplierDeliveries.Add(
            new SupplierDelivery
            {
                CapacityConstraint =
                    new CapacityConstraint(3, 50.0)
            });

        LotSizingProblemFeatures features =
            LotSizingProblemFeatureExtractor.Extract(chain);

        Assert.True(features.HasMaximumLotSizes);
        Assert.True(features.HasSupplierCapacityConstraints);

        LotSizingProblemDescriptor descriptor =
            LotSizingProblemDescriptor.FromLegacyFeatures(features);

        Assert.True(descriptor.Production.HasMaximumLotSizes);
        Assert.True(descriptor.Capacity.HasSupplierCapacity);

        LotSizingProblemFeatures roundTrip =
            descriptor.ToLegacyFeatures();

        Assert.True(roundTrip.HasMaximumLotSizes);
        Assert.True(roundTrip.HasSupplierCapacityConstraints);
    }
}
