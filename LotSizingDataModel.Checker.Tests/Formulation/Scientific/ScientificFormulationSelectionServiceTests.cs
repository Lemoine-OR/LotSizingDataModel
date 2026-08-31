using LotSizingDataModel.Instance.Classification;
using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Instance.Descriptors;
using LotSizingDataModel.Instance.Scientific;
using LotSizingDataModel.Solver.Formulation;
using LotSizingDataModel.Solver.Formulation.Scientific;

namespace LotSizingDataModel.Checker.Tests.Formulation.Scientific;

public sealed class ScientificFormulationSelectionServiceTests
{
    private readonly ScientificClassificationEngine _engine =
        new();

    [Fact]
    public void ScientificPreselection_SelectsStandardForVerifiedCore()
    {
        var registry =
            new MathematicalModelFormulationRegistry();

        registry.Register(
            StandardLotSizingFormulationFactory.CreateDefault());

        ScientificClassificationResult classification =
            _engine.Analyze(
                Descriptor());

        ScientificFormulationSelectionResult result =
            new ScientificFormulationSelectionService()
                .Select(
                    classification,
                    registry);

        Assert.True(result.IsSuccessful);

        Assert.Equal(
            StandardLotSizingFormulation.StandardFormulationId,
            result.Formulation!.FormulationId);

        Assert.False(result.UsedFallback);
    }

    [Fact]
    public void ScientificPreselection_DoesNotAutoSelectUndeterminedCandidate()
    {
        LotSizingProblemFeatures features =
            BaseFeatures();

        features.HasLostSales = true;

        var registry =
            new MathematicalModelFormulationRegistry();

        registry.Register(
            StandardLotSizingFormulationFactory.CreateDefault());

        ScientificFormulationSelectionResult result =
            new ScientificFormulationSelectionService()
                .Select(
                    _engine.Analyze(
                        LotSizingProblemDescriptor.FromLegacyFeatures(
                            features)),
                    registry);

        Assert.False(result.IsSuccessful);

        Assert.Null(result.Formulation);

        Assert.Contains(
            result.Diagnostics,
            diagnostic =>
                diagnostic.Code == "LSDM-FORM-033");
    }

    [Fact]
    public void RequestedIncompatibleFormulation_NoFallbackFails()
    {
        LotSizingProblemFeatures features =
            BaseFeatures();

        features.HasStartUpTimes = true;

        var registry =
            new MathematicalModelFormulationRegistry();

        registry.Register(
            StandardLotSizingFormulationFactory.CreateDefault());

        ScientificFormulationSelectionResult result =
            new ScientificFormulationSelectionService()
                .Select(
                    _engine.Analyze(
                        LotSizingProblemDescriptor.FromLegacyFeatures(
                            features)),
                    registry,
                    requestedFormulationId:
                        StandardLotSizingFormulation.StandardFormulationId,
                    allowFallback:
                        false);

        Assert.False(result.IsSuccessful);

        Assert.Contains(
            result.Diagnostics,
            diagnostic =>
                diagnostic.Code == "LSDM-FORM-032");
    }

    private static LotSizingProblemDescriptor Descriptor() =>
        LotSizingProblemDescriptor.FromLegacyFeatures(
            BaseFeatures());

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
