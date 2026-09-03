using LotSizingDataModel.Core;
using LotSizingDataModel.Core.DecisionModel.Constraints;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance.Classification;
using LotSizingDataModel.Instance.Classification.Notation;

namespace LotSizingDataModel.Checker.Tests.Classification;

public sealed class LsiPack06OperationalExtensionsTests
{
    [Fact]
    public void MaximumLotSize_IsNonNegativeAndParticipatesInRoutingLifecycle()
    {
        var routing =
            new ProductionRouting
            {
                MaximumLotSize =
                    new MaximumLotSize(3, 25.0)
            };

        Assert.True(routing.HasLotSizingConstraints);
        Assert.Equal(3, routing.PlanningHorizon);
        Assert.Equal(
            25.0,
            routing.MaximumLotSize!.GetMaximumLotSize(2));

        routing.MaximumLotSize.SetMaximumLotSize(2, 0.0);

        Assert.Equal(
            0.0,
            routing.MaximumLotSize.GetMaximumLotSize(2));

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                routing.MaximumLotSize.SetMaximumLotSize(
                    1,
                    -1.0));

        routing.ResizeTimeSeries(5);

        Assert.Equal(
            5,
            routing.MaximumLotSize.PlanningHorizon);

        routing.ClearLotSizingConstraints();

        Assert.Null(routing.MaximumLotSize);
    }

    [Fact]
    public void SupplierCapacity_ParticipatesInDeliveryLifecycle()
    {
        var delivery =
            new SupplierDelivery
            {
                CapacityConstraint =
                    new CapacityConstraint(3, 40.0)
            };

        Assert.True(delivery.HasDecisionParameters);
        Assert.True(delivery.HasConsistentPlanningHorizon);
        Assert.Equal(3, delivery.PlanningHorizon);

        delivery.ResizeTimeSeries(5);

        Assert.Equal(
            5,
            delivery.CapacityConstraint!.PlanningHorizon);

        delivery.ClearDecisionParameters();

        Assert.Null(delivery.CapacityConstraint);
    }

    [Fact]
    public void FeatureExtractor_DetectsMaximumLotAndSupplierCapacity()
    {
        var chain = new SupplyChain
        {
            PlanningHorizon = 3
        };

        chain.ProductionRoutings.Add(
            new ProductionRouting
            {
                MaximumLotSize =
                    new MaximumLotSize(3, 12.0)
            });

        chain.SupplierDeliveries.Add(
            new SupplierDelivery
            {
                CapacityConstraint =
                    new CapacityConstraint(3, 40.0)
            });

        LotSizingProblemFeatures features =
            LotSizingProblemFeatureExtractor.Extract(chain);

        Assert.True(features.HasMaximumLotSizes);
        Assert.True(features.HasSupplierCapacityConstraints);

        LotSizingInstanceSignature signature =
            LotSizingInstanceSignatureExtractor.Extract(chain);

        Assert.Equal(
            FeatureState.Present,
            signature.Features
                .Find(LsiFeatureCodes.MaximumLotSize)!
                .State);

        Assert.Equal(
            FeatureState.Present,
            signature.Features
                .Find(LsiFeatureCodes.SupplierCapacity)!
                .State);
    }
}
