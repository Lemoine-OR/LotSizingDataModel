using LotSizingDataModel.Instance;
using LotSizingDataModel.Instance.Notation.Matching;
using LotSizingDataModel.Instance.Scientific;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solution.Metadata.Scientific;
using LotSizingDataModel.Solver.Formulation.Scientific;
using LotSizingDataModel.Solver.Resolution.Scientific;
using LotSizingDataModel.Solver.Common;

namespace LotSizingDataModel.Checker.Scientific;

/// <summary>
/// Re-evaluates a solution's recorded scientific provenance against the
/// current instance and current formulation capability catalog.
/// </summary>
public sealed class SolutionScientificProvenanceChecker
{
    private readonly ScientificClassificationEngine _classificationEngine;
    private readonly UniversalNotationMatcher _notationMatcher;
    private readonly ScientificFormulationCompatibilityService
        _formulationCompatibilityService;

    public SolutionScientificProvenanceChecker()
        : this(
            new ScientificClassificationEngine(),
            new UniversalNotationMatcher(),
            new ScientificFormulationCompatibilityService())
    {
    }

    public SolutionScientificProvenanceChecker(
        ScientificClassificationEngine classificationEngine,
        UniversalNotationMatcher notationMatcher,
        ScientificFormulationCompatibilityService
            formulationCompatibilityService)
    {
        _classificationEngine =
            classificationEngine ??
            throw new ArgumentNullException(nameof(classificationEngine));

        _notationMatcher =
            notationMatcher ??
            throw new ArgumentNullException(nameof(notationMatcher));

        _formulationCompatibilityService =
            formulationCompatibilityService ??
            throw new ArgumentNullException(
                nameof(formulationCompatibilityService));
    }

    public SolutionScientificProvenanceCheckResult Check(
        LotSizingInstance instance,
        LotSizingSolution solution)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(solution);

        SolutionScientificProvenanceReadResult read =
            SolutionScientificProvenanceCodec.Read(
                solution.GenerationMetadata);

        if (
            read.Kind ==
            SolutionScientificProvenanceReadKind.Missing)
        {
            return Result(
                SolutionScientificProvenanceCheckKind.Missing,
                provenance: null,
                classification: null,
                compatibility: null,
                Diagnostic(
                    "LSDM-PROV-001",
                    SolutionScientificProvenanceDiagnosticSeverity.Warning,
                    "solution.generationMetadata",
                    "Scientific solution provenance is not recorded."));
        }

        if (
            read.Kind ==
            SolutionScientificProvenanceReadKind.Invalid)
        {
            return Result(
                SolutionScientificProvenanceCheckKind.Invalid,
                provenance: null,
                classification: null,
                compatibility: null,
                Diagnostic(
                    "LSDM-PROV-002",
                    SolutionScientificProvenanceDiagnosticSeverity.Error,
                    "solution.generationMetadata",
                    read.Diagnostic));
        }

        SolutionScientificProvenance provenance =
            read.Provenance!;

        ScientificClassificationResult classification =
            _classificationEngine.Analyze(instance);

        if (
            classification.IsBlocked ||
            classification.Descriptor is null)
        {
            return Result(
                SolutionScientificProvenanceCheckKind.Incomplete,
                provenance,
                classification,
                compatibility: null,
                Diagnostic(
                    "LSDM-PROV-003",
                    SolutionScientificProvenanceDiagnosticSeverity.Warning,
                    "instance",
                    "Current instance scientific classification is blocked."));
        }

        var diagnostics =
            new List<SolutionScientificProvenanceDiagnostic>();

        SolutionScientificProvenanceCheckKind aggregate =
            SolutionScientificProvenanceCheckKind.Coherent;

        UniversalNotationMatchResult notationMatch;

        try
        {
            notationMatch =
                _notationMatcher.Match(
                    classification.Descriptor,
                    provenance.DetectedNotation);
        }
        catch (Exception exception)
            when (
                exception is FormatException or
                NotSupportedException or
                ArgumentException)
        {
            return Result(
                SolutionScientificProvenanceCheckKind.Invalid,
                provenance,
                classification,
                compatibility: null,
                Diagnostic(
                    "LSDM-PROV-004",
                    SolutionScientificProvenanceDiagnosticSeverity.Error,
                    "solution.provenance.detectedNotation",
                    "Recorded detected notation is invalid: " +
                    exception.Message));
        }

