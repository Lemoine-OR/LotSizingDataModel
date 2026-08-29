using LotSizingDataModel.Instance.Classification;
using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Instance.Descriptors;
using LotSizingDataModel.Instance.Scientific;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solution.Metadata.Scientific;
using LotSizingDataModel.Solver.Formulation;
using LotSizingDataModel.Solver.Formulation.Scientific;
using LotSizingDataModel.Solver.Mapping.Scientific;

namespace LotSizingDataModel.Checker.Tests.ScientificProvenance;

public sealed class ScientificSolutionProvenanceMapperTests
{
    [Fact]
    public void Mapper_CapturesDetectedClassAndSelectedFormulation()
    {
        ScientificClassificationResult classification =
            new ScientificClassificationEngine()
                .Analyze(
                    LotSizingProblemDescriptor.FromLegacyFeatures(
                        BaseFeatures()));

        var registry =
            new MathematicalModelFormulationRegistry();

        registry.Register(
            StandardLotSizingFormulationFactory.CreateDefault());

        ScientificFormulationSelectionResult selection =
            new ScientificFormulationSelectionService()
                .Select(
                    classification,
                    registry);

        var solution =
            new LotSizingSolution(
                planningHorizon: 6);

        SolutionScientificProvenance provenance =
            ScientificSolutionProvenanceMapper.Apply(
                solution,
                selection,
                new DateTime(
                    2026,
                    8,
                    29,
                    17,
                    30,
                    0,
                    DateTimeKind.Utc));

        Assert.Equal(
            "LSDM",
            provenance.NotationSchemeId);

        Assert.Equal(
            "SI-ULS",
            provenance.CanonicalProblemClassCode);

        Assert.Equal(
            "standard",
            provenance.FormulationId);

        Assert.Equal(
            "Compatible",
            provenance.FormulationScientificCompatibility);

        Assert.Equal(
            SolutionScientificProvenanceReadKind.Valid,
            SolutionScientificProvenanceCodec
                .Read(solution.GenerationMetadata)
                .Kind);
    }

    private static LotSizingProblemFeatures BaseFeatures() =>
        new()
        {
            ItemCount = 1,
            PlanningHorizon = 6,
            ProductStructureRelationshipCount = 0,
            ProductStructureType =
                ProductStructureType.IndependentItems,
            HasDemand = true,
            HasDeterministicDemand = true,
            HasProduction = true,
            HasSetupCosts = true
        };
}
