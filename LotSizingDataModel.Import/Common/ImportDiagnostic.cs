using System;
using System.Globalization;
using System.Xml.Serialization;

namespace LotSizingDataModel.Import.Common;

/// <summary>
/// Represents one diagnostic message produced during an
/// instance import operation.
/// </summary>
/// <remarks>
/// A diagnostic may describe:
/// <list type="bullet">
/// <item>
/// <description>
/// an informational event;
/// </description>
/// </item>
/// <item>
/// <description>
/// a non-blocking warning;
/// </description>
/// </item>
/// <item>
/// <description>
/// a recoverable import error;
/// </description>
/// </item>
/// <item>
/// <description>
/// a fatal condition that prevents the import from
/// continuing.
/// </description>
/// </item>
/// </list>
///
/// Diagnostics should use stable codes so that client
/// applications can filter, display or test specific import
/// conditions independently from the human-readable message.
/// </remarks>
[Serializable]
[XmlType(TypeName = "importDiagnostic")]
public sealed class ImportDiagnostic
{
    private string _code =
        string.Empty;

    private string _message =
        string.Empty;

    private string _sourcePath =
        string.Empty;

    private string _entityKey =
        string.Empty;

    private string _exceptionType =
        string.Empty;

    private string _technicalDetails =
        string.Empty;

    /// <summary>
    /// Initializes an empty import diagnostic.
    /// </summary>
    /// <remarks>
    /// This constructor is required for XML serialization.
    /// </remarks>
    public ImportDiagnostic()
    {
    }

