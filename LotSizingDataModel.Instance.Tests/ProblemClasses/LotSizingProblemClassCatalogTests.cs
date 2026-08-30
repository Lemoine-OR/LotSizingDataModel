using LotSizingDataModel.Instance.ProblemClasses;
using LotSizingDataModel.Instance.Taxonomy;

namespace LotSizingDataModel.Instance.Tests.ProblemClasses;

public sealed class LotSizingProblemClassCatalogTests
{
    [Fact]
    public void Catalog_HasTenExecutableNoClassifiableAndNoCatalogOnly()
    {
        Assert.Equal(
            10,
            LotSizingProblemClassCatalog.All.Count);

        Assert.Equal(
            10,
            LotSizingProblemClassCatalog.ExecutableClasses.Count);

        Assert.Empty(
            LotSizingProblemClassCatalog.ClassifiableClasses);

        Assert.DoesNotContain(
            LotSizingProblemClassCatalog.All,
            definition =>
                definition.SupportLevel ==
                LotSizingProblemClassSupportLevel.CatalogOnly);
    }

    [Fact]
    public void EveryEntry_IsTaxonomicallyALotSizingProblemClass()
    {
        Assert.All(
            LotSizingProblemClassCatalog.All,
            definition =>
                Assert.Equal(
                    ScientificConceptCategory.LotSizingProblemClass,
                    definition.Concept.Category));
    }

    [Fact]
    public void DrpAndMrp_AreNotProblemClassCatalogEntries()
    {
        Assert.DoesNotContain(
            LotSizingProblemClassCatalog.All,
            definition =>
                definition.Code.Equals(
                    "DRP",
                    StringComparison.OrdinalIgnoreCase));

        Assert.DoesNotContain(
            LotSizingProblemClassCatalog.All,
            definition =>
                definition.Code.Equals(
                    "MRP",
                    StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void SmallBucketSupportLevels_ReflectExecutableMilestone()
    {
        Assert.All(
            new[]
            {
                LotSizingProblemClassCatalog.Dlsp,
                LotSizingProblemClassCatalog.Cslp,
                LotSizingProblemClassCatalog.Plsp
            },
            definition =>
            {
                Assert.Equal(
                    LotSizingProblemClassSupportLevel.Executable,
                    definition.SupportLevel);

                Assert.NotNull(
                    definition.UniversalCoreSpecification);
            });

        Assert.Equal(
            LotSizingProblemClassSupportLevel.Executable,
            LotSizingProblemClassCatalog.Glsp.SupportLevel);

        Assert.NotNull(
            LotSizingProblemClassCatalog.Glsp
                .UniversalCoreSpecification);
    }

    [Fact]
    public void ExecutableClasses_HaveUniversalCoreSpecifications()
    {
        Assert.All(
            LotSizingProblemClassCatalog.ExecutableClasses,
            definition =>
                Assert.NotNull(
                    definition.UniversalCoreSpecification));
    }
}
