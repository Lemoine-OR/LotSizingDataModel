using LotSizingDataModel.Core;
using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Instance.Classification;

namespace LotSizingDataModel.Instance.Tests.Classification;

public sealed class MacroMicroFeatureExtractionTests
{
    [Fact]
    public void Extractor_UsesRealMacroMicroProfileData()
    {
        var count =
            new MicroPeriodCount(
                planningHorizon: 3,
                defaultMicroPeriodCount: 2);

        count.SetCount(2, 4);

        var workCenter =
            new WorkCenter(1, "M1")
            {
                SchedulingProfile =
                    new ProductionSchedulingProfile
                    {
                        BucketMode = SchedulingBucketMode.MacroMicro,
                        MicroPeriodLengthMode = MicroPeriodLengthMode.Variable,
                        MicroPeriodAssignmentMode = MicroPeriodAssignmentMode.SingleItem,
                        MicroPeriodCount = count
                    }
            };

        var plant =
            new Plant(
                1,
                "P1",
                new PlantWarehouse("P1-Warehouse"));

        plant.WorkCenters.Add(workCenter);

        var supplyChain = new SupplyChain(3);
        supplyChain.Plants.Add(plant);

        LotSizingProblemFeatures features =
            LotSizingProblemFeatureExtractor.Extract(supplyChain);

        Assert.Equal(
            MicroPeriodLengthMode.Variable,
            features.MicroPeriodLengthMode);
        Assert.Equal(
            MicroPeriodAssignmentMode.SingleItem,
            features.MicroPeriodAssignmentMode);
        Assert.True(features.HasExplicitMicroPeriodGrid);
        Assert.Equal(8, features.TotalMicroPeriodCount);
        Assert.Equal(4, features.MaximumMicroPeriodCountPerMacroPeriod);
        Assert.True(features.HasVariableMicroPeriodCount);
    }
}
