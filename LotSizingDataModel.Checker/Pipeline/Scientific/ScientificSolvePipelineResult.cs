using LotSizingDataModel.Checker.Facade;
using LotSizingDataModel.Checker.Scientific;
using LotSizingDataModel.Instance.Scientific;
using LotSizingDataModel.Solution.Metadata.Scientific;
using LotSizingDataModel.Solver.Execution;
using LotSizingDataModel.Solver.Formulation.Scientific;

namespace LotSizingDataModel.Checker.Pipeline.Scientific;

/// <summary>
/// Keeps every evidence channel of one end-to-end scientific solve separate.
/// </summary>
public sealed class ScientificSolvePipelineResult
{
    internal ScientificSolvePipelineResult(
        ScientificSolvePipelineStatus status,
        ScientificClassificationResult classification,
        ScientificFormulationSelectionResult formulationSelection,
        SolverRunResult? solverRun,
        SolutionScientificProvenance? capturedProvenance,
        LotSizingSolutionVerificationResult? numericalVerification,
        SolutionScientificProvenanceCheckResult? provenanceVerification,
        IEnumerable<ScientificSolvePipelineDiagnostic> diagnostics)
    {
        Status = status;

        Classification =
            classification ??
            throw new ArgumentNullException(nameof(classification));

        FormulationSelection =
            formulationSelection ??
            throw new ArgumentNullException(nameof(formulationSelection));

        SolverRun = solverRun;
        CapturedProvenance = capturedProvenance;
        NumericalVerification = numericalVerification;
        ProvenanceVerification = provenanceVerification;

        Diagnostics =
            diagnostics.ToArray();
    }

    public ScientificSolvePipelineStatus Status { get; }

    public ScientificClassificationResult Classification { get; }

    public ScientificFormulationSelectionResult
        FormulationSelection { get; }

    public SolverRunResult? SolverRun { get; }

    public SolutionScientificProvenance? CapturedProvenance { get; }

    /// <summary>
    /// Existing independent numerical/structural/feasibility verification.
    /// </summary>
    public LotSizingSolutionVerificationResult?
        NumericalVerification { get; }

    /// <summary>
    /// Independent scientific provenance verification.
    /// </summary>
    public SolutionScientificProvenanceCheckResult?
        ProvenanceVerification { get; }

    public IReadOnlyList<ScientificSolvePipelineDiagnostic>
        Diagnostics { get; }

    public bool HasSolution =>
        SolverRun?.HasSolution == true;

    public bool IsNumericallyVerified =>
        NumericalVerification?.IsValid == true;

    public bool IsProvenanceCoherent =>
        ProvenanceVerification?.IsCoherent == true;

    /// <summary>
    /// Strict end-to-end success: the pipeline completed with a solution and
    /// every requested verification channel succeeded.
    /// </summary>
    public bool IsEndToEndCoherent =>
        Status == ScientificSolvePipelineStatus.Completed &&
        HasSolution &&
        (NumericalVerification is null || IsNumericallyVerified) &&
        (ProvenanceVerification is null || IsProvenanceCoherent);
}
