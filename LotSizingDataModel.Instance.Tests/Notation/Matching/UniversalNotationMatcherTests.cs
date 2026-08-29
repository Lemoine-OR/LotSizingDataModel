using LotSizingDataModel.Core;
using LotSizingDataModel.Core.LogicalModel;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance.Classification;
using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Instance.Descriptors;
using LotSizingDataModel.Instance.Notation;
using LotSizingDataModel.Instance.Notation.Matching;

namespace LotSizingDataModel.Instance.Tests.Notation.Matching;

public sealed class UniversalNotationMatcherTests
{
    [Fact]
    public void Match_GeneratedCanonicalNotation_IsExact()
    {
        LotSizingProblemDescriptor descriptor =
            CreateRepresentativeDescriptor();

        string generated =
            new UniversalNotationGenerator()
                .Generate(descriptor)
                .Render();

        UniversalNotationMatchResult result =
            new UniversalNotationMatcher()
                .Match(
                    descriptor,
                    generated);

        Assert.Equal(
            UniversalNotationMatchKind.Exact,
            result.Kind);

        Assert.Empty(result.Issues);
        Assert.True(result.IsExact);
        Assert.True(result.IsCompatible);
    }

    [Fact]
    public void Match_LessSpecificPositiveSpecification_IsCompatible()
    {
        LotSizingProblemDescriptor descriptor =
            CreateRepresentativeDescriptor();

        UniversalNotationMatchResult result =
            new UniversalNotationMatcher()
                .Match(
                    descriptor,
                    "m,ML:GEN,Net:DIV | Dem,Prod | Obj:Econ");

        Assert.Equal(
            UniversalNotationMatchKind.Compatible,
            result.Kind);

        Assert.Empty(result.Issues);
        Assert.False(result.IsExact);
        Assert.True(result.IsCompatible);
    }

    [Fact]
    public void Match_RequiredAbsentBetaFeature_IsContradiction()
    {
        LotSizingProblemDescriptor descriptor =
            CreateRepresentativeDescriptor();

        UniversalNotationMatchResult result =
            new UniversalNotationMatcher()
                .Match(
                    descriptor,
                    "m,ML:GEN,Net:DIV | Dem,Prod,LS | Obj:Econ");

        Assert.Equal(
            UniversalNotationMatchKind.Contradiction,
            result.Kind);

        Assert.Contains(
            result.Issues,
            issue =>
                issue.Code == "LSDM-MATCH-030" &&
                issue.IsContradiction);
    }

    [Fact]
    public void Match_KnownTopologyMismatch_IsContradiction()
    {
        LotSizingProblemDescriptor descriptor =
            CreateRepresentativeDescriptor();

        UniversalNotationMatchResult result =
            new UniversalNotationMatcher()
                .Match(
                    descriptor,
                    "m,ML:GEN,Net:CONV | Dem,Prod | Obj:Econ");

        Assert.Equal(
            UniversalNotationMatchKind.Contradiction,
            result.Kind);

        Assert.Contains(
            result.Issues,
            issue =>
                issue.Code == "LSDM-MATCH-021");
    }

    [Fact]
    public void Match_UnknownActualTopologyAgainstRequiredKnownTopology_IsIncomplete()
    {
        var features =
            new LotSizingProblemFeatures
            {
                ItemCount = 1,
                PlanningHorizon = 3,
                ProductStructureType =
                    ProductStructureType.IndependentItems,
                HasDemand = true
            };

        LotSizingProblemDescriptor descriptor =
            LotSizingProblemDescriptor.FromLegacyFeatures(
                features);

        UniversalNotationMatchResult result =
            new UniversalNotationMatcher()
                .Match(
                    descriptor,
                    "1,SL,Net:DIV | Dem | Obj:Econ");

        Assert.Equal(
            UniversalNotationMatchKind.Incomplete,
            result.Kind);

        Assert.Contains(
            result.Issues,
            issue =>
                issue.Code == "LSDM-MATCH-022" &&
                !issue.IsContradiction);
    }

