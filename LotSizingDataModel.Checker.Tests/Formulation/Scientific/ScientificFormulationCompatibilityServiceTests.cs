using LotSizingDataModel.Instance.Classification;
using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Instance.Descriptors;
using LotSizingDataModel.Instance.Scientific;
using LotSizingDataModel.Solver.Formulation;
using LotSizingDataModel.Solver.Formulation.Scientific;

namespace LotSizingDataModel.Checker.Tests.Formulation.Scientific;

public sealed class ScientificFormulationCompatibilityServiceTests
{
    private readonly ScientificClassificationEngine _engine =
        new();

    private readonly ScientificFormulationCompatibilityService _service =
        new();

    [Fact]
    public void StandardFormulation_BasicUls_IsCompatible()
    {
        ScientificClassificationResult classification =
            _engine.Analyze(
                Descriptor());

        ScientificFormulationCompatibilityResult result =
            _service.Assess(
                classification,
                StandardLotSizingFormulation.StandardFormulationId);

        Assert.Equal(
            ScientificFormulationCompatibilityKind.Compatible,
            result.Kind);

        Assert.Empty(result.KnownUnsupportedExtensions);
        Assert.Empty(result.UndeterminedExtensions);
    }

    [Fact]
    public void StandardFormulation_BackloggingExtension_IsVerifiedCompatible()
    {
        LotSizingProblemFeatures features =
            BaseFeatures();

        features.HasBacklogging = true;

        ScientificFormulationCompatibilityResult result =
            _service.Assess(
                _engine.Analyze(
                    LotSizingProblemDescriptor.FromLegacyFeatures(features)),
                StandardLotSizingFormulation.StandardFormulationId);

        Assert.Equal(
            ScientificFormulationCompatibilityKind.Compatible,
            result.Kind);

        Assert.Contains(
            LotSizingDataModel.Instance.ProblemClasses
                .LotSizingProblemClassExtensionKind.Backlogging,
            result.VerifiedSupportedExtensions);
    }

    [Fact]
    public void StandardFormulation_StartUpTimeExtension_IsIncompatible()
    {
        LotSizingProblemFeatures features =
            BaseFeatures();

        features.HasStartUpTimes = true;

        ScientificFormulationCompatibilityResult result =
            _service.Assess(
                _engine.Analyze(
                    LotSizingProblemDescriptor.FromLegacyFeatures(features)),
                StandardLotSizingFormulation.StandardFormulationId);

        Assert.Equal(
            ScientificFormulationCompatibilityKind.Incompatible,
            result.Kind);

        Assert.Contains(
            LotSizingDataModel.Instance.ProblemClasses
                .LotSizingProblemClassExtensionKind.StartUpTimes,
            result.KnownUnsupportedExtensions);

        Assert.Contains(
            result.Diagnostics,
            diagnostic =>
                diagnostic.Code == "LSDM-FORM-011");
    }

    [Fact]
    public void StandardFormulation_UnverifiedLostSales_RemainsUndetermined()
    {
        LotSizingProblemFeatures features =
            BaseFeatures();

        features.HasLostSales = true;

        ScientificFormulationCompatibilityResult result =
            _service.Assess(
                _engine.Analyze(
                    LotSizingProblemDescriptor.FromLegacyFeatures(features)),
                StandardLotSizingFormulation.StandardFormulationId);

        Assert.Equal(
            ScientificFormulationCompatibilityKind.Undetermined,
            result.Kind);

        Assert.Contains(
            LotSizingDataModel.Instance.ProblemClasses
                .LotSizingProblemClassExtensionKind.LostSales,
            result.UndeterminedExtensions);

        Assert.Contains(
            result.Diagnostics,
            diagnostic =>
                diagnostic.Code == "LSDM-FORM-012");
    }

    [Fact]
    public void UnknownFormulationProfile_IsUndetermined()
    {
        ScientificFormulationCompatibilityResult result =
            _service.Assess(
                _engine.Analyze(Descriptor()),
                "custom-formulation");

        Assert.Equal(
            ScientificFormulationCompatibilityKind.Undetermined,
            result.Kind);

        Assert.Contains(
            result.Diagnostics,
            diagnostic =>
                diagnostic.Code == "LSDM-FORM-001");
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
