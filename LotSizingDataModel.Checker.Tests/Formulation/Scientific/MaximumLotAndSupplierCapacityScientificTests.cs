using LotSizingDataModel.Instance.Classification;
using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Instance.Descriptors;
using LotSizingDataModel.Instance.ProblemClasses;
using LotSizingDataModel.Instance.Scientific;
using LotSizingDataModel.Solver.Formulation;
using LotSizingDataModel.Solver.Formulation.Scientific;

namespace LotSizingDataModel.Checker.Tests.Formulation.Scientific;

public sealed class MaximumLotAndSupplierCapacityScientificTests
{
    [Fact]
    public void StandardFactory_RegistersBothConstraintFamilies()
    {
        string[] families =
            StandardLotSizingFormulationFactory
                .CreateConstraintFamilyBuilders()
                .Select(builder => builder.ConstraintFamilyId)
                .ToArray();

        Assert.Contains("maximumLotSize", families);
        Assert.Contains("supplierCapacity", families);
    }

    [Fact]
    public void StandardProfile_VerifiesBothExtensions()
    {
        Assert.True(
            MathematicalFormulationScientificCatalog.Standard
                .IsExtensionVerifiedSupported(
                    LotSizingProblemClassExtensionKind.MaximumLotSize));

        Assert.True(
            MathematicalFormulationScientificCatalog.Standard
                .IsExtensionVerifiedSupported(
                    LotSizingProblemClassExtensionKind.SupplierCapacity));

        Assert.False(
            MathematicalFormulationScientificCatalog.Standard
                .IsExtensionKnownUnsupported(
                    LotSizingProblemClassExtensionKind.MaximumLotSize));

        Assert.False(
            MathematicalFormulationScientificCatalog.Standard
                .IsExtensionKnownUnsupported(
                    LotSizingProblemClassExtensionKind.SupplierCapacity));
    }

    [Fact]
    public void BothExtensionsRemainCompatibleWithStandardFormulation()
    {
        var features =
            new LotSizingProblemFeatures
            {
                ItemCount = 1,
                PlanningHorizon = 4,
                ProductStructureType =
                    ProductStructureType.IndependentItems,
                HasDemand = true,
                HasDeterministicDemand = true,
                HasProduction = true,
                HasSetupCosts = true,
                HasMaximumLotSizes = true,
                HasPurchasing = true,
                HasSupplierCapacityConstraints = true
            };

        ScientificClassificationResult classification =
            new ScientificClassificationEngine()
                .Analyze(
                    LotSizingProblemDescriptor
                        .FromLegacyFeatures(features));

        ScientificFormulationCompatibilityResult result =
            new ScientificFormulationCompatibilityService()
                .Assess(
                    classification,
                    StandardLotSizingFormulation.StandardFormulationId);

        Assert.Equal(
            ScientificFormulationCompatibilityKind.Compatible,
            result.Kind);

        Assert.Contains(
            LotSizingProblemClassExtensionKind.MaximumLotSize,
            result.VerifiedSupportedExtensions);

        Assert.Contains(
            LotSizingProblemClassExtensionKind.SupplierCapacity,
            result.VerifiedSupportedExtensions);
    }
}
