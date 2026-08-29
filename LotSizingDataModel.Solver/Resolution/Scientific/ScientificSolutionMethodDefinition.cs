using LotSizingDataModel.Instance.ProblemClasses;

namespace LotSizingDataModel.Solver.Resolution.Scientific;

/// <summary>
/// One scientific solution-method definition.
/// </summary>
public sealed class ScientificSolutionMethodDefinition
{
    public ScientificSolutionMethodDefinition(
        string methodId,
        string name,
        ScientificSolutionMethodCategory category,
        ScientificSolutionMethodSupportLevel supportLevel,
        IEnumerable<CanonicalLotSizingProblemClassId>
            applicableProblemClasses,
        bool requiresMathematicalFormulation,
        bool requiresMilpBackend,
        IEnumerable<string>? evidence = null,
        string? note = null)
    {
        if (string.IsNullOrWhiteSpace(methodId))
        {
            throw new ArgumentException(
                "A solution-method identifier is required.",
                nameof(methodId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "A solution-method name is required.",
                nameof(name));
        }

        ArgumentNullException.ThrowIfNull(applicableProblemClasses);

        MethodId = methodId.Trim();
        Name = name.Trim();
        Category = category;
        SupportLevel = supportLevel;
        RequiresMathematicalFormulation =
            requiresMathematicalFormulation;
        RequiresMilpBackend =
            requiresMilpBackend;

        ApplicableProblemClasses =
            applicableProblemClasses
                .Distinct()
                .OrderBy(value => (int)value)
                .ToArray();

        Evidence =
            (evidence ?? Array.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToArray();

        Note = note ?? string.Empty;
    }

    public string MethodId { get; }
    public string Name { get; }
    public ScientificSolutionMethodCategory Category { get; }
    public ScientificSolutionMethodSupportLevel SupportLevel { get; }

    public IReadOnlyList<CanonicalLotSizingProblemClassId>
        ApplicableProblemClasses { get; }

    public bool RequiresMathematicalFormulation { get; }
    public bool RequiresMilpBackend { get; }
    public IReadOnlyList<string> Evidence { get; }
    public string Note { get; }

    public bool IsApplicableTo(
        CanonicalLotSizingProblemClassId problemClass) =>
            ApplicableProblemClasses.Contains(problemClass);

    public override string ToString() =>
        $"{MethodId} — {Name}";
}
