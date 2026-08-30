using LotSizingDataModel.Core;
using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Instance.Classification;
using LotSizingDataModel.Instance.Descriptors;

namespace LotSizingDataModel.Instance.Tests.Classification;

public sealed class SchedulingFeatureExtractionTests
{
    [Fact]
    public void Extractor_DetectsWorkCenterSchedulingSemantics()
    {
        var plant =
            new Plant(
                id: 1,
                name: "P1",
                warehouse:
                    new PlantWarehouse("P1-Warehouse"));

        plant.WorkCenters.Add(
            new WorkCenter(
                id: 1,
                name: "M1")
            {
                SchedulingProfile =
                    CreateProfile()
            });

        var chain =
            new SupplyChain
            {
                PlanningHorizon = 3
            };

        chain.Plants.Add(plant);

        LotSizingProblemFeatures features =
            LotSizingProblemFeatureExtractor.Extract(chain);

        Assert.True(features.HasIntegratedScheduling);
        Assert.Equal(
            SchedulingBucketMode.MacroMicro,
            features.SchedulingBucketMode);
        Assert.True(features.HasInitialSetupState);
        Assert.True(features.HasSetupCarryOver);
        Assert.True(features.HasSequenceDependentChangeoverTimes);
        Assert.True(features.HasSequenceDependentChangeoverCosts);
        Assert.True(features.HasMaximumSetupCountConstraints);

        LotSizingProblemFeatures roundTrip =
            LotSizingProblemDescriptor
                .FromLegacyFeatures(features)
                .ToLegacyFeatures();

        Assert.Equal(
            features.SchedulingBucketMode,
            roundTrip.SchedulingBucketMode);

        Assert.Equal(
            features.HasSequenceDependentChangeoverTimes,
            roundTrip.HasSequenceDependentChangeoverTimes);
    }

    private static ProductionSchedulingProfile CreateProfile()
    {
        var profile =
            new ProductionSchedulingProfile
            {
                BucketMode =
                    SchedulingBucketMode.MacroMicro,
                SetupCarryOverPolicy =
                    SetupCarryOverPolicy.Allowed,
                InitialSetupItemId = 1,
                MicroPeriodCount =
                    new MicroPeriodCount(3, 2),
                MaximumSetupCount =
                    new MaximumSetupCount(3, 1)
            };

        profile.Changeovers.Add(
            new ProductionChangeover
            {
                FromItemId = 1,
                ToItemId = 2,
                ChangeoverTime =
                    new SequenceDependentChangeoverTime(3, 0.25),
                ChangeoverCost =
                    new SequenceDependentChangeoverCost(3, 4.0)
            });

        return profile;
    }
}
