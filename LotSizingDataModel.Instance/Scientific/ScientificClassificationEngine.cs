using LotSizingDataModel.Core;
using LotSizingDataModel.Instance.Analysis;
using LotSizingDataModel.Instance.Classification;
using LotSizingDataModel.Instance.Descriptors;
using LotSizingDataModel.Instance.Notation;
using LotSizingDataModel.Instance.Notation.Matching;
using LotSizingDataModel.Instance.ProblemClasses;
using LotSizingDataModel.Instance.Validation;

namespace LotSizingDataModel.Instance.Scientific;

/// <summary>
/// Single entry point for current scientific classification capabilities.
/// </summary>
/// <remarks>
/// This engine composes existing services; it does not duplicate their rules.
///
/// It deliberately does not infer planning paradigms, mathematical
/// formulations or solution methods.
/// </remarks>
public sealed class ScientificClassificationEngine
{
    private readonly InstanceModelChecker _modelChecker;
    private readonly UniversalNotationGenerator _notationGenerator;
    private readonly UniversalNotationParser _notationParser;
    private readonly UniversalNotationMatcher _notationMatcher;
    private readonly LotSizingProblemClassDetector _problemClassDetector;
    private readonly HistoricalClassificationCapabilityAnalyzer
        _historicalCapabilityAnalyzer;

    public ScientificClassificationEngine()
        : this(
            new InstanceModelChecker(),
            new UniversalNotationGenerator(),
            new UniversalNotationParser(),
            new UniversalNotationMatcher(),
            new LotSizingProblemClassDetector(),
            new HistoricalClassificationCapabilityAnalyzer())
    {
    }

    public ScientificClassificationEngine(
        InstanceModelChecker modelChecker,
        UniversalNotationGenerator notationGenerator,
        UniversalNotationParser notationParser,
        UniversalNotationMatcher notationMatcher,
        LotSizingProblemClassDetector problemClassDetector,
        HistoricalClassificationCapabilityAnalyzer
            historicalCapabilityAnalyzer)
    {
        _modelChecker =
            modelChecker ??
            throw new ArgumentNullException(nameof(modelChecker));

        _notationGenerator =
            notationGenerator ??
            throw new ArgumentNullException(nameof(notationGenerator));

        _notationParser =
            notationParser ??
            throw new ArgumentNullException(nameof(notationParser));

        _notationMatcher =
            notationMatcher ??
            throw new ArgumentNullException(nameof(notationMatcher));

        _problemClassDetector =
            problemClassDetector ??
            throw new ArgumentNullException(nameof(problemClassDetector));

        _historicalCapabilityAnalyzer =
            historicalCapabilityAnalyzer ??
            throw new ArgumentNullException(
                nameof(historicalCapabilityAnalyzer));
    }

