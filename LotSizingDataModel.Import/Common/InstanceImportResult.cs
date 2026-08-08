using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Instance;

namespace LotSizingDataModel.Import.Common;

/// <summary>
/// Represents the complete result of an external
/// lot-sizing instance import operation.
/// </summary>
/// <remarks>
/// An import result contains:
/// <list type="bullet">
/// <item>
/// <description>
/// the imported <see cref="LotSizingInstance"/>, when one
/// could be created;
/// </description>
/// </item>
/// <item>
/// <description>
/// the detected or explicitly selected source format;
/// </description>
/// </item>
/// <item>
/// <description>
/// the source location and import duration;
/// </description>
/// </item>
/// <item>
/// <description>
/// all informational, warning, error and fatal diagnostics
/// produced during the import workflow.
/// </description>
/// </item>
/// </list>
///
/// A result is considered successful only when it contains an
/// instance and no blocking diagnostic.
/// </remarks>
[Serializable]
[XmlRoot("instanceImportResult")]
[XmlType(TypeName = "instanceImportResult")]
public sealed class InstanceImportResult
{
    private LotSizingInstance? _instance;

    private InstanceFormat _format =
        InstanceFormat.Unknown;

    private string _sourcePath =
        string.Empty;

    private string _importerName =
        string.Empty;

    private string _importerVersion =
        string.Empty;

    private DateTime? _startedAtUtc;

    private DateTime? _completedAtUtc;

    private long _durationMilliseconds;

    /// <summary>
    /// Initializes an empty import result.
    /// </summary>
    /// <remarks>
    /// This constructor is required for XML serialization.
    /// </remarks>
    public InstanceImportResult()
    {
    }

    /// <summary>
    /// Initializes an import result.
    /// </summary>
    /// <param name="format">
    /// Detected or explicitly selected source format.
    /// </param>
    /// <param name="sourcePath">
    /// Optional source file path or source description.
    /// </param>
    public InstanceImportResult(
        InstanceFormat format,
        string sourcePath)
    {
        Format =
            format;

        SourcePath =
            sourcePath;

        StartedAtUtc =
            DateTime.UtcNow;
    }

    /// <summary>
    /// Gets or sets the imported lot-sizing instance.
    /// </summary>
    /// <remarks>
    /// The value may be <see langword="null"/> when the
    /// import fails before a domain instance can be created.
    ///
    /// When partial imports are enabled, the value may contain
    /// an incomplete instance even when blocking diagnostics
    /// exist.
    /// </remarks>
    [XmlElement("instance", IsNullable = true)]
    public LotSizingInstance? Instance
    {
        get => _instance;
        set => _instance = value;
    }

    /// <summary>
    /// Gets or sets the detected or explicitly selected
    /// source format.
    /// </summary>
    [XmlAttribute("format")]
    public InstanceFormat Format
    {
        get => _format;
        set => _format = value;
    }

    /// <summary>
    /// Gets or sets the file path or source description
    /// associated with the import operation.
    /// </summary>
    [XmlElement("sourcePath")]
    public string SourcePath
    {
        get => _sourcePath;

        set =>
            _sourcePath =
                value?.Trim() ??
                string.Empty;
    }

    /// <summary>
    /// Gets or sets the human-readable name of the importer
    /// that produced this result.
    /// </summary>
    [XmlAttribute("importerName")]
    public string ImporterName
    {
        get => _importerName;

        set =>
            _importerName =
                value?.Trim() ??
                string.Empty;
    }

    /// <summary>
    /// Gets or sets the version of the importer that produced
    /// this result.
    /// </summary>
    [XmlAttribute("importerVersion")]
    public string ImporterVersion
    {
        get => _importerVersion;

        set =>
            _importerVersion =
                value?.Trim() ??
                string.Empty;
    }

    /// <summary>
    /// Gets or sets the UTC date and time at which the import
    /// operation started.
    /// </summary>
    [XmlElement("startedAtUtc", IsNullable = true)]
    public DateTime? StartedAtUtc
    {
        get => _startedAtUtc;

        set =>
            _startedAtUtc =
                ConvertToUtc(value);
    }

    /// <summary>
    /// Gets or sets the UTC date and time at which the import
    /// operation completed.
    /// </summary>
    [XmlElement("completedAtUtc", IsNullable = true)]
    public DateTime? CompletedAtUtc
    {
        get => _completedAtUtc;

        set =>
            _completedAtUtc =
                ConvertToUtc(value);
    }

