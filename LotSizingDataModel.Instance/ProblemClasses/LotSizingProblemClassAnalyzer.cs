using LotSizingDataModel.Instance.Descriptors;
using LotSizingDataModel.Instance.Notation.Matching;

namespace LotSizingDataModel.Instance.ProblemClasses;

/// <summary>
/// Assesses one descriptor against one canonical problem-class core.
/// </summary>
public sealed class LotSizingProblemClassAnalyzer
{
    private readonly UniversalNotationMatcher _matcher;
    private readonly LotSizingProblemClassExtensionAnalyzer _extensionAnalyzer;

    public LotSizingProblemClassAnalyzer()
        : this(
            new UniversalNotationMatcher(),
            new LotSizingProblemClassExtensionAnalyzer())
    {
    }

    public LotSizingProblemClassAnalyzer(
        UniversalNotationMatcher matcher,
        LotSizingProblemClassExtensionAnalyzer extensionAnalyzer)
    {
        _matcher =
            matcher ??
            throw new ArgumentNullException(nameof(matcher));

        _extensionAnalyzer =
            extensionAnalyzer ??
            throw new ArgumentNullException(nameof(extensionAnalyzer));
    }

    public LotSizingProblemClassMatchResult Assess(
        LotSizingProblemDescriptor descriptor,
        LotSizingProblemClassDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(definition);

        if (
            definition.SupportLevel ==
            LotSizingProblemClassSupportLevel.CatalogOnly)
        {
            return new LotSizingProblemClassMatchResult(
                definition,
                LotSizingProblemClassMatchKind.NotRepresentable,
                failedRequirements:
                    definition.CapabilityGaps);
        }

        UniversalNotationMatchResult universal =
            _matcher.Match(
                descriptor,
                definition.UniversalCoreSpecification!);

        if (
            universal.Kind ==
            UniversalNotationMatchKind.Contradiction)
        {
            return new LotSizingProblemClassMatchResult(
                definition,
                LotSizingProblemClassMatchKind.NotApplicable,
                failedRequirements:
                    universal.Issues.Select(
                        issue => issue.ToString()));
        }

        if (
            universal.Kind ==
            UniversalNotationMatchKind.Incomplete)
        {
            return new LotSizingProblemClassMatchResult(
                definition,
                LotSizingProblemClassMatchKind.Incomplete,
                failedRequirements:
                    universal.Issues.Select(
                        issue => issue.ToString()));
        }

        IReadOnlyList<LotSizingProblemClassExtensionKind>
            extensions =
                _extensionAnalyzer.Analyze(
                    descriptor);

        return new LotSizingProblemClassMatchResult(
            definition,
            extensions.Count == 0
                ? LotSizingProblemClassMatchKind.ExactCore
                : LotSizingProblemClassMatchKind.CompatibleExtension,
            extensions);
    }
}