    public ScientificClassificationResult Analyze(
        LotSizingProblemDescriptor descriptor,
        ScientificClassificationRequest? request = null)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        return AnalyzeDescriptorCore(
            descriptor,
            request ?? new ScientificClassificationRequest(),
            instanceValidation: null,
            initialDiagnostics:
                Array.Empty<ScientificClassificationDiagnostic>());
    }

    public ScientificClassificationResult Analyze(
        LotSizingProblemFeatures features,
        ScientificClassificationRequest? request = null)
    {
        ArgumentNullException.ThrowIfNull(features);

        return Analyze(
            LotSizingProblemDescriptor.FromLegacyFeatures(
                features),
            request);
    }

    public ScientificClassificationResult Analyze(
        SupplyChain supplyChain,
        ScientificClassificationRequest? request = null)
    {
        ArgumentNullException.ThrowIfNull(supplyChain);

        // A transient wrapper lets SupplyChain analysis honor the exact same
        // structural/model-validation gate as LotSizingInstance analysis
        // without mutating or serializing the caller's model.
        return Analyze(
            new LotSizingInstance(
                supplyChain),
            request);
    }

    public ScientificClassificationResult Analyze(
        LotSizingInstance instance,
        ScientificClassificationRequest? request = null)
    {
        ArgumentNullException.ThrowIfNull(instance);

        ScientificClassificationRequest effectiveRequest =
            request ??
            new ScientificClassificationRequest();

        InstanceModelCheckResult validation =
            _modelChecker.Check(instance);

        if (!validation.Capabilities.CanClassify)
        {
            return CreateBlockedResult(
                instanceValidation: validation,
                diagnostics:
                    new[]
                    {
                        new ScientificClassificationDiagnostic(
                            code: "LSDM-SCI-001",
                            severity:
                                ScientificClassificationDiagnosticSeverity
                                    .Error,
                            path: "instance",
                            message:
                                "Scientific classification is blocked by " +
                                "instance-model validation.")
                    },
                declaredNotation:
                    effectiveRequest.DeclaredNotation);
        }

        return AnalyzeSupplyChain(
            instance.SupplyChain,
            effectiveRequest,
            validation);
    }

    private ScientificClassificationResult AnalyzeSupplyChain(
        SupplyChain supplyChain,
        ScientificClassificationRequest request,
        InstanceModelCheckResult? instanceValidation)
    {
        ProductStructureAnalysis productStructureAnalysis =
            ProductStructureAnalyzer.Analyze(
                supplyChain);

        LotSizingProblemFeatures features =
            LotSizingProblemFeatureExtractor.Extract(
                supplyChain,
                productStructureAnalysis,
                request.NumericalTolerance);

        LotSizingProblemDescriptor descriptor =
            LotSizingProblemDescriptor.FromLegacyFeatures(
                features,
                supplyChain);

        var diagnostics =
            new List<ScientificClassificationDiagnostic>();

        diagnostics.AddRange(
            productStructureAnalysis.Warnings.Select(
                warning =>
                    new ScientificClassificationDiagnostic(
                        code: "LSDM-SCI-002",
                        severity:
                            ScientificClassificationDiagnosticSeverity
                                .Warning,
                        path: "descriptor.structure",
                        message: warning)));

        diagnostics.AddRange(
            productStructureAnalysis.Errors.Select(
                error =>
                    new ScientificClassificationDiagnostic(
                        code: "LSDM-SCI-003",
                        severity:
                            ScientificClassificationDiagnosticSeverity
                                .Error,
                        path: "descriptor.structure",
                        message: error)));

        if (productStructureAnalysis.Errors.Count > 0)
        {
            return CreateBlockedResult(
                instanceValidation,
                diagnostics,
                request.DeclaredNotation,
                descriptor);
        }

        return AnalyzeDescriptorCore(
            descriptor,
            request,
            instanceValidation,
            diagnostics);
    }

    private ScientificClassificationResult AnalyzeDescriptorCore(
        LotSizingProblemDescriptor descriptor,
        ScientificClassificationRequest request,
        InstanceModelCheckResult? instanceValidation,
        IEnumerable<ScientificClassificationDiagnostic>
            initialDiagnostics)
    {
        var diagnostics =
            new List<ScientificClassificationDiagnostic>(
                initialDiagnostics);

        UniversalLotSizingNotation detectedNotation =
            _notationGenerator.Generate(
                descriptor,
                request.DerivedSemantics);

        IReadOnlyList<LotSizingProblemClassMatchResult>
            problemClasses =
                _problemClassDetector.Detect(
                    descriptor);

        LotSizingProblemClassMatchResult? primaryProblemClass =
            problemClasses.Count == 1
                ? problemClasses[0]
                : null;

        if (problemClasses.Count == 0)
        {
            diagnostics.Add(
                new ScientificClassificationDiagnostic(
                    code: "LSDM-SCI-020",
                    severity:
                        ScientificClassificationDiagnosticSeverity
                            .Warning,
                    path: "classification.problemClasses",
                    message:
                        "No currently detectable canonical lot-sizing " +
                        "problem class matches this descriptor."));
        }
        else if (problemClasses.Count > 1)
        {
            diagnostics.Add(
                new ScientificClassificationDiagnostic(
                    code: "LSDM-SCI-021",
                    severity:
                        ScientificClassificationDiagnosticSeverity
                            .Warning,
                    path: "classification.problemClasses",
                    message:
                        "More than one canonical problem class is " +
                        "compatible; no primary class was selected."));
        }

        ScientificNotationComparison notationComparison =
            CompareDeclaredNotation(
                descriptor,
                request,
                diagnostics);

        IReadOnlyList<HistoricalClassificationCapability>
            historicalCapabilities =
                _historicalCapabilityAnalyzer.Analyze(
                    descriptor);

        return new ScientificClassificationResult(
            isBlocked: false,
            descriptor,
            detectedNotation,
            notationComparison,
            problemClasses,
            primaryProblemClass,
            historicalCapabilities,
            diagnostics,
            instanceValidation);
    }

    private ScientificNotationComparison CompareDeclaredNotation(
        LotSizingProblemDescriptor descriptor,
        ScientificClassificationRequest request,
        ICollection<ScientificClassificationDiagnostic> diagnostics)
    {
        if (!request.HasDeclaredNotation)
        {
            return new ScientificNotationComparison(
                string.Empty,
                ScientificNotationComparisonKind.NotDeclared);
        }

        UniversalProblemSpecification declaredSpecification;

        try
        {
            UniversalLotSizingNotation parsed =
                _notationParser.Parse(
                    request.DeclaredNotation);

            declaredSpecification =
                new UniversalProblemSpecification(
                    parsed);
        }
        catch (Exception exception)
            when (
                exception is FormatException or
                NotSupportedException or
                ArgumentException)
        {
            diagnostics.Add(
                new ScientificClassificationDiagnostic(
                    code: "LSDM-SCI-010",
                    severity:
                        ScientificClassificationDiagnosticSeverity
                            .Error,
                    path: "classification.declaredNotation",
                    message:
                        "Declared notation is invalid: " +
                        exception.Message));

            return new ScientificNotationComparison(
                request.DeclaredNotation,
                ScientificNotationComparisonKind
                    .InvalidDeclaredNotation);
        }

        UniversalNotationMatchResult match =
            _notationMatcher.Match(
                descriptor,
                declaredSpecification,
                request.DerivedSemantics);

        ScientificNotationComparisonKind kind =
            match.Kind switch
            {
                UniversalNotationMatchKind.Exact =>
                    ScientificNotationComparisonKind.Exact,

                UniversalNotationMatchKind.Compatible =>
                    ScientificNotationComparisonKind.Compatible,

                UniversalNotationMatchKind.Incomplete =>
                    ScientificNotationComparisonKind.Incomplete,

                UniversalNotationMatchKind.Contradiction =>
                    ScientificNotationComparisonKind.Contradiction,

                _ => throw new ArgumentOutOfRangeException()
            };

        AppendDeclaredNotationDiagnostic(
            kind,
            diagnostics);

        return new ScientificNotationComparison(
            request.DeclaredNotation,
            kind,
            declaredSpecification,
            match);
    }

    private static void AppendDeclaredNotationDiagnostic(
        ScientificNotationComparisonKind kind,
        ICollection<ScientificClassificationDiagnostic> diagnostics)
    {
        switch (kind)
        {
            case ScientificNotationComparisonKind.Exact:
            case ScientificNotationComparisonKind.NotDeclared:
                return;

            case ScientificNotationComparisonKind.Compatible:
                diagnostics.Add(
                    new ScientificClassificationDiagnostic(
                        code: "LSDM-SCI-011",
                        severity:
                            ScientificClassificationDiagnosticSeverity
                                .Information,
                        path: "classification.declaredNotation",
                        message:
                            "Declared notation is compatible but less " +
                            "specific than detected instance semantics."));
                return;

            case ScientificNotationComparisonKind.Incomplete:
                diagnostics.Add(
                    new ScientificClassificationDiagnostic(
                        code: "LSDM-SCI-012",
                        severity:
                            ScientificClassificationDiagnosticSeverity
                                .Warning,
                        path: "classification.declaredNotation",
                        message:
                            "Declared notation requires semantic " +
                            "information that has not been analyzed."));
                return;

            case ScientificNotationComparisonKind.Contradiction:
                diagnostics.Add(
                    new ScientificClassificationDiagnostic(
                        code: "LSDM-SCI-013",
                        severity:
                            ScientificClassificationDiagnosticSeverity
                                .Error,
                        path: "classification.declaredNotation",
                        message:
                            "Declared notation contradicts detected " +
                            "instance semantics."));
                return;

            case ScientificNotationComparisonKind.InvalidDeclaredNotation:
                return;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(kind),
                    kind,
                    "Unknown notation-comparison kind.");
        }
    }

    private static ScientificClassificationResult CreateBlockedResult(
        InstanceModelCheckResult? instanceValidation,
        IEnumerable<ScientificClassificationDiagnostic> diagnostics,
        string declaredNotation,
        LotSizingProblemDescriptor? descriptor = null)
    {
        ScientificNotationComparison notationComparison =
            string.IsNullOrWhiteSpace(declaredNotation)
                ? new ScientificNotationComparison(
                    string.Empty,
                    ScientificNotationComparisonKind.NotDeclared)
                : new ScientificNotationComparison(
                    declaredNotation,
                    ScientificNotationComparisonKind
                        .NotEvaluated);

        return new ScientificClassificationResult(
            isBlocked: true,
            descriptor,
            detectedNotation: null,
            notationComparison,
            problemClassMatches:
                Array.Empty<LotSizingProblemClassMatchResult>(),
            primaryProblemClass: null,
            historicalCapabilities:
                Array.Empty<HistoricalClassificationCapability>(),
            diagnostics,
            instanceValidation);
    }
}
