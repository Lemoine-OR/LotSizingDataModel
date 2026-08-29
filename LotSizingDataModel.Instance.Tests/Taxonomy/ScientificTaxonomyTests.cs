using LotSizingDataModel.Instance.Taxonomy;

namespace LotSizingDataModel.Instance.Tests.Taxonomy;

public sealed class ScientificTaxonomyTests
{
    [Fact]
    public void Drp_IsPlanningParadigm_NotLotSizingProblemClass()
    {
        ScientificConceptReference drp =
            ScientificConceptCatalog.Drp;

        Assert.Equal("DRP", drp.Code);

        Assert.Equal(
            ScientificConceptCategory.PlanningParadigm,
            drp.Category);

        Assert.NotEqual(
            ScientificConceptCategory.LotSizingProblemClass,
            drp.Category);
    }

    [Fact]
    public void Mrp_IsPlanningParadigm_NotLotSizingProblemClass()
    {
        ScientificConceptReference mrp =
            ScientificConceptCatalog.Mrp;

        Assert.Equal(
            ScientificConceptCategory.PlanningParadigm,
            mrp.Category);

        Assert.NotEqual(
            ScientificConceptCategory.LotSizingProblemClass,
            mrp.Category);
    }

    [Fact]
    public void BitranYanasse_IsHistoricalClassification()
    {
        Assert.Equal(
            ScientificConceptCategory.HistoricalClassification,
            ScientificConceptCatalog.BitranYanasse.Category);
    }

    [Fact]
    public void CatalogLookup_IsCaseInsensitive()
    {
        Assert.Same(
            ScientificConceptCatalog.Drp,
            ScientificConceptCatalog.FindByCode("drp"));
    }

    [Fact]
    public void TaxonomyDimensions_AreDistinct()
    {
        Assert.NotEqual(
            ScientificConceptCategory.PlanningParadigm,
            ScientificConceptCategory.MathematicalFormulation);

        Assert.NotEqual(
            ScientificConceptCategory.LotSizingProblemClass,
            ScientificConceptCategory.SolutionMethod);

        Assert.NotEqual(
            ScientificConceptCategory.StructuralProperty,
            ScientificConceptCategory.PlanningParadigm);
    }
}