        switch (notationMatch.Kind)
        {
            case UniversalNotationMatchKind.Exact:
                break;

            case UniversalNotationMatchKind.Compatible:
                aggregate =
                    Merge(
                        aggregate,
                        SolutionScientificProvenanceCheckKind.Stale);

                diagnostics.Add(
                    Diagnostic(
                        "LSDM-PROV-010",
                        SolutionScientificProvenanceDiagnosticSeverity.Warning,
                        "solution.provenance.detectedNotation",
                        "Recorded notation remains compatible but is less " +
                        "specific than current detected semantics."));
                break;

            case UniversalNotationMatchKind.Incomplete:
                aggregate =
                    Merge(
                        aggregate,
                        SolutionScientificProvenanceCheckKind.Incomplete);

                diagnostics.Add(
                    Diagnostic(
                        "LSDM-PROV-011",
                        SolutionScientificProvenanceDiagnosticSeverity.Warning,
                        "solution.provenance.detectedNotation",
                        "Current analysis lacks information required to fully " +
                        "revalidate the recorded notation."));
                break;

            case UniversalNotationMatchKind.Contradiction:
                aggregate =
                    Merge(
                        aggregate,
                        SolutionScientificProvenanceCheckKind.Contradiction);

                diagnostics.Add(
                    Diagnostic(
                        "LSDM-PROV-012",
                        SolutionScientificProvenanceDiagnosticSeverity.Error,
                        "solution.provenance.detectedNotation",
                        "Recorded detected notation contradicts current " +
                        "instance semantics."));
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        if (classification.PrimaryProblemClass is null)
        {
            aggregate =
                Merge(
                    aggregate,
                    SolutionScientificProvenanceCheckKind.Incomplete);

            diagnostics.Add(
                Diagnostic(
                    "LSDM-PROV-020",
                    SolutionScientificProvenanceDiagnosticSeverity.Warning,
                    "classification.problemClass",
                    "Current instance has no unique canonical problem class."));
        }
        else if (
            !classification.PrimaryProblemClass.Definition.Code.Equals(
                provenance.CanonicalProblemClassCode,
                StringComparison.OrdinalIgnoreCase))
        {
            aggregate =
                Merge(
                    aggregate,
                    SolutionScientificProvenanceCheckKind.Contradiction);

            diagnostics.Add(
                Diagnostic(
                    "LSDM-PROV-021",
                    SolutionScientificProvenanceDiagnosticSeverity.Error,
                    "solution.provenance.problemClassCode",
                    $"Recorded problem class " +
                    $"'{provenance.CanonicalProblemClassCode}' differs from " +
                    $"current class " +
                    $"'{classification.PrimaryProblemClass.Definition.Code}'."));
        }
        else if (
            !classification.PrimaryProblemClass.Kind.ToString().Equals(
                provenance.ProblemClassMatchKind,
                StringComparison.Ordinal))
        {
            aggregate =
                Merge(
                    aggregate,
                    SolutionScientificProvenanceCheckKind.Stale);

            diagnostics.Add(
                Diagnostic(
                    "LSDM-PROV-022",
                    SolutionScientificProvenanceDiagnosticSeverity.Warning,
                    "solution.provenance.problemClassMatchKind",
                    "Canonical problem class is unchanged, but core/extension " +
                    "match status has changed."));
        }

        ScientificFormulationCompatibilityResult compatibility =
            _formulationCompatibilityService.Assess(
                classification,
                provenance.FormulationId);

        switch (compatibility.Kind)
        {
            case ScientificFormulationCompatibilityKind.Compatible:
                break;

            case ScientificFormulationCompatibilityKind.Undetermined:
            case ScientificFormulationCompatibilityKind.Blocked:
                aggregate =
                    Merge(
                        aggregate,
                        SolutionScientificProvenanceCheckKind.Incomplete);

                diagnostics.Add(
                    Diagnostic(
                        "LSDM-PROV-030",
                        SolutionScientificProvenanceDiagnosticSeverity.Warning,
                        "solution.provenance.formulationId",
                        "Current scientific formulation compatibility cannot " +
                        "be established completely."));
                break;

            case ScientificFormulationCompatibilityKind.Incompatible:
                aggregate =
                    Merge(
                        aggregate,
                        SolutionScientificProvenanceCheckKind.Contradiction);

                diagnostics.Add(
                    Diagnostic(
                        "LSDM-PROV-031",
                        SolutionScientificProvenanceDiagnosticSeverity.Error,
                        "solution.provenance.formulationId",
                        "Recorded formulation is scientifically incompatible " +
                        "with the current instance classification."));
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        if (
            compatibility.Profile is not null &&
            !string.IsNullOrWhiteSpace(provenance.FormulationFamily) &&
            !compatibility.Profile.FormulationFamily.Equals(
                provenance.FormulationFamily,
                StringComparison.Ordinal))
        {
            aggregate =
                Merge(
                    aggregate,
                    SolutionScientificProvenanceCheckKind.Stale);

            diagnostics.Add(
                Diagnostic(
                    "LSDM-PROV-032",
                    SolutionScientificProvenanceDiagnosticSeverity.Warning,
                    "solution.provenance.formulationFamily",
                    "The formulation identifier is unchanged but its current " +
                    "scientific family label differs from the recorded one."));
        }

        if (provenance.IsLegacySchema)
        {
            aggregate =
                Merge(
                    aggregate,
                    SolutionScientificProvenanceCheckKind.Stale);

            diagnostics.Add(
                Diagnostic(
                    "LSDM-PROV-040",
                    SolutionScientificProvenanceDiagnosticSeverity.Warning,
                    "solution.provenance.schemaVersion",
                    "Legacy scientific provenance schema v1 has no explicit " +
                    "solution-method/backend evidence."));
        }
        else
        {
            ScientificSolutionMethodDefinition? method =
                ScientificSolutionMethodCatalog.Find(
                    provenance.SolutionMethodId);

            if (method is null)
            {
                aggregate =
                    Merge(
                        aggregate,
                        SolutionScientificProvenanceCheckKind.Incomplete);

                diagnostics.Add(
                    Diagnostic(
                        "LSDM-PROV-041",
                        SolutionScientificProvenanceDiagnosticSeverity.Warning,
                        "solution.provenance.solutionMethodId",
                        $"Recorded solution method " +
                        $"'{provenance.SolutionMethodId}' is not present in " +
                        "the current scientific method catalog."));
            }
            else if (
                classification.PrimaryProblemClass is not null &&
                !method.IsApplicableTo(
                    classification.PrimaryProblemClass.Definition.Id))
            {
                aggregate =
                    Merge(
                        aggregate,
                        SolutionScientificProvenanceCheckKind.Contradiction);

                diagnostics.Add(
                    Diagnostic(
                        "LSDM-PROV-042",
                        SolutionScientificProvenanceDiagnosticSeverity.Error,
                        "solution.provenance.solutionMethodId",
                        $"Recorded solution method '{method.MethodId}' is not " +
                        "applicable to the current canonical problem class."));
            }

            if (
                !Enum.TryParse(
                    provenance.SolverBackendKind,
                    ignoreCase: true,
                    out SolverKind backendKind))
            {
                aggregate =
                    Merge(
                        aggregate,
                        SolutionScientificProvenanceCheckKind.Invalid);

                diagnostics.Add(
                    Diagnostic(
                        "LSDM-PROV-043",
                        SolutionScientificProvenanceDiagnosticSeverity.Error,
                        "solution.provenance.solverBackendKind",
                        $"Recorded solver backend " +
                        $"'{provenance.SolverBackendKind}' is unknown."));
            }
            else if (method is not null)
            {
                ScientificSolverBackendDefinition? backend =
                    ScientificSolverBackendCatalog.Find(
                        backendKind);

                if (
                    method.RequiresMilpBackend &&
                    (
                        backend is null ||
                        !backend.Supports(method)
                    ))
                {
                    aggregate =
                        Merge(
                            aggregate,
                            SolutionScientificProvenanceCheckKind.Contradiction);

                    diagnostics.Add(
                        Diagnostic(
                            "LSDM-PROV-044",
                            SolutionScientificProvenanceDiagnosticSeverity.Error,
                            "solution.provenance.solverBackendKind",
                            $"Recorded backend '{backendKind}' is not " +
                            $"compatible with recorded solution method " +
                            $"'{method.MethodId}'."));
                }
            }
        }

        if (diagnostics.Count == 0)
        {
            diagnostics.Add(
                Diagnostic(
                    "LSDM-PROV-100",
                    SolutionScientificProvenanceDiagnosticSeverity.Information,
                    "solution.provenance",
                    "Recorded scientific provenance is coherent with the " +
                    "current instance and formulation capability catalog."));
        }

        return new SolutionScientificProvenanceCheckResult(
            aggregate,
            provenance,
            classification,
            compatibility,
            diagnostics);
    }

    private static SolutionScientificProvenanceCheckKind Merge(
        SolutionScientificProvenanceCheckKind current,
        SolutionScientificProvenanceCheckKind candidate)
    {
        static int Rank(
            SolutionScientificProvenanceCheckKind value) =>
                value switch
                {
                    SolutionScientificProvenanceCheckKind.Coherent => 0,
                    SolutionScientificProvenanceCheckKind.Missing => 1,
                    SolutionScientificProvenanceCheckKind.Stale => 2,
                    SolutionScientificProvenanceCheckKind.Incomplete => 3,
                    SolutionScientificProvenanceCheckKind.Contradiction => 4,
                    SolutionScientificProvenanceCheckKind.Invalid => 5,
                    _ => throw new ArgumentOutOfRangeException(
                        nameof(value),
                        value,
                        null)
                };

        return Rank(candidate) > Rank(current)
            ? candidate
            : current;
    }

    private static SolutionScientificProvenanceDiagnostic Diagnostic(
        string code,
        SolutionScientificProvenanceDiagnosticSeverity severity,
        string path,
        string message) =>
            new(
                code,
                severity,
                path,
                message);

    private static SolutionScientificProvenanceCheckResult Result(
        SolutionScientificProvenanceCheckKind kind,
        SolutionScientificProvenance? provenance,
        ScientificClassificationResult? classification,
        ScientificFormulationCompatibilityResult? compatibility,
        params SolutionScientificProvenanceDiagnostic[] diagnostics) =>
            new(
                kind,
                provenance,
                classification,
                compatibility,
                diagnostics);
}
