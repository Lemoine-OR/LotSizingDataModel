using LotSizingDataModel.Core;
using LotSizingDataModel.Core.LogicalModel;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Instance.Classification;
using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Instance.Descriptors;
using LotSizingDataModel.Instance.Notation;

namespace LotSizingDataModel.Instance.Tests.Notation;

public sealed class UniversalNotationTests
{
    [Fact]
    public void SchemeVersion_IsExplicitAndStable()
    {
        Assert.Equal("LSDM", UniversalNotationScheme.Id);
        Assert.Equal("1", UniversalNotationScheme.CurrentVersion);
        Assert.True(UniversalNotationScheme.IsSupported("1"));
        Assert.False(UniversalNotationScheme.IsSupported("2"));
    }

    [Fact]
    public void Generate_SimpleDescriptor_ProducesReadableThreeFieldNotation()
    {
        var features =
            new LotSizingProblemFeatures
            {
                ItemCount = 1,
                PlanningHorizon = 6,
                ProductStructureType =
                    ProductStructureType.IndependentItems,
                HasDemand = true,
                HasDeterministicDemand = true,
                HasProduction = true,
                HasSetupCosts = true
            };

        var supplyChain =
            new SupplyChain(planningHorizon: 6);

        supplyChain.AddItem(
            new Item(
                id: 1,
                name: "Item",
                billOfMaterialsLevel: 0));

        supplyChain.AddStandaloneWarehouse(
            new StandaloneWarehouse(
                id: 1,
                name: "Warehouse"));

        LotSizingProblemDescriptor descriptor =
            LotSizingProblemDescriptor.FromLegacyFeatures(
                features,
                supplyChain);

        string notation =
            new UniversalNotationGenerator()
                .Generate(descriptor)
                .Render();

        Assert.Equal(
            "1,SL,Net:IND:E1 | Dem,Det,Prod,Uncap:P,SC | Obj:Econ",
            notation);
    }

    [Fact]
    public void Generate_RepresentativeDescriptor_UsesCanonicalBetaOrdering()
    {
        var features =
            new LotSizingProblemFeatures
            {
                ItemCount = 4,
                PlanningHorizon = 12,
                ProductStructureRelationshipCount = 3,
                ProductStructureType =
                    ProductStructureType.General,
                HasDemand = true,
                HasDeterministicDemand = true,
                HasTimeVaryingDemand = true,
                HasProduction = true,
                HasProductionCapacityConstraints = true,
                HasSharedProductionCapacity = true,
                HasTimeVaryingProductionCapacity = true,
                HasSetupCosts = true,
                HasSetupTimes = true,
                HasMinimumLotSizes = true,
                HasBacklogging = true,
                HasPurchasing = true,
                HasTransportation = true,
                HasDistribution = true,
                HasFinancialConstraints = true
            };

        var descriptor =
            LotSizingProblemDescriptor.FromLegacyFeatures(
                features);

        string notation =
            new UniversalNotationGenerator()
                .Generate(descriptor)
                .Render();

        Assert.StartsWith(
            "m,ML:GEN,Net:UNK",
            notation);

        Assert.Contains(
            "Dem,Det,DVar,Prod,Cap:P,Cap:Shared,Cap:Var,SC,ST,MinLot,BL,Buy,Tr,Dist,Fin",
            notation);

        Assert.EndsWith(
            "| Obj:Econ",
            notation);
    }

    [Fact]
    public void GenerateParseRender_RoundTrip_IsExact()
    {
        var features =
            new LotSizingProblemFeatures
            {
                ItemCount = 2,
                PlanningHorizon = 4,
                ProductStructureRelationshipCount = 1,
                ProductStructureType =
                    ProductStructureType.Serial,
                HasDemand = true,
                HasProduction = true,
                HasProductionCapacityConstraints = true,
                HasSetupCosts = true,
                HasLotSizeMultiples = true
            };

        LotSizingProblemDescriptor descriptor =
            LotSizingProblemDescriptor.FromLegacyFeatures(
                features);

        string generated =
            new UniversalNotationGenerator()
                .Generate(descriptor)
                .Render();

        UniversalLotSizingNotation parsed =
            new UniversalNotationParser()
                .Parse(generated);

        Assert.Equal(
            generated,
            parsed.Render());

        Assert.Equal(
            UniversalNotationScheme.CurrentVersion,
            parsed.SchemeVersion);
    }

    [Fact]
    public void Parser_CanonicalizesWhitespaceBetaOrderAndNetworkModifiers()
    {
        string raw =
            " m , ML:GEN , Net:DIV:TS:E3:MS " +
            " | SC , Dem , Prod , Det " +
            " | Obj:Econ ";

        string canonical =
            new UniversalNotationParser()
                .Canonicalize(raw);

        Assert.Equal(
            "m,ML:GEN,Net:DIV:E3:MS:TS | Dem,Det,Prod,SC | Obj:Econ",
            canonical);
    }

    [Fact]
    public void Parser_ParsesReservedClosedLoopNetworkShape()
    {
        UniversalLotSizingNotation parsed =
            new UniversalNotationParser()
                .Parse(
                    "m,SL,Net:CL(F:DIV;R:CONV):E3:MS " +
                    "| Dem,Tr | Obj:Econ");

        Assert.Equal(
            "m,SL,Net:CL(F:DIV;R:CONV):E3:MS | Dem,Tr | Obj:Econ",
            parsed.Render());
    }

    [Fact]
    public void Parser_RejectsWrongNumberOfFields()
    {
        Assert.Throws<FormatException>(
            () =>
                new UniversalNotationParser()
                    .Parse(
                        "1,SL,Net:IND | Dem,Det"));
    }

    [Fact]
    public void Parser_RejectsUnknownBetaToken()
    {
        Assert.Throws<FormatException>(
            () =>
                new UniversalNotationParser()
                    .Parse(
                        "1,SL,Net:IND | Magic | Obj:Econ"));
    }

    [Fact]
    public void MultipleObjectiveDescriptor_OverridesEconomicDefault()
    {
        var features =
            new LotSizingProblemFeatures
            {
                ItemCount = 1,
                PlanningHorizon = 2,
                ProductStructureType =
                    ProductStructureType.IndependentItems,
                HasMultipleObjectives = true
            };

        LotSizingProblemDescriptor descriptor =
            LotSizingProblemDescriptor.FromLegacyFeatures(
                features);

        string notation =
            new UniversalNotationGenerator()
                .Generate(descriptor)
                .Render();

        Assert.EndsWith(
            "| Obj:Multi",
            notation);
    }
}