    [Fact]
    public void Match_UnknownSpecificationValuesActAsWildcards()
    {
        var features =
            new LotSizingProblemFeatures
            {
                ItemCount = 1,
                PlanningHorizon = 3,
                ProductStructureType =
                    ProductStructureType.IndependentItems,
                HasDemand = true
            };

        LotSizingProblemDescriptor descriptor =
            LotSizingProblemDescriptor.FromLegacyFeatures(
                features);

        UniversalNotationMatchResult result =
            new UniversalNotationMatcher()
                .Match(
                    descriptor,
                    "?,Level:?,Net:UNK | Dem | Obj:?");

        Assert.Equal(
            UniversalNotationMatchKind.Compatible,
            result.Kind);

        Assert.Empty(result.Issues);
    }

    [Fact]
    public void Match_RequiredNetworkModifierIsPositiveConstraint()
    {
        LotSizingProblemDescriptor descriptor =
            CreateRepresentativeDescriptor();

        UniversalNotationMatchResult result =
            new UniversalNotationMatcher()
                .Match(
                    descriptor,
                    "m,ML:GEN,Net:DIV:E2:MS | Dem,Prod | Obj:Econ");

        Assert.Equal(
            UniversalNotationMatchKind.Contradiction,
            result.Kind);

        Assert.Contains(
            result.Issues,
            issue =>
                issue.Code == "LSDM-MATCH-026");
    }

    [Fact]
    public void Match_OmittedNetworkModifierDoesNotNegateRicherInstance()
    {
        LotSizingProblemDescriptor descriptor =
            CreateRepresentativeDescriptor();

        UniversalNotationMatchResult result =
            new UniversalNotationMatcher()
                .Match(
                    descriptor,
                    "m,ML:GEN,Net:DIV:E2 | Dem,Prod | Obj:Econ");

        Assert.Equal(
            UniversalNotationMatchKind.Compatible,
            result.Kind);

        Assert.Empty(result.Issues);
    }

    [Fact]
    public void Match_ObjectiveContradictionHasHighestPrecedence()
    {
        LotSizingProblemDescriptor descriptor =
            CreateRepresentativeDescriptor();

        UniversalNotationMatchResult result =
            new UniversalNotationMatcher()
                .Match(
                    descriptor,
                    "m,ML:GEN,Net:DIV | Dem,Prod | Obj:Multi");

        Assert.Equal(
            UniversalNotationMatchKind.Contradiction,
            result.Kind);

        Assert.Contains(
            result.Issues,
            issue =>
                issue.Code == "LSDM-MATCH-040");
    }

    [Fact]
    public void ProblemSpecification_StoresCanonicalNotation()
    {
        UniversalProblemSpecification specification =
            UniversalProblemSpecification.Parse(
                " m , ML:GEN , Net:DIV | Prod , Dem | Obj:Econ ");

        Assert.Equal(
            "m,ML:GEN,Net:DIV | Dem,Prod | Obj:Econ",
            specification.CanonicalText);
    }

    private static LotSizingProblemDescriptor
        CreateRepresentativeDescriptor()
    {
        var features =
            new LotSizingProblemFeatures
            {
                ItemCount = 3,
                PlanningHorizon = 6,
                ProductStructureRelationshipCount = 2,
                ProductStructureType =
                    ProductStructureType.General,
                HasDemand = true,
                HasDeterministicDemand = true,
                HasProduction = true,
                HasProductionCapacityConstraints = true,
                HasSetupCosts = true,
                HasBacklogging = true
            };

        var supplyChain =
            new SupplyChain(planningHorizon: 6);

        supplyChain.AddItem(
            new Item(
                id: 1,
                name: "Item 1",
                billOfMaterialsLevel: 0));

        supplyChain.AddStandaloneWarehouse(
            new StandaloneWarehouse(
                id: 1,
                name: "Central"));

        supplyChain.AddStandaloneWarehouse(
            new StandaloneWarehouse(
                id: 2,
                name: "Regional A"));

        supplyChain.AddStandaloneWarehouse(
            new StandaloneWarehouse(
                id: 3,
                name: "Regional B"));

        var transport =
            new TransportResource(
                id: 1,
                name: "Transport");

        transport.AddLane(
            new TransportLane(
                WarehouseReference.ForStandaloneWarehouse(1),
                WarehouseReference.ForStandaloneWarehouse(2),
                0));

        transport.AddLane(
            new TransportLane(
                WarehouseReference.ForStandaloneWarehouse(1),
                WarehouseReference.ForStandaloneWarehouse(3),
                0));

        supplyChain.AddTransportResource(
            transport);

        return LotSizingProblemDescriptor.FromLegacyFeatures(
            features,
            supplyChain);
    }
}
