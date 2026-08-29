namespace LotSizingDataModel.Instance.ProblemClasses;

/// <summary>
/// Complete canonical problem-class membership assessment.
/// </summary>
public sealed class LotSizingProblemClassMatchResult
{
    internal LotSizingProblemClassMatchResult(
        LotSizingProblemClassDefinition definition,
        LotSizingProblemClassMatchKind kind,
        IEnumerable<LotSizingProblemClassExtensionKind>? extensions = null,
        IEnumerable<string>? failedRequirements = null)
    {
        Definition =
            definition ??
            throw new ArgumentNullException(nameof(definition));

        Kind = kind;

        Extensions =
            (extensions ??
             Array.Empty<LotSizingProblemClassExtensionKind>())
                .Distinct()
                .OrderBy(extension => (int)extension)
                .ToArray();

        FailedRequirements =
            (failedRequirements ??
             Array.Empty<string>())
                .Distinct(StringComparer.Ordinal)
                .ToArray();
    }

    public LotSizingProblemClassDefinition Definition { get; }
    public LotSizingProblemClassMatchKind Kind { get; }
    public IReadOnlyList<LotSizingProblemClassExtensionKind> Extensions { get; }
    public IReadOnlyList<string> FailedRequirements { get; }

    public bool IsMember =>
        Kind is
            LotSizingProblemClassMatchKind.ExactCore or
            LotSizingProblemClassMatchKind.CompatibleExtension;
}
