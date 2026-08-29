using System.Globalization;
using LotSizingDataModel.Solution.Common;
using LotSizingDataModel.Solution.Metadata;

namespace LotSizingDataModel.Solution.Metadata.Scientific;

/// <summary>
/// Stores and reads scientific provenance through the existing serializable
/// <see cref="SolutionGenerationMetadata.Parameters"/> collection.
/// </summary>
public static class SolutionScientificProvenanceCodec
{
    public const string ParameterPrefix =
        "lsdm.scientific.";

    public const string SchemaVersionParameter =
        ParameterPrefix + "schemaVersion";

    public const string NotationSchemeIdParameter =
        ParameterPrefix + "notationSchemeId";

    public const string NotationSchemeVersionParameter =
        ParameterPrefix + "notationSchemeVersion";

    public const string DetectedNotationParameter =
        ParameterPrefix + "detectedNotation";

    public const string ProblemClassCodeParameter =
        ParameterPrefix + "problemClassCode";

    public const string ProblemClassMatchKindParameter =
        ParameterPrefix + "problemClassMatchKind";

    public const string FormulationIdParameter =
        ParameterPrefix + "formulationId";

    public const string FormulationFamilyParameter =
        ParameterPrefix + "formulationFamily";

    public const string FormulationCompatibilityParameter =
        ParameterPrefix + "formulationCompatibility";

    public const string CapturedAtUtcParameter =
        ParameterPrefix + "capturedAtUtc";

    public static IReadOnlyList<string> ReservedParameterNames { get; } =
        new[]
        {
            SchemaVersionParameter,
            NotationSchemeIdParameter,
            NotationSchemeVersionParameter,
            DetectedNotationParameter,
            ProblemClassCodeParameter,
            ProblemClassMatchKindParameter,
            FormulationIdParameter,
            FormulationFamilyParameter,
            FormulationCompatibilityParameter,
            CapturedAtUtcParameter
        };

    public static bool IsReservedParameterName(
        string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        return ReservedParameterNames.Contains(
            name.Trim(),
            StringComparer.OrdinalIgnoreCase);
    }

    public static void Write(
        SolutionGenerationMetadata metadata,
        SolutionScientificProvenance provenance)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(provenance);

        if (!provenance.IsStructurallyComplete)
        {
            throw new ArgumentException(
                "Scientific provenance is structurally incomplete.",
                nameof(provenance));
        }

        if (
            !provenance.SchemaVersion.Equals(
                SolutionScientificProvenance.CurrentSchemaVersion,
                StringComparison.Ordinal))
        {
            throw new NotSupportedException(
                $"Scientific provenance schema " +
                $"'{provenance.SchemaVersion}' is not supported for writing.");
        }

        Set(metadata, SchemaVersionParameter, provenance.SchemaVersion);
        Set(metadata, NotationSchemeIdParameter, provenance.NotationSchemeId);
        Set(
            metadata,
            NotationSchemeVersionParameter,
            provenance.NotationSchemeVersion);
        Set(metadata, DetectedNotationParameter, provenance.DetectedNotation);
        Set(
            metadata,
            ProblemClassCodeParameter,
            provenance.CanonicalProblemClassCode);
        Set(
            metadata,
            ProblemClassMatchKindParameter,
            provenance.ProblemClassMatchKind);
        Set(metadata, FormulationIdParameter, provenance.FormulationId);
        Set(
            metadata,
            FormulationFamilyParameter,
            provenance.FormulationFamily);
        Set(
            metadata,
            FormulationCompatibilityParameter,
            provenance.FormulationScientificCompatibility);
        Set(
            metadata,
            CapturedAtUtcParameter,
            provenance.CapturedAtUtc
                .ToUniversalTime()
                .ToString(
                    "O",
                    CultureInfo.InvariantCulture));
    }

    public static SolutionScientificProvenanceReadResult Read(
        SolutionGenerationMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        bool hasAny =
            metadata.Parameters.Any(
                parameter =>
                    parameter.Name.StartsWith(
                        ParameterPrefix,
                        StringComparison.OrdinalIgnoreCase));

        if (!hasAny)
        {
            return new SolutionScientificProvenanceReadResult(
                SolutionScientificProvenanceReadKind.Missing,
                provenance: null,
                diagnostic:
                    "No LotSizingDataModel scientific provenance is recorded.");
        }

        string? schema =
            Get(metadata, SchemaVersionParameter);

        if (string.IsNullOrWhiteSpace(schema))
        {
            return Invalid(
                "Scientific provenance is missing schemaVersion.");
        }

        if (
            !schema.Equals(
                SolutionScientificProvenance.CurrentSchemaVersion,
                StringComparison.Ordinal))
        {
            return Invalid(
                $"Unsupported scientific provenance schema '{schema}'.");
        }

        string? capturedText =
            Get(metadata, CapturedAtUtcParameter);

        if (
            string.IsNullOrWhiteSpace(capturedText) ||
            !DateTime.TryParse(
                capturedText,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out DateTime capturedAtUtc))
        {
            return Invalid(
                "Scientific provenance contains an invalid capturedAtUtc.");
        }

        var provenance =
            new SolutionScientificProvenance
            {
                SchemaVersion = schema,
                NotationSchemeId =
                    Get(metadata, NotationSchemeIdParameter) ??
                    string.Empty,
                NotationSchemeVersion =
                    Get(metadata, NotationSchemeVersionParameter) ??
                    string.Empty,
                DetectedNotation =
                    Get(metadata, DetectedNotationParameter) ??
                    string.Empty,
                CanonicalProblemClassCode =
                    Get(metadata, ProblemClassCodeParameter) ??
                    string.Empty,
                ProblemClassMatchKind =
                    Get(metadata, ProblemClassMatchKindParameter) ??
                    string.Empty,
                FormulationId =
                    Get(metadata, FormulationIdParameter) ??
                    string.Empty,
                FormulationFamily =
                    Get(metadata, FormulationFamilyParameter) ??
                    string.Empty,
                FormulationScientificCompatibility =
                    Get(metadata, FormulationCompatibilityParameter) ??
                    string.Empty,
                CapturedAtUtc =
                    capturedAtUtc.ToUniversalTime()
            };

        if (!provenance.IsStructurallyComplete)
        {
            return Invalid(
                "Scientific provenance is structurally incomplete.");
        }

        return new SolutionScientificProvenanceReadResult(
            SolutionScientificProvenanceReadKind.Valid,
            provenance,
            diagnostic: string.Empty);
    }

    public static bool Clear(
        SolutionGenerationMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        bool removedAny = false;

        foreach (
            string parameterName
            in ReservedParameterNames)
        {
            removedAny =
                metadata.RemoveParameter(parameterName) ||
                removedAny;
        }

        return removedAny;
    }

    private static void Set(
        SolutionGenerationMetadata metadata,
        string name,
        string value)
    {
        metadata.SetParameter(
            AlgorithmParameter.FromString(
                name,
                value ?? string.Empty,
                "LotSizingDataModel scientific solution provenance."));
    }

    private static string? Get(
        SolutionGenerationMetadata metadata,
        string name) =>
            metadata.FindParameter(name)?.Value;

    private static SolutionScientificProvenanceReadResult Invalid(
        string diagnostic) =>
            new(
                SolutionScientificProvenanceReadKind.Invalid,
                provenance: null,
                diagnostic);
}
