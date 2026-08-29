namespace LotSizingDataModel.Solution.Metadata.Scientific;

/// <summary>
/// Dependency-neutral scientific provenance snapshot for one generated
/// lot-sizing solution.
/// </summary>
/// <remarks>
/// Only neutral values are stored so Solution remains independent from
/// Instance and Solver. Persistence uses the existing generation-parameter
/// collection through <see cref="SolutionScientificProvenanceCodec"/>.
/// </remarks>
public sealed class SolutionScientificProvenance
{
    public const string LegacySchemaVersion = "1";

    public const string CurrentSchemaVersion = "2";

    public string SchemaVersion { get; init; } =
        CurrentSchemaVersion;

    public string NotationSchemeId { get; init; } =
        "LSDM";

    public string NotationSchemeVersion { get; init; } =
        "1";

    public string DetectedNotation { get; init; } =
        string.Empty;

    public string CanonicalProblemClassCode { get; init; } =
        string.Empty;

    public string ProblemClassMatchKind { get; init; } =
        string.Empty;

    public string FormulationId { get; init; } =
        string.Empty;

    public string FormulationFamily { get; init; } =
        string.Empty;

    public string FormulationScientificCompatibility { get; init; } =
        string.Empty;

    public string SolutionMethodId { get; init; } =
        string.Empty;

    public string SolutionMethodCategory { get; init; } =
        string.Empty;

    public string SolverBackendKind { get; init; } =
        string.Empty;

    public DateTime CapturedAtUtc { get; init; } =
        DateTime.UtcNow;

    public bool IsLegacySchema =>
        SchemaVersion.Equals(
            LegacySchemaVersion,
            StringComparison.Ordinal);

    public bool HasResolutionMethodEvidence =>
        !string.IsNullOrWhiteSpace(SolutionMethodId) &&
        !string.IsNullOrWhiteSpace(SolutionMethodCategory) &&
        !string.IsNullOrWhiteSpace(SolverBackendKind);

    public bool IsStructurallyComplete =>
        !string.IsNullOrWhiteSpace(SchemaVersion) &&
        !string.IsNullOrWhiteSpace(NotationSchemeId) &&
        !string.IsNullOrWhiteSpace(NotationSchemeVersion) &&
        !string.IsNullOrWhiteSpace(DetectedNotation) &&
        !string.IsNullOrWhiteSpace(CanonicalProblemClassCode) &&
        !string.IsNullOrWhiteSpace(FormulationId) &&
        !string.IsNullOrWhiteSpace(
            FormulationScientificCompatibility) &&
        (
            IsLegacySchema ||
            (
                SchemaVersion.Equals(
                    CurrentSchemaVersion,
                    StringComparison.Ordinal) &&
                HasResolutionMethodEvidence
            )
        );
}
