using LotSizingDataModel.Instance.ProblemClasses;
using LotSizingDataModel.Instance.Taxonomy;

namespace LotSizingDataModel.Instance.Tests.ProblemClasses;

public sealed class LotSizingProblemClassCatalogTests
{
    [Fact]
    public void Catalog_HasSixExecutableThreeClassifiableAndOneCatalogOnly()
    {
        Assert.Equal(
            10,
            LotSizingProblemClassCatalog.All.Count);

        Assert.Equal(
            6,
            LotSizingProblemClassCatalog.ExecutableClasses.Count);

        Assert.Equal(
            3,
            LotSizingProblemClassCatalog.ClassifiableClasses.Count);

        Assert.Single(
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
    public void SmallBucketClasses_AreClassifiableButNotExecutable()
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
                    LotSizingProblemClassSupportLevel.Classifiable,
                    definition.SupportLevel);

                Assert.Null(
                    definition.UniversalCoreSpecification);
            });

        Assert.Equal(
            LotSizingProblemClassSupportLevel.CatalogOnly,
            LotSizingProblemClassCatalog.Glsp.SupportLevel);
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