    /// <summary>
    /// Gets or sets the import duration expressed in
    /// milliseconds.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the supplied value is negative.
    /// </exception>
    [XmlAttribute("durationMilliseconds")]
    public long DurationMilliseconds
    {
        get => _durationMilliseconds;

        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The import duration cannot be negative.");
            }

            _durationMilliseconds =
                value;
        }
    }

    /// <summary>
    /// Gets the diagnostics produced during the import
    /// operation.
    /// </summary>
    [XmlArray("diagnostics")]
    [XmlArrayItem("diagnostic")]
    public List<ImportDiagnostic> Diagnostics { get; } =
        new();

    /// <summary>
    /// Gets a value indicating whether an imported instance
    /// is available.
    /// </summary>
    [XmlIgnore]
    public bool HasInstance =>
        Instance is not null;

    /// <summary>
    /// Gets a value indicating whether a source path or source
    /// description has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasSourcePath =>
        !string.IsNullOrWhiteSpace(
            SourcePath);

    /// <summary>
    /// Gets a value indicating whether importer information
    /// has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasImporterInformation =>
        !string.IsNullOrWhiteSpace(
            ImporterName);

    /// <summary>
    /// Gets a value indicating whether an importer version
    /// has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasImporterVersion =>
        !string.IsNullOrWhiteSpace(
            ImporterVersion);

    /// <summary>
    /// Gets a value indicating whether an import start date
    /// has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasStartDate =>
        StartedAtUtc.HasValue;

    /// <summary>
    /// Gets a value indicating whether an import completion
    /// date has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasCompletionDate =>
        CompletedAtUtc.HasValue;

    /// <summary>
    /// Gets the import duration as a
    /// <see cref="TimeSpan"/>.
    /// </summary>
    [XmlIgnore]
    public TimeSpan Duration =>
        TimeSpan.FromMilliseconds(
            DurationMilliseconds);

    /// <summary>
    /// Gets a value indicating whether at least one
    /// diagnostic has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasDiagnostics =>
        Diagnostics.Count > 0;

    /// <summary>
    /// Gets the total number of diagnostics.
    /// </summary>
    [XmlIgnore]
    public int DiagnosticCount =>
        Diagnostics.Count;

    /// <summary>
    /// Gets the number of informational diagnostics.
    /// </summary>
    [XmlIgnore]
    public int InformationCount =>
        Diagnostics.Count(
            diagnostic =>
                diagnostic is not null &&
                diagnostic.IsInformation);

    /// <summary>
    /// Gets the number of warning diagnostics.
    /// </summary>
    [XmlIgnore]
    public int WarningCount =>
        Diagnostics.Count(
            diagnostic =>
                diagnostic is not null &&
                diagnostic.IsWarning);

    /// <summary>
    /// Gets the number of error diagnostics.
    /// </summary>
    [XmlIgnore]
    public int ErrorCount =>
        Diagnostics.Count(
            diagnostic =>
                diagnostic is not null &&
                diagnostic.IsError);

    /// <summary>
    /// Gets the number of fatal diagnostics.
    /// </summary>
    [XmlIgnore]
    public int FatalCount =>
        Diagnostics.Count(
            diagnostic =>
                diagnostic is not null &&
                diagnostic.IsFatal);

    /// <summary>
    /// Gets a value indicating whether at least one warning
    /// has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasWarnings =>
        WarningCount > 0;

    /// <summary>
    /// Gets a value indicating whether at least one error has
    /// been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasErrors =>
        ErrorCount > 0;

    /// <summary>
    /// Gets a value indicating whether at least one fatal
    /// diagnostic has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasFatalErrors =>
        FatalCount > 0;

    /// <summary>
    /// Gets a value indicating whether at least one blocking
    /// diagnostic has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasBlockingDiagnostics =>
        Diagnostics.Any(
            diagnostic =>
                diagnostic is not null &&
                diagnostic.IsBlocking);

    /// <summary>
    /// Gets a value indicating whether the import operation
    /// completed successfully.
    /// </summary>
    /// <remarks>
    /// A successful import requires:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// a non-null imported instance;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// no error or fatal diagnostic.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    [XmlIgnore]
    public bool IsSuccessful =>
        HasInstance &&
        !HasBlockingDiagnostics;

    /// <summary>
    /// Gets a value indicating whether the result contains a
    /// partially imported instance together with blocking
    /// diagnostics.
    /// </summary>
    [XmlIgnore]
    public bool IsPartial =>
        HasInstance &&
        HasBlockingDiagnostics;

    /// <summary>
    /// Gets a value indicating whether the import failed
    /// without producing an instance.
    /// </summary>
    [XmlIgnore]
    public bool HasFailed =>
        !HasInstance &&
        HasBlockingDiagnostics;

    /// <summary>
    /// Gets a value indicating whether the import operation
    /// has been completed.
    /// </summary>
    [XmlIgnore]
    public bool IsCompleted =>
        CompletedAtUtc.HasValue;

    /// <summary>
    /// Adds a diagnostic to the result.
    /// </summary>
    /// <param name="diagnostic">
    /// Diagnostic to add.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="diagnostic"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the diagnostic is structurally invalid.
    /// </exception>
    public void AddDiagnostic(
        ImportDiagnostic diagnostic)
    {
        ArgumentNullException.ThrowIfNull(
            diagnostic);

        if (!diagnostic.IsValid)
        {
            throw new ArgumentException(
                "The import diagnostic is invalid.",
                nameof(diagnostic));
        }

        Diagnostics.Add(
            diagnostic);
    }

    /// <summary>
    /// Adds several diagnostics to the result.
    /// </summary>
    /// <param name="diagnostics">
    /// Diagnostics to add.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="diagnostics"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the collection contains a null or invalid
    /// diagnostic.
    /// </exception>
    public void AddDiagnostics(
        IEnumerable<ImportDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(
            diagnostics);

        ImportDiagnostic[] materializedDiagnostics =
            diagnostics.ToArray();

        if (materializedDiagnostics.Any(
                diagnostic =>
                    diagnostic is null))
        {
            throw new ArgumentException(
                "The diagnostic collection cannot contain " +
                "a null element.",
                nameof(diagnostics));
        }

        ImportDiagnostic? invalidDiagnostic =
            materializedDiagnostics.FirstOrDefault(
                diagnostic =>
                    !diagnostic.IsValid);

        if (invalidDiagnostic is not null)
        {
            throw new ArgumentException(
                $"Diagnostic '{invalidDiagnostic.Code}' is " +
                "invalid.",
                nameof(diagnostics));
        }

        Diagnostics.AddRange(
            materializedDiagnostics);
    }

    /// <summary>
    /// Removes all diagnostics from the result.
    /// </summary>
    public void ClearDiagnostics()
    {
        Diagnostics.Clear();
    }

    /// <summary>
    /// Returns diagnostics having the supplied severity.
    /// </summary>
    /// <param name="severity">
    /// Severity to select.
    /// </param>
    /// <returns>
    /// Matching diagnostics.
    /// </returns>
    public IReadOnlyList<ImportDiagnostic>
        GetDiagnostics(
            ImportSeverity severity)
    {
        return Diagnostics
            .Where(
                diagnostic =>
                    diagnostic is not null &&
                    diagnostic.Severity == severity)
            .ToArray();
    }

    /// <summary>
    /// Returns the first blocking diagnostic.
    /// </summary>
    /// <returns>
    /// First blocking diagnostic, or
    /// <see langword="null"/> when none exists.
    /// </returns>
    public ImportDiagnostic? GetFirstBlockingDiagnostic()
    {
        return Diagnostics
            .FirstOrDefault(
                diagnostic =>
                    diagnostic is not null &&
                    diagnostic.IsBlocking);
    }

    /// <summary>
    /// Marks the import operation as completed and calculates
    /// its duration.
    /// </summary>
    public void Complete()
    {
        DateTime completedAtUtc =
            DateTime.UtcNow;

        CompletedAtUtc =
            completedAtUtc;

        if (!StartedAtUtc.HasValue)
        {
            StartedAtUtc =
                completedAtUtc;

            DurationMilliseconds =
                0;

            return;
        }

        TimeSpan duration =
            completedAtUtc -
            StartedAtUtc.Value;

        DurationMilliseconds =
            Math.Max(
                0L,
                Convert.ToInt64(
                    Math.Round(
                        duration.TotalMilliseconds,
                        MidpointRounding.AwayFromZero)));
    }

    /// <summary>
    /// Marks the import operation as completed using an
    /// explicitly supplied duration.
    /// </summary>
    /// <param name="duration">
    /// Import duration.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="duration"/> is negative.
    /// </exception>
    public void Complete(
        TimeSpan duration)
    {
        if (duration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(duration),
                duration,
                "The import duration cannot be negative.");
        }

        CompletedAtUtc =
            DateTime.UtcNow;

        DurationMilliseconds =
            Convert.ToInt64(
                Math.Round(
                    duration.TotalMilliseconds,
                    MidpointRounding.AwayFromZero));
    }

    /// <summary>
    /// Validates the structural consistency of the result.
    /// </summary>
    /// <returns>
    /// Ordered validation-error collection. An empty
    /// collection indicates that the result is valid.
    /// </returns>
    public IReadOnlyList<string> Validate()
    {
        var errors =
            new List<string>();

        for (int index = 0;
             index < Diagnostics.Count;
             index++)
        {
            ImportDiagnostic? diagnostic =
                Diagnostics[index];

            if (diagnostic is null)
            {
                errors.Add(
                    $"Diagnostic at index {index} is null.");

                continue;
            }

            if (!diagnostic.IsValid)
            {
                errors.Add(
                    $"Diagnostic '{diagnostic.Code}' is " +
                    "invalid.");
            }
        }

        if (DurationMilliseconds < 0)
        {
            errors.Add(
                "The import duration cannot be negative.");
        }

        if (StartedAtUtc.HasValue &&
            CompletedAtUtc.HasValue &&
            CompletedAtUtc.Value <
            StartedAtUtc.Value)
        {
            errors.Add(
                "The import completion date precedes the " +
                "start date.");
        }

        if (IsSuccessful &&
            Format == InstanceFormat.Unknown)
        {
            errors.Add(
                "A successful import must identify the " +
                "source format.");
        }

        return errors
            .Where(
                error =>
                    !string.IsNullOrWhiteSpace(error))
            .Select(
                error =>
                    error.Trim())
            .Distinct(
                StringComparer.Ordinal)
            .OrderBy(
                error =>
                    error,
                StringComparer.Ordinal)
            .ToArray();
    }

    /// <summary>
    /// Validates the result and throws an exception when it is
    /// structurally inconsistent.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when at least one validation error exists.
    /// </exception>
    public void EnsureValid()
    {
        IReadOnlyList<string> errors =
            Validate();

        if (errors.Count == 0)
        {
            return;
        }

        throw new InvalidOperationException(
            "The instance import result is invalid:" +
            Environment.NewLine +
            string.Join(
                Environment.NewLine,
                errors.Select(
                    error =>
                        "- " + error)));
    }

    /// <summary>
    /// Determines whether the imported instance must be
    /// serialized.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when an instance exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool ShouldSerializeInstance()
    {
        return HasInstance;
    }

    /// <summary>
    /// Determines whether the source path must be serialized.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when a source path exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool ShouldSerializeSourcePath()
    {
        return HasSourcePath;
    }

    /// <summary>
    /// Determines whether importer information must be
    /// serialized.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when importer information
    /// exists; otherwise, <see langword="false"/>.
    /// </returns>
    public bool ShouldSerializeImporterName()
    {
        return HasImporterInformation;
    }

    /// <summary>
    /// Determines whether the importer version must be
    /// serialized.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when an importer version exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool ShouldSerializeImporterVersion()
    {
        return HasImporterVersion;
    }

    /// <summary>
    /// Determines whether the import start date must be
    /// serialized.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when a start date exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool ShouldSerializeStartedAtUtc()
    {
        return HasStartDate;
    }

    /// <summary>
    /// Determines whether the import completion date must be
    /// serialized.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when a completion date exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool ShouldSerializeCompletedAtUtc()
    {
        return HasCompletionDate;
    }

    /// <summary>
    /// Determines whether diagnostics must be serialized.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when at least one diagnostic
    /// exists; otherwise, <see langword="false"/>.
    /// </returns>
    public bool ShouldSerializeDiagnostics()
    {
        return HasDiagnostics;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        string status =
            IsSuccessful
                ? "Successful"
                : IsPartial
                    ? "Partial"
                    : HasFailed
                        ? "Failed"
                        : "Incomplete";

        return
            $"{status} import; " +
            $"format {Format}; " +
            $"{DiagnosticCount.ToString(
                CultureInfo.InvariantCulture)} diagnostic(s); " +
            $"{DurationMilliseconds.ToString(
                CultureInfo.InvariantCulture)} ms";
    }

    private static DateTime? ConvertToUtc(
        DateTime? value)
    {
        if (!value.HasValue)
        {
            return null;
        }

        DateTime dateTimeValue =
            value.Value;

        return dateTimeValue.Kind switch
        {
            DateTimeKind.Utc =>
                dateTimeValue,

            DateTimeKind.Local =>
                dateTimeValue.ToUniversalTime(),

            _ =>
                DateTime.SpecifyKind(
                    dateTimeValue,
                    DateTimeKind.Utc)
        };
    }
}