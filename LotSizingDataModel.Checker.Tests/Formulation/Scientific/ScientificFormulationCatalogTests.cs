using LotSizingDataModel.Instance.ProblemClasses;
using LotSizingDataModel.Solver.Formulation;
using LotSizingDataModel.Solver.Formulation.Scientific;

namespace LotSizingDataModel.Checker.Tests.Formulation.Scientific;

public sealed class ScientificFormulationCatalogTests
{
    [Fact]
    public void StandardProfile_IsBoundToActualStandardFormulationId()
    {
        Assert.Equal(
            StandardLotSizingFormulation.StandardFormulationId,
            MathematicalFormulationScientificCatalog.Standard.FormulationId);
    }

    [Fact]
    public void StandardProfile_SupportsSixGenericLotSizingCoreClassesOnly()
    {
        Assert.Equal(
            6,
            MathematicalFormulationScientificCatalog.Standard
                .SupportedProblemClasses.Count);

        Assert.All(
            new[]
            {
                LotSizingProblemClassCatalog.SingleItemUncapacitated,
                LotSizingProblemClassCatalog.SingleItemCapacitated,
                LotSizingProblemClassCatalog.MultiItemUncapacitated,
                LotSizingProblemClassCatalog.MultiItemCapacitated,
                LotSizingProblemClassCatalog.UncapacitatedMultiLevel,
                LotSizingProblemClassCatalog.MultiLevelCapacitated
            },
            definition =>
                Assert.True(
                    MathematicalFormulationScientificCatalog.Standard
                        .SupportsProblemClass(definition.Id)));

        Assert.False(
            MathematicalFormulationScientificCatalog.Standard
                .SupportsProblemClass(
                    CanonicalLotSizingProblemClassId
                        .DiscreteLotSizingAndScheduling));

        Assert.False(
            MathematicalFormulationScientificCatalog.Standard
                .SupportsProblemClass(
                    CanonicalLotSizingProblemClassId
                        .ContinuousSetupLotSizing));
    }

    [Fact]
    public void DedicatedSmallBucketProfiles_CoverDlspAndCslp()
    {
        Assert.Equal(
            3,
            MathematicalFormulationScientificCatalog.All.Count);

        Assert.True(
            MathematicalFormulationScientificCatalog.DlspSmallBucket
                .SupportsProblemClass(
                    CanonicalLotSizingProblemClassId
                        .DiscreteLotSizingAndScheduling));

        Assert.True(
            MathematicalFormulationScientificCatalog.CslpSmallBucket
                .SupportsProblemClass(
                    CanonicalLotSizingProblemClassId
                        .ContinuousSetupLotSizing));
    }

    [Theory]
    [InlineData(LotSizingProblemClassExtensionKind.Backlogging)]
    [InlineData(LotSizingProblemClassExtensionKind.SafetyStock)]
    [InlineData(LotSizingProblemClassExtensionKind.SetupTimes)]
    [InlineData(LotSizingProblemClassExtensionKind.ProductionLeadTimes)]
    [InlineData(LotSizingProblemClassExtensionKind.MinimumLotSize)]
    [InlineData(LotSizingProblemClassExtensionKind.MaximumLotSize)]
    [InlineData(LotSizingProblemClassExtensionKind.LotSizeMultiple)]
    [InlineData(LotSizingProblemClassExtensionKind.Purchasing)]
    [InlineData(LotSizingProblemClassExtensionKind.SupplierCapacity)]
    [InlineData(LotSizingProblemClassExtensionKind.Transportation)]
    [InlineData(LotSizingProblemClassExtensionKind.TransportLeadTime)]
    public void VerifiedSupportedExtensions_AreExplicit(
        LotSizingProblemClassExtensionKind extension)
    {
        Assert.True(
            MathematicalFormulationScientificCatalog.Standard
                .IsExtensionVerifiedSupported(extension));
    }

    [Theory]
    [InlineData(LotSizingProblemClassExtensionKind.StartUpCosts)]
    [InlineData(LotSizingProblemClassExtensionKind.StartUpTimes)]
    [InlineData(LotSizingProblemClassExtensionKind.MultipleObjectives)]
    [InlineData(LotSizingProblemClassExtensionKind.IntegratedScheduling)]
    [InlineData(LotSizingProblemClassExtensionKind.BigBucketScheduling)]
    [InlineData(LotSizingProblemClassExtensionKind.SmallBucketScheduling)]
    [InlineData(LotSizingProblemClassExtensionKind.MacroMicroScheduling)]
    [InlineData(LotSizingProblemClassExtensionKind.InitialSetupState)]
    [InlineData(LotSizingProblemClassExtensionKind.SetupCarryOver)]
    [InlineData(LotSizingProblemClassExtensionKind.SequenceDependentChangeoverTimes)]
    [InlineData(LotSizingProblemClassExtensionKind.SequenceDependentChangeoverCosts)]
    [InlineData(LotSizingProblemClassExtensionKind.MaximumSetupCount)]
    public void KnownUnsupportedExtensions_AreExplicit(
        LotSizingProblemClassExtensionKind extension)
    {
        Assert.True(
            MathematicalFormulationScientificCatalog.Standard
                .IsExtensionKnownUnsupported(extension));
    }

    [Fact]
    public void UnlistedExtension_RemainsUndeterminedRatherThanGuessed()
    {
        LotSizingProblemClassExtensionKind extension =
            LotSizingProblemClassExtensionKind.LostSales;

        Assert.False(
            MathematicalFormulationScientificCatalog.Standard
                .IsExtensionVerifiedSupported(extension));

        Assert.False(
            MathematicalFormulationScientificCatalog.Standard
                .IsExtensionKnownUnsupported(extension));
    }
}