    /// <summary>
    /// Initializes an import diagnostic.
    /// </summary>
    /// <param name="severity">
    /// Diagnostic severity.
    /// </param>
    /// <param name="code">
    /// Stable diagnostic code.
    /// </param>
    /// <param name="message">
    /// Human-readable diagnostic message.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="code"/> or
    /// <paramref name="message"/> is empty.
    /// </exception>
    public ImportDiagnostic(
        ImportSeverity severity,
        string code,
        string message)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException(
                "A diagnostic code is required.",
                nameof(code));
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException(
                "A diagnostic message is required.",
                nameof(message));
        }

        Severity =
            severity;

        Code =
            code;

        Message =
            message;
    }

    /// <summary>
    /// Initializes an import diagnostic associated with a
    /// source location.
    /// </summary>
    /// <param name="severity">
    /// Diagnostic severity.
    /// </param>
    /// <param name="code">
    /// Stable diagnostic code.
    /// </param>
    /// <param name="message">
    /// Human-readable diagnostic message.
    /// </param>
    /// <param name="sourcePath">
    /// Optional path identifying the source location.
    /// </param>
    /// <param name="lineNumber">
    /// Optional one-based source line number.
    /// </param>
    /// <param name="linePosition">
    /// Optional one-based source-column position.
    /// </param>
    public ImportDiagnostic(
        ImportSeverity severity,
        string code,
        string message,
        string sourcePath,
        int? lineNumber = null,
        int? linePosition = null)
        : this(
            severity,
            code,
            message)
    {
        SourcePath =
            sourcePath;

        LineNumber =
            lineNumber;

        LinePosition =
            linePosition;
    }

    /// <summary>
    /// Gets or sets the diagnostic severity.
    /// </summary>
    [XmlAttribute("severity")]
    public ImportSeverity Severity { get; set; }

    /// <summary>
    /// Gets or sets the stable diagnostic code.
    /// </summary>
    /// <remarks>
    /// Recommended code families include:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// <c>IMP</c> for generic import diagnostics;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <c>DJ</c> for Dellaert–Jeunet diagnostics;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// format-specific prefixes for future importers.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    [XmlAttribute("code")]
    public string Code
    {
        get => _code;
        set =>
            _code =
                value?.Trim() ??
                string.Empty;
    }

    /// <summary>
    /// Gets or sets the human-readable diagnostic message.
    /// </summary>
    [XmlElement("message")]
    public string Message
    {
        get => _message;
        set =>
            _message =
                value?.Trim() ??
                string.Empty;
    }

    /// <summary>
    /// Gets or sets an optional path identifying the source
    /// location associated with the diagnostic.
    /// </summary>
    /// <remarks>
    /// Examples include:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// a file path;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// an XML path such as
    /// <c>/Instance/Items/Item[12]/Demand</c>;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// a CSV row or JSON property path.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
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
    /// Gets or sets an optional one-based source line number.
    /// </summary>
    [XmlAttribute("lineNumber")]
    public int? LineNumber { get; set; }

    /// <summary>
    /// Gets or sets an optional one-based source-column
    /// position.
    /// </summary>
    [XmlAttribute("linePosition")]
    public int? LinePosition { get; set; }

    /// <summary>
    /// Gets or sets an optional key identifying the affected
    /// source or domain entity.
    /// </summary>
    /// <remarks>
    /// Examples include an item identifier, a component
    /// relationship key or an instance identifier.
    /// </remarks>
    [XmlAttribute("entityKey")]
    public string EntityKey
    {
        get => _entityKey;
        set =>
            _entityKey =
                value?.Trim() ??
                string.Empty;
    }

    /// <summary>
    /// Gets or sets the type name of an exception associated
    /// with the diagnostic.
    /// </summary>
    [XmlAttribute("exceptionType")]
    public string ExceptionType
    {
        get => _exceptionType;
        set =>
            _exceptionType =
                value?.Trim() ??
                string.Empty;
    }

    /// <summary>
    /// Gets or sets optional technical details intended for
    /// logs or advanced diagnostics.
    /// </summary>
    /// <remarks>
    /// This property should not contain sensitive data.
    /// </remarks>
    [XmlElement("technicalDetails")]
    public string TechnicalDetails
    {
        get => _technicalDetails;
        set =>
            _technicalDetails =
                value?.Trim() ??
                string.Empty;
    }

    /// <summary>
    /// Gets a value indicating whether the diagnostic has a
    /// non-empty code.
    /// </summary>
    [XmlIgnore]
    public bool HasCode =>
        !string.IsNullOrWhiteSpace(
            Code);

    /// <summary>
    /// Gets a value indicating whether the diagnostic has a
    /// non-empty human-readable message.
    /// </summary>
    [XmlIgnore]
    public bool HasMessage =>
        !string.IsNullOrWhiteSpace(
            Message);

    /// <summary>
    /// Gets a value indicating whether a source path has been
    /// recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasSourcePath =>
        !string.IsNullOrWhiteSpace(
            SourcePath);

    /// <summary>
    /// Gets a value indicating whether a source location has
    /// been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasSourceLocation =>
        LineNumber.HasValue ||
        LinePosition.HasValue;

    /// <summary>
    /// Gets a value indicating whether an entity key has been
    /// recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasEntityKey =>
        !string.IsNullOrWhiteSpace(
            EntityKey);

    /// <summary>
    /// Gets a value indicating whether exception information
    /// has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasExceptionInformation =>
        !string.IsNullOrWhiteSpace(
            ExceptionType);

    /// <summary>
    /// Gets a value indicating whether technical details have
    /// been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasTechnicalDetails =>
        !string.IsNullOrWhiteSpace(
            TechnicalDetails);

    /// <summary>
    /// Gets a value indicating whether the diagnostic is
    /// informational.
    /// </summary>
    [XmlIgnore]
    public bool IsInformation =>
        Severity ==
        ImportSeverity.Information;

    /// <summary>
    /// Gets a value indicating whether the diagnostic is a
    /// warning.
    /// </summary>
    [XmlIgnore]
    public bool IsWarning =>
        Severity ==
        ImportSeverity.Warning;

    /// <summary>
    /// Gets a value indicating whether the diagnostic is an
    /// error.
    /// </summary>
    [XmlIgnore]
    public bool IsError =>
        Severity ==
        ImportSeverity.Error;

    /// <summary>
    /// Gets a value indicating whether the diagnostic is
    /// fatal.
    /// </summary>
    [XmlIgnore]
    public bool IsFatal =>
        Severity ==
        ImportSeverity.Fatal;

    /// <summary>
    /// Gets a value indicating whether the diagnostic blocks
    /// a successful import.
    /// </summary>
    [XmlIgnore]
    public bool IsBlocking =>
        Severity is
            ImportSeverity.Error or
            ImportSeverity.Fatal;

    /// <summary>
    /// Gets a value indicating whether the diagnostic is
    /// structurally valid.
    /// </summary>
    [XmlIgnore]
    public bool IsValid =>
        HasCode &&
        HasMessage &&
        ValidateSourceLocation();

    /// <summary>
    /// Creates an informational diagnostic.
    /// </summary>
    /// <param name="code">
    /// Stable diagnostic code.
    /// </param>
    /// <param name="message">
    /// Human-readable message.
    /// </param>
    /// <returns>
    /// Newly created informational diagnostic.
    /// </returns>
    public static ImportDiagnostic Information(
        string code,
        string message)
    {
        return new ImportDiagnostic(
            ImportSeverity.Information,
            code,
            message);
    }

    /// <summary>
    /// Creates a warning diagnostic.
    /// </summary>
    /// <param name="code">
    /// Stable diagnostic code.
    /// </param>
    /// <param name="message">
    /// Human-readable message.
    /// </param>
    /// <returns>
    /// Newly created warning diagnostic.
    /// </returns>
    public static ImportDiagnostic Warning(
        string code,
        string message)
    {
        return new ImportDiagnostic(
            ImportSeverity.Warning,
            code,
            message);
    }

    /// <summary>
    /// Creates an error diagnostic.
    /// </summary>
    /// <param name="code">
    /// Stable diagnostic code.
    /// </param>
    /// <param name="message">
    /// Human-readable message.
    /// </param>
    /// <returns>
    /// Newly created error diagnostic.
    /// </returns>
    public static ImportDiagnostic Error(
        string code,
        string message)
    {
        return new ImportDiagnostic(
            ImportSeverity.Error,
            code,
            message);
    }

    /// <summary>
    /// Creates a fatal diagnostic.
    /// </summary>
    /// <param name="code">
    /// Stable diagnostic code.
    /// </param>
    /// <param name="message">
    /// Human-readable message.
    /// </param>
    /// <returns>
    /// Newly created fatal diagnostic.
    /// </returns>
    public static ImportDiagnostic Fatal(
        string code,
        string message)
    {
        return new ImportDiagnostic(
            ImportSeverity.Fatal,
            code,
            message);
    }

    /// <summary>
    /// Creates a diagnostic from an exception.
    /// </summary>
    /// <param name="severity">
    /// Diagnostic severity.
    /// </param>
    /// <param name="code">
    /// Stable diagnostic code.
    /// </param>
    /// <param name="message">
    /// Human-readable diagnostic message.
    /// </param>
    /// <param name="exception">
    /// Exception associated with the diagnostic.
    /// </param>
    /// <returns>
    /// Newly created diagnostic containing exception
    /// metadata.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="exception"/> is
    /// <see langword="null"/>.
    /// </exception>
    public static ImportDiagnostic FromException(
        ImportSeverity severity,
        string code,
        string message,
        Exception exception)
    {
        ArgumentNullException.ThrowIfNull(
            exception);

        return new ImportDiagnostic(
            severity,
            code,
            message)
        {
            ExceptionType =
                exception.GetType().FullName ??
                exception.GetType().Name,

            TechnicalDetails =
                exception.Message
        };
    }

    /// <summary>
    /// Determines whether the diagnostic should be
    /// serialized with a source path.
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
    /// Determines whether the diagnostic should be
    /// serialized with a line number.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when a line number exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool ShouldSerializeLineNumber()
    {
        return LineNumber.HasValue;
    }

    /// <summary>
    /// Determines whether the diagnostic should be
    /// serialized with a line position.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when a line position exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool ShouldSerializeLinePosition()
    {
        return LinePosition.HasValue;
    }

    /// <summary>
    /// Determines whether the diagnostic should be
    /// serialized with an entity key.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when an entity key exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool ShouldSerializeEntityKey()
    {
        return HasEntityKey;
    }

    /// <summary>
    /// Determines whether the diagnostic should be
    /// serialized with exception information.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when exception information
    /// exists; otherwise, <see langword="false"/>.
    /// </returns>
    public bool ShouldSerializeExceptionType()
    {
        return HasExceptionInformation;
    }

    /// <summary>
    /// Determines whether the diagnostic should be
    /// serialized with technical details.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when technical details exist;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool ShouldSerializeTechnicalDetails()
    {
        return HasTechnicalDetails;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        string location =
            BuildLocationDescription();

        string prefix =
            string.IsNullOrWhiteSpace(location)
                ? string.Empty
                : location + ": ";

        return
            $"{Severity} {Code}: {prefix}{Message}";
    }

    private bool ValidateSourceLocation()
    {
        bool lineNumberIsValid =
            !LineNumber.HasValue ||
            LineNumber.Value > 0;

        bool linePositionIsValid =
            !LinePosition.HasValue ||
            LinePosition.Value > 0;

        return
            lineNumberIsValid &&
            linePositionIsValid;
    }

    private string BuildLocationDescription()
    {
        string sourceDescription =
            HasSourcePath
                ? SourcePath
                : string.Empty;

        if (!LineNumber.HasValue)
        {
            return sourceDescription;
        }

        string lineDescription =
            LineNumber.Value.ToString(
                CultureInfo.InvariantCulture);

        if (LinePosition.HasValue)
        {
            lineDescription +=
                ":" +
                LinePosition.Value.ToString(
                    CultureInfo.InvariantCulture);
        }

        if (string.IsNullOrWhiteSpace(
                sourceDescription))
        {
            return lineDescription;
        }

        return
            sourceDescription +
            " (" +
            lineDescription +
            ")";
    }
}