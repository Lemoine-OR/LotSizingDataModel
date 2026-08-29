using LotSizingDataModel.Instance.Classification;
using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Instance.Descriptors;
using LotSizingDataModel.Instance.ProblemClasses;
using LotSizingDataModel.Instance.Scientific;

namespace LotSizingDataModel.Instance.Tests.Scientific;

public sealed class ScientificClassificationEngineTests
{
    private readonly ScientificClassificationEngine _engine =
        new();

    [Fact]
    public void AnalyzeDescriptor_ProducesDetectedNotationAndCanonicalClass()
    {
        LotSizingProblemDescriptor descriptor =
            CreateSingleItemUlsDescriptor();

        ScientificClassificationResult result =
            _engine.Analyze(descriptor);

        Assert.False(result.IsBlocked);

        Assert.Equal(
            "1,SL,Net:UNK | " +
            "Dem,Det,DVar,Prod,Uncap:P,SC | Obj:Econ",
            result.DetectedNotationText);

        Assert.NotNull(
            result.PrimaryProblemClass);

        Assert.Equal(
            CanonicalLotSizingProblemClassId
                .SingleItemUncapacitatedLotSizing,
            result.PrimaryProblemClass!.Definition.Id);

        Assert.Equal(
            LotSizingProblemClassMatchKind.ExactCore,
            result.PrimaryProblemClass.Kind);

        Assert.Equal(
            ScientificNotationComparisonKind.NotDeclared,
            result.NotationComparison.Kind);
    }

    [Fact]
    public void DeclaredNotation_ExactIsKeptSeparateFromDetectedNotation()
    {
        LotSizingProblemDescriptor descriptor =
            CreateSingleItemUlsDescriptor();

        const string declared =
            "1,SL,Net:UNK | " +
            "Dem,Det,DVar,Prod,Uncap:P,SC | Obj:Econ";

        ScientificClassificationResult result =
            _engine.Analyze(
                descriptor,
                new ScientificClassificationRequest(
                    declaredNotation: declared));

        Assert.Equal(
            ScientificNotationComparisonKind.Exact,
            result.NotationComparison.Kind);

        Assert.Equal(
            declared,
            result.DeclaredNotationText);

        Assert.Equal(
            declared,
            result.DetectedNotationText);

        Assert.NotSame(
            result.NotationComparison.DeclaredSpecification,
            result.DetectedNotation);
    }

    [Fact]
    public void LessSpecificDeclaredNotation_IsCompatible()
    {
        ScientificClassificationResult result =
            _engine.Analyze(
                CreateSingleItemUlsDescriptor(),
                new ScientificClassificationRequest(
                    declaredNotation:
                        "1,SL,Net:UNK | " +
                        "Dem,Prod,Uncap:P,SC | Obj:Econ"));

        Assert.Equal(
            ScientificNotationComparisonKind.Compatible,
            result.NotationComparison.Kind);

        Assert.Contains(
            result.Diagnostics,
            diagnostic =>
                diagnostic.Code == "LSDM-SCI-011");
    }

    [Fact]
    public void ContradictoryDeclaredNotation_IsReportedAsError()
    {
        ScientificClassificationResult result =
            _engine.Analyze(
                CreateSingleItemUlsDescriptor(),
                new ScientificClassificationRequest(
                    declaredNotation:
                        "1,SL,Net:UNK | " +
                        "Dem,Det,Prod,Cap:P,SC | Obj:Econ"));

        Assert.Equal(
            ScientificNotationComparisonKind.Contradiction,
            result.NotationComparison.Kind);

        Assert.True(
            result.HasDeclaredNotationConflict);

        Assert.True(
            result.HasErrors);

        Assert.Contains(
            result.Diagnostics,
            diagnostic =>
                diagnostic.Code == "LSDM-SCI-013");
    }

    [Fact]
    public void InvalidDeclaredNotation_DoesNotDestroyDetectedClassification()
    {
        ScientificClassificationResult result =
            _engine.Analyze(
                CreateSingleItemUlsDescriptor(),
                new ScientificClassificationRequest(
                    declaredNotation:
                        "this is not universal notation"));

        Assert.Equal(
            ScientificNotationComparisonKind.InvalidDeclaredNotation,
            result.NotationComparison.Kind);

        Assert.NotNull(
            result.DetectedNotation);

        Assert.NotNull(
            result.PrimaryProblemClass);

        Assert.Contains(
            result.Diagnostics,
            diagnostic =>
                diagnostic.Code == "LSDM-SCI-010");
    }

