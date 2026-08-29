namespace LotSizingDataModel.Instance.Taxonomy;

/// <summary>
/// Identifies one named scientific concept together with its taxonomic
/// category.
/// </summary>
public sealed class ScientificConceptReference
{
    public ScientificConceptReference(
        string code,
        string name,
        ScientificConceptCategory category)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException(
                "A scientific concept code is required.",
                nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "A scientific concept name is required.",
                nameof(name));
        }

        Code = code.Trim();
        Name = name.Trim();
        Category = category;
    }

    public string Code { get; }
    public string Name { get; }
    public ScientificConceptCategory Category { get; }

    public override string ToString() =>
        $"{Code} [{Category}]";
}
