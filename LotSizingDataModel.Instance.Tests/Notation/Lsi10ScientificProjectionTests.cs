using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Instance.Descriptors;
using LotSizingDataModel.Instance.Descriptors.Network;
using LotSizingDataModel.Instance.Notation;
using LotSizingDataModel.Instance.Notation.Lsi;

namespace LotSizingDataModel.Instance.Tests.Notation;

public sealed class Lsi10ScientificProjectionTests
{
    [Fact]
    public void Projector_UsesUniversalNotationAsSemanticSource()
    {
        var descriptor =
            new LotSizingProblemDescriptor
            {
                Structure =
                    new StructureDescriptor
                    {
                        ItemCount = 4,
                        PlantCount = 1,
                        WorkCenterCount = 1,
                        WarehouseCount = 1,
                        ProductStructureRelationshipCount = 3,
                        MaximumProductStructureDepth = 2,
                        ProductStructureType =
                            ProductStructureType.General
                    },
                Time =
                    new TimeDescriptor
                    {
                        PlanningHorizon = 12
                    },
                Demand =
                    new DemandDescriptor
                    {
                        HasDemand = true,
                        IsDeterministic = true,
                        IsTimeVarying = true
                    },
                Production =
                    new ProductionDescriptor
                    {
                        HasProduction = true
                    },
                Capacity =
                    new CapacityDescriptor
                    {
                        HasProductionCapacity = true
                    },
                Setup =
                    new SetupDescriptor
                    {
                        HasSetupCosts = true,
                        HasProductionSetupFamilies = true,
                        HasProductionSetupFamilyTimes = true
                    }
            };

        UniversalLotSizingNotation universal =
            new UniversalNotationGenerator()
                .Generate(descriptor);

        Lsi10Projection projection =
            new Lsi10ScientificProjector()
                .Project(
                    descriptor,
                    universal);

        Assert.StartsWith(
            "LSI/1.0:",
            projection.CanonicalText,
            StringComparison.Ordinal);

        Assert.Contains(
            "SET.FAM=1",
            projection.CanonicalText,
            StringComparison.Ordinal);

        Assert.Contains(
            "SET.FAM.T=1",
            projection.CanonicalText,
            StringComparison.Ordinal);

        Assert.Contains(
            "@ sigma{T=12,I=4",
            projection.CanonicalText,
            StringComparison.Ordinal);

        Assert.Equal(
            "MLCLSP",
            projection.LegacyProblemFamily);

        Assert.Equal(
            universal.Render(),
            projection.UniversalNotationText);
    }

    [Fact]
    public void Projector_MapsSingleItemUncapacitatedToLsU()
    {
        var descriptor =
            new LotSizingProblemDescriptor
            {
                Structure =
                    new StructureDescriptor
                    {
                        ItemCount = 1,
                        ProductStructureRelationshipCount = 0,
                        ProductStructureType =
                            ProductStructureType.IndependentItems
                    },
                Time =
                    new TimeDescriptor
                    {
                        PlanningHorizon = 6
                    },
                Production =
                    new ProductionDescriptor
                    {
                        HasProduction = true
                    }
            };

        UniversalLotSizingNotation universal =
            new UniversalNotationGenerator()
                .Generate(descriptor);

        Lsi10Projection projection =
            new Lsi10ScientificProjector()
                .Project(
                    descriptor,
                    universal);

        Assert.Equal(
            "LS-U",
            projection.LegacyProblemFamily);
    }
}
