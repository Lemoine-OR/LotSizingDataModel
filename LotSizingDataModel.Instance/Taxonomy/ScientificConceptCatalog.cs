namespace LotSizingDataModel.Instance.Taxonomy;

/// <summary>
/// Provides a small canonical concept catalog used to keep scientific
/// categories separated before historical mappings are introduced.
/// </summary>
/// <remarks>
/// This catalog is taxonomic metadata. It does not assert that a given
/// instance belongs to any listed concept.
/// </remarks>
public static class ScientificConceptCatalog
{
    public static ScientificConceptReference Mrp { get; } =
        new(
            code: "MRP",
            name: "Material Requirements Planning",
            category:
                ScientificConceptCategory.PlanningParadigm);

    public static ScientificConceptReference Drp { get; } =
        new(
            code: "DRP",
            name: "Distribution Requirements Planning",
            category:
                ScientificConceptCategory.PlanningParadigm);

    public static ScientificConceptReference BitranYanasse { get; } =
        new(
            code: "BY",
            name: "Bitran-Yanasse temporal classification",
            category:
                ScientificConceptCategory.HistoricalClassification);

    public static IReadOnlyList<ScientificConceptReference> All { get; } =
        new[]
        {
            Mrp,
            Drp,
            BitranYanasse
        };

    public static ScientificConceptReference? FindByCode(
        string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        return All.FirstOrDefault(
            concept =>
                concept.Code.Equals(
                    code.Trim(),
                    StringComparison.OrdinalIgnoreCase));
    }
}
