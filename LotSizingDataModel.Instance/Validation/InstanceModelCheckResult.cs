using System.Collections.ObjectModel;

namespace LotSizingDataModel.Instance.Validation;

/// <summary>
/// Represents the complete result of semantic instance validation.
/// </summary>
public sealed class InstanceModelCheckResult
{
    private readonly IReadOnlyList<InstanceDiagnostic> _diagnostics;

    internal InstanceModelCheckResult(
        IEnumerable<InstanceDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);

        InstanceDiagnostic[] materialized =
            diagnostics.ToArray();

        _diagnostics =
            new ReadOnlyCollection<InstanceDiagnostic>(
                materialized);

        HasBlockingIssues =
            materialized.Any(
                diagnostic => diagnostic.IsBlocking);

        HasWarnings =
            materialized.Any(
                diagnostic =>
                    diagnostic.Severity ==
                    InstanceDiagnosticSeverity.Warning);

        Capabilities =
            new InstanceValidationCapabilities(
                canSaveDraft: true,
                canValidate: true,
                canClassify: !HasBlockingIssues,
                canGenerateNotation: !HasBlockingIssues,
                canSolve: !HasBlockingIssues,
                canExportAsValidatedInstance:
                    !HasBlockingIssues);
    }

    /// <summary>
    /// Gets all diagnostics in deterministic evaluation order.
    /// </summary>
    public IReadOnlyList<InstanceDiagnostic> Diagnostics =>
        _diagnostics;

    /// <summary>
    /// Gets a value indicating whether at least one Error or
    /// Fatal diagnostic exists.
    /// </summary>
    public bool HasBlockingIssues { get; }

    /// <summary>
    /// Gets a value indicating whether at least one warning exists.
    /// </summary>
    public bool HasWarnings { get; }

    /// <summary>
    /// Gets a value indicating whether the instance has no
    /// blocking structural or semantic issue.
    /// </summary>
    public bool IsValid => !HasBlockingIssues;

    /// <summary>
    /// Gets downstream-operation capabilities derived from the
    /// validation result.
    /// </summary>
    public InstanceValidationCapabilities Capabilities { get; }

    /// <summary>
    /// Gets the number of blocking diagnostics.
    /// </summary>
    public int BlockingIssueCount =>
        Diagnostics.Count(
            diagnostic => diagnostic.IsBlocking);

    /// <summary>
    /// Gets the number of warnings.
    /// </summary>
    public int WarningCount =>
        Diagnostics.Count(
            diagnostic =>
                diagnostic.Severity ==
                InstanceDiagnosticSeverity.Warning);
}
