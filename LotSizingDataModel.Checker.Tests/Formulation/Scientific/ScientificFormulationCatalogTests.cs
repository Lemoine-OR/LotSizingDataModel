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
    public void StandardProfile_SupportsAllSixExecutableCoreClasses()
    {
        Assert.Equal(
            6,
            MathematicalFormulationScientificCatalog.Standard
                .SupportedProblemClasses.Count);

        Assert.All(
            LotSizingProblemClassCatalog.ExecutableClasses,
            definition =>
                Assert.True(
                    MathematicalFormulationScientificCatalog.Standard
                        .SupportsProblemClass(definition.Id)));
    }

    [Theory]
    [InlineData(LotSizingProblemClassExtensionKind.Backlogging)]
    [InlineData(LotSizingProblemClassExtensionKind.SafetyStock)]
    [InlineData(LotSizingProblemClassExtensionKind.SetupTimes)]
    [InlineData(LotSizingProblemClassExtensionKind.ProductionLeadTimes)]
    [InlineData(LotSizingProblemClassExtensionKind.MinimumLotSize)]
    [InlineData(LotSizingProblemClassExtensionKind.LotSizeMultiple)]
    [InlineData(LotSizingProblemClassExtensionKind.Purchasing)]
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
    [InlineData(LotSizingProblemClassExtensionKind.MaximumLotSize)]
    [InlineData(LotSizingProblemClassExtensionKind.SupplierCapacity)]
    [InlineData(LotSizingProblemClassExtensionKind.FinancialConstraints)]
    [InlineData(LotSizingProblemClassExtensionKind.MultipleObjectives)]
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