    [Fact]
    public void IndustrialFeatures_AreReturnedAsProblemClassExtensions()
    {
        LotSizingProblemFeatures features =
            CreateSingleItemUlsFeatures();

        features.HasBacklogging = true;
        features.HasSafetyStockRequirements = true;

        ScientificClassificationResult result =
            _engine.Analyze(
                LotSizingProblemDescriptor.FromLegacyFeatures(
                    features));

        Assert.NotNull(
            result.PrimaryProblemClass);

        Assert.Equal(
            LotSizingProblemClassMatchKind.CompatibleExtension,
            result.PrimaryProblemClass!.Kind);

        Assert.Contains(
            LotSizingProblemClassExtensionKind.Backlogging,
            result.PrimaryProblemClass.Extensions);

        Assert.Contains(
            LotSizingProblemClassExtensionKind.SafetyStock,
            result.PrimaryProblemClass.Extensions);
    }

    [Fact]
    public void CapacitatedSingleItem_ReportsBitranYanasseProfileRequirement()
    {
        LotSizingProblemFeatures features =
            CreateSingleItemUlsFeatures();

        features.HasProductionCapacityConstraints = true;

        ScientificClassificationResult result =
            _engine.Analyze(
                LotSizingProblemDescriptor.FromLegacyFeatures(
                    features));

        HistoricalClassificationCapability by =
            Assert.Single(
                result.HistoricalCapabilities,
                capability =>
                    capability.Code == "BY");

        Assert.Equal(
            HistoricalClassificationCapabilityKind
                .ApplicableNeedsExplicitProfile,
            by.Kind);

        Assert.True(
            by.CanMapDeclaredClassification);

        Assert.False(
            by.CanDetectCompleteHistoricalCode);

        Assert.True(
            by.RequiresExplicitParameterProfile);
    }

    [Fact]
    public void WolseyCapability_DoesNotClaimAutomaticHistoricalDetection()
    {
        ScientificClassificationResult result =
            _engine.Analyze(
                CreateSingleItemUlsDescriptor());

        HistoricalClassificationCapability wolsey =
            Assert.Single(
                result.HistoricalCapabilities,
                capability =>
                    capability.Code == "WOLSEY-2002");

        Assert.Equal(
            HistoricalClassificationCapabilityKind.MappingSupportedOnly,
            wolsey.Kind);

        Assert.True(
            wolsey.CanMapDeclaredClassification);

        Assert.False(
            wolsey.CanDetectCompleteHistoricalCode);
    }

    [Fact]
    public void Coverage_ExplicitlyDoesNotInferDrpMrpFormulationsOrMethods()
    {
        ScientificClassificationCoverage coverage =
            _engine.Analyze(
                    CreateSingleItemUlsDescriptor())
                .Coverage;

        Assert.Equal(
            ScientificClassificationAxisStatus.Analyzed,
            coverage.GetStatus(
                ScientificClassificationAxis.ProblemClasses));

        Assert.Equal(
            ScientificClassificationAxisStatus.NotInferred,
            coverage.GetStatus(
                ScientificClassificationAxis.PlanningParadigms));

        Assert.Equal(
            ScientificClassificationAxisStatus.NotInferred,
            coverage.GetStatus(
                ScientificClassificationAxis.MathematicalFormulations));

        Assert.Equal(
            ScientificClassificationAxisStatus.NotInferred,
            coverage.GetStatus(
                ScientificClassificationAxis.SolutionMethods));

        Assert.False(
            coverage.InfersPlanningParadigms);
    }

    [Fact]
    public void MissingSetupCost_ProducesNoExecutableCanonicalClass()
    {
        LotSizingProblemFeatures features =
            CreateSingleItemUlsFeatures();

        features.HasSetupCosts = false;

        ScientificClassificationResult result =
            _engine.Analyze(
                LotSizingProblemDescriptor.FromLegacyFeatures(
                    features));

        Assert.Null(
            result.PrimaryProblemClass);

        Assert.Empty(
            result.ProblemClassMatches);

        Assert.Contains(
            result.Diagnostics,
            diagnostic =>
                diagnostic.Code == "LSDM-SCI-020");
    }

    private static LotSizingProblemDescriptor
        CreateSingleItemUlsDescriptor() =>
            LotSizingProblemDescriptor.FromLegacyFeatures(
                CreateSingleItemUlsFeatures());

    private static LotSizingProblemFeatures
        CreateSingleItemUlsFeatures() =>
            new()
            {
                ItemCount = 1,
                PlanningHorizon = 6,
                ProductStructureRelationshipCount = 0,
                ProductStructureType =
                    ProductStructureType.IndependentItems,
                HasDemand = true,
                HasDeterministicDemand = true,
                HasTimeVaryingDemand = true,
                HasProduction = true,
                HasSetupCosts = true
            };
}
