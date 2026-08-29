using LotSizingDataModel.Instance.Descriptors;
using LotSizingDataModel.Instance.Notation;
using LotSizingDataModel.Instance.ProblemClasses;
using LotSizingDataModel.Instance.Validation;

namespace LotSizingDataModel.Instance.Scientific;

/// <summary>
/// Consolidated, UI-independent scientific classification result.
/// </summary>
public sealed class ScientificClassificationResult
{
    internal ScientificClassificationResult(
        bool isBlocked,
        LotSizingProblemDescriptor? descriptor,
        UniversalLotSizingNotation? detectedNotation,
        ScientificNotationComparison notationComparison,
        IEnumerable<LotSizingProblemClassMatchResult> problemClassMatches,
        LotSizingProblemClassMatchResult? primaryProblemClass,
        IEnumerable<HistoricalClassificationCapability>
            historicalCapabilities,
        IEnumerable<ScientificClassificationDiagnostic> diagnostics,
        InstanceModelCheckResult? instanceValidation = null)
    {
        IsBlocked = isBlocked;
        Descriptor = descriptor;
        DetectedNotation = detectedNotation;

        NotationComparison =
            notationComparison ??
            throw new ArgumentNullException(
                nameof(notationComparison));

        ProblemClassMatches =
            (problemClassMatches ??
             throw new ArgumentNullException(
                 nameof(problemClassMatches)))
                .ToArray();

        PrimaryProblemClass =
            primaryProblemClass;

        HistoricalCapabilities =
            (historicalCapabilities ??
             throw new ArgumentNullException(
                 nameof(historicalCapabilities)))
                .ToArray();

        Diagnostics =
            (diagnostics ??
             throw new ArgumentNullException(
                 nameof(diagnostics)))
                .ToArray();

        InstanceValidation =
            instanceValidation;

        Coverage =
            ScientificClassificationCoverage.Current;
    }

    public bool IsBlocked { get; }

    public LotSizingProblemDescriptor? Descriptor { get; }

    /// <summary>
    /// Gets notation computed from the actual descriptor and supplied derived
    /// analyses. It is never copied from DeclaredNotation.
    /// </summary>
    public UniversalLotSizingNotation? DetectedNotation { get; }

    public string? DetectedNotationText =>
        DetectedNotation?.Render();

    public ScientificNotationComparison NotationComparison { get; }

    public string DeclaredNotationText =>
        NotationComparison.DeclaredText;

    public IReadOnlyList<LotSizingProblemClassMatchResult>
        ProblemClassMatches { get; }

    public LotSizingProblemClassMatchResult?
        PrimaryProblemClass { get; }

    public IReadOnlyList<HistoricalClassificationCapability>
        HistoricalCapabilities { get; }

    public IReadOnlyList<ScientificClassificationDiagnostic>
        Diagnostics { get; }

    public InstanceModelCheckResult? InstanceValidation { get; }

    public ScientificClassificationCoverage Coverage { get; }

    public bool HasErrors =>
        Diagnostics.Any(
            diagnostic => diagnostic.IsError);

    public bool HasDeclaredNotationConflict =>
        NotationComparison.Kind ==
        ScientificNotationComparisonKind.Contradiction;
}
