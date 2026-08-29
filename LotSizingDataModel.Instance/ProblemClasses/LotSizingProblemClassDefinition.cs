using LotSizingDataModel.Instance.Notation.Matching;
using LotSizingDataModel.Instance.Taxonomy;

namespace LotSizingDataModel.Instance.ProblemClasses;

/// <summary>
/// One canonical scientific problem-class definition.
/// </summary>
public sealed class LotSizingProblemClassDefinition
{
    public LotSizingProblemClassDefinition(
        CanonicalLotSizingProblemClassId id,
        string code,
        string name,
        LotSizingProblemClassSupportLevel supportLevel,
        IEnumerable<string>? aliases = null,
        UniversalProblemSpecification? universalCoreSpecification = null,
        IEnumerable<string>? capabilityGaps = null,
        string? scientificNote = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException(
                "A canonical problem-class code is required.",
                nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "A canonical problem-class name is required.",
                nameof(name));
        }

        if (
            supportLevel ==
                LotSizingProblemClassSupportLevel.Executable &&
            universalCoreSpecification is null)
        {
            throw new ArgumentException(
                "Executable problem classes require a universal core " +
                "specification.",
                nameof(universalCoreSpecification));
        }

        Id = id;
        Code = code.Trim();
        Name = name.Trim();
        SupportLevel = supportLevel;
        UniversalCoreSpecification = universalCoreSpecification;
        ScientificNote = scientificNote;

        Aliases =
            (aliases ?? Array.Empty<string>())
                .Where(alias => !string.IsNullOrWhiteSpace(alias))
                .Select(alias => alias.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(alias => alias, StringComparer.OrdinalIgnoreCase)
                .ToArray();

        CapabilityGaps =
            (capabilityGaps ?? Array.Empty<string>())
                .Where(gap => !string.IsNullOrWhiteSpace(gap))
                .Select(gap => gap.Trim())
                .Distinct(StringComparer.Ordinal)
                .OrderBy(gap => gap, StringComparer.Ordinal)
                .ToArray();

        Concept =
            new ScientificConceptReference(
                Code,
                Name,
                ScientificConceptCategory.LotSizingProblemClass);
    }

    public CanonicalLotSizingProblemClassId Id { get; }
    public string Code { get; }
    public string Name { get; }
    public LotSizingProblemClassSupportLevel SupportLevel { get; }
    public IReadOnlyList<string> Aliases { get; }
    public UniversalProblemSpecification? UniversalCoreSpecification { get; }
    public IReadOnlyList<string> CapabilityGaps { get; }
    public string? ScientificNote { get; }
    public ScientificConceptReference Concept { get; }

    public override string ToString() =>
        $"{Code} — {Name}";
}
