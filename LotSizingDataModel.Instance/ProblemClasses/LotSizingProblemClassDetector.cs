using LotSizingDataModel.Instance.Descriptors;

namespace LotSizingDataModel.Instance.ProblemClasses;

/// <summary>
/// Detects all executable canonical problem classes compatible with one
/// descriptor.
/// </summary>
public sealed class LotSizingProblemClassDetector
{
    private readonly LotSizingProblemClassAnalyzer _analyzer;

    public LotSizingProblemClassDetector()
        : this(new LotSizingProblemClassAnalyzer())
    {
    }

    public LotSizingProblemClassDetector(
        LotSizingProblemClassAnalyzer analyzer)
    {
        _analyzer =
            analyzer ??
            throw new ArgumentNullException(nameof(analyzer));
    }

    public IReadOnlyList<LotSizingProblemClassMatchResult> Detect(
        LotSizingProblemDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        return LotSizingProblemClassCatalog.ExecutableClasses
            .Select(
                definition =>
                    _analyzer.Assess(
                        descriptor,
                        definition))
            .Where(result => result.IsMember)
            .OrderBy(
                result =>
                    result.Kind ==
                    LotSizingProblemClassMatchKind.ExactCore
                        ? 0
                        : 1)
            .ThenBy(
                result =>
                    result.Definition.Code,
                StringComparer.Ordinal)
            .ToArray();
    }

    public LotSizingProblemClassMatchResult? DetectSingle(
        LotSizingProblemDescriptor descriptor)
    {
        IReadOnlyList<LotSizingProblemClassMatchResult> matches =
            Detect(descriptor);

        return matches.Count == 1
            ? matches[0]
            : null;
    }
}
