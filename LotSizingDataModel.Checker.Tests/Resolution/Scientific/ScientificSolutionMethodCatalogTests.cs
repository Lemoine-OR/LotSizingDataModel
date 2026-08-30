using LotSizingDataModel.Instance.ProblemClasses;
using LotSizingDataModel.Solver.Resolution.Scientific;

namespace LotSizingDataModel.Checker.Tests.Resolution.Scientific;

public sealed class ScientificSolutionMethodCatalogTests
{
    [Fact]
    public void OnlyGeneralMilp_IsCurrentlyExecutable()
    {
        ScientificSolutionMethodDefinition method =
            Assert.Single(
                ScientificSolutionMethodCatalog.ExecutableMethods);

        Assert.Equal(
            "MILP-GENERAL",
            method.MethodId);

        Assert.Equal(
            ScientificSolutionMethodCategory
                .MixedIntegerLinearProgramming,
            method.Category);
    }

    [Fact]
    public void GeneralMilp_CoversAllNineExecutableCanonicalClasses()
    {
        Assert.Equal(
            9,
            ScientificSolutionMethodCatalog.GeneralMilp
                .ApplicableProblemClasses.Count);

        Assert.All(
            LotSizingProblemClassCatalog.ExecutableClasses,
            definition =>
                Assert.True(
                    ScientificSolutionMethodCatalog.GeneralMilp
                        .IsApplicableTo(definition.Id)));
    }

    [Theory]
    [InlineData("DP-SI-ULS")]
    [InlineData("SP-SI-ULS")]
    [InlineData("LR-CLSP")]
    [InlineData("DW-BP-CLSP")]
    [InlineData("HEURISTIC-GENERAL")]
    [InlineData("METAHEURISTIC-GENERAL")]
    [InlineData("MATHEURISTIC-GENERAL")]
    public void FutureMethodFamilies_AreCatalogOnly(
        string methodId)
    {
        ScientificSolutionMethodDefinition method =
            ScientificSolutionMethodCatalog.Find(methodId)!;

        Assert.NotNull(method);

        Assert.Equal(
            ScientificSolutionMethodSupportLevel.CatalogOnly,
            method.SupportLevel);
    }
}
