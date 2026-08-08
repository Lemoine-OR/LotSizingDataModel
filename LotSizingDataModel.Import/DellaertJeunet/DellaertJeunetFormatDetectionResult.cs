using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Import.Common;

namespace LotSizingDataModel.Import.DellaertJeunet;

/// <summary>
/// Represents the result of a Dellaert–Jeunet XML format
/// detection operation.
/// </summary>
[Serializable]
[XmlRoot("dellaertJeunetFormatDetectionResult")]
[XmlType(TypeName = "dellaertJeunetFormatDetectionResult")]
public sealed class DellaertJeunetFormatDetectionResult
{
    private string _sourceName =
        string.Empty;

    private string _rootElementName =
        string.Empty;

    private string _rootNamespace =
        string.Empty;

    private int _compatibilityScore;

    /// <summary>
    /// Initializes an empty format-detection result.
    /// </summary>
    public DellaertJeunetFormatDetectionResult()
    {
    }

    /// <summary>
    /// Gets or sets the format evaluated by the detector.
    /// </summary>
    [XmlAttribute("format")]
    public InstanceFormat Format { get; set; } =
        InstanceFormat.DellaertJeunetXml;

    /// <summary>
    /// Gets or sets the optional source name.
    /// </summary>
    [XmlElement("sourceName")]
    public string SourceName
    {
        get =>
            _sourceName;

        set =>
            _sourceName =
                value?.Trim() ??
                string.Empty;
    }

    /// <summary>
    /// Gets or sets the detected root-element name.
    /// </summary>
    [XmlAttribute("rootElementName")]
    public string RootElementName
    {
        get =>
            _rootElementName;

        set =>
            _rootElementName =
                value?.Trim() ??
                string.Empty;
    }

    /// <summary>
    /// Gets or sets the detected root-element namespace.
    /// </summary>
    [XmlElement("rootNamespace")]
    public string RootNamespace
    {
        get =>
            _rootNamespace;

        set =>
            _rootNamespace =
                value?.Trim() ??
                string.Empty;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the expected
    /// root-element name was detected.
    /// </summary>
    [XmlAttribute("hasExpectedRootName")]
    public bool HasExpectedRootName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the expected
    /// empty XML namespace was detected.
    /// </summary>
    [XmlAttribute("hasExpectedNamespace")]
    public bool HasExpectedNamespace { get; set; }

    /// <summary>
    /// Gets or sets the compatibility score between zero and
    /// one hundred.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the supplied score is outside the interval
    /// from zero to one hundred.
    /// </exception>
    [XmlAttribute("compatibilityScore")]
    public int CompatibilityScore
    {
        get =>
            _compatibilityScore;

        set
        {
            if (value < 0 ||
                value > 100)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The compatibility score must be between " +
                    "zero and one hundred.");
            }

            _compatibilityScore =
                value;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the document
    /// appears compatible with the format.
    /// </summary>
    [XmlAttribute("isMatch")]
    public bool IsMatch { get; set; }

    /// <summary>
    /// Gets or sets the number of XML elements inspected.
    /// </summary>
    [XmlAttribute("inspectedElementCount")]
    public int InspectedElementCount { get; set; }

    /// <summary>
    /// Gets or sets the characteristic root-level elements
    /// detected in the document.
    /// </summary>
    [XmlArray("detectedRootElements")]
    [XmlArrayItem("element")]
    public List<string> DetectedRootElements { get; set; } =
        new();

    /// <summary>
    /// Gets or sets the characteristic item-level elements
    /// detected in the document.
    /// </summary>
    [XmlArray("detectedItemElements")]
    [XmlArrayItem("element")]
    public List<string> DetectedItemElements { get; set; } =
        new();

    /// <summary>
    /// Gets the diagnostics produced during detection.
    /// </summary>
    [XmlArray("diagnostics")]
    [XmlArrayItem("diagnostic")]
    public List<ImportDiagnostic> Diagnostics { get; } =
        new();

    /// <summary>
    /// Gets a value indicating whether a root element was
    /// detected.
    /// </summary>
    [XmlIgnore]
    public bool HasRootElement =>
        !string.IsNullOrWhiteSpace(
            RootElementName);

    /// <summary>
    /// Gets a value indicating whether a source name is
    /// available.
    /// </summary>
    [XmlIgnore]
    public bool HasSourceName =>
        !string.IsNullOrWhiteSpace(
            SourceName);

    /// <summary>
    /// Gets a value indicating whether diagnostics exist.
    /// </summary>
    [XmlIgnore]
    public bool HasDiagnostics =>
        Diagnostics.Count > 0;

    /// <summary>
    /// Gets a value indicating whether the detection
    /// operation is complete.
    /// </summary>
    [XmlIgnore]
    public bool IsCompleted { get; private set; }

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
    public void AddDiagnostic(
        ImportDiagnostic diagnostic)
    {
        ArgumentNullException.ThrowIfNull(
            diagnostic);

        Diagnostics.Add(
            diagnostic);
    }

    /// <summary>
    /// Marks the detection result as complete.
    /// </summary>
    public void Complete()
    {
        IsCompleted =
            true;
    }

    /// <summary>
    /// Determines whether the source name should be
    /// serialized.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when a source name exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool ShouldSerializeSourceName()
    {
        return HasSourceName;
    }

    /// <summary>
    /// Determines whether the root namespace should be
    /// serialized.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when a namespace exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool ShouldSerializeRootNamespace()
    {
        return
            !string.IsNullOrWhiteSpace(
                RootNamespace);
    }

    /// <summary>
    /// Determines whether root elements should be serialized.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when at least one root element
    /// was detected; otherwise, <see langword="false"/>.
    /// </returns>
    public bool ShouldSerializeDetectedRootElements()
    {
        return
            DetectedRootElements.Count >
            0;
    }

    /// <summary>
    /// Determines whether item elements should be serialized.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when at least one item element
    /// was detected; otherwise, <see langword="false"/>.
    /// </returns>
    public bool ShouldSerializeDetectedItemElements()
    {
        return
            DetectedItemElements.Count >
            0;
    }

    /// <summary>
    /// Determines whether diagnostics should be serialized.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when diagnostics exist;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool ShouldSerializeDiagnostics()
    {
        return HasDiagnostics;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        string status =
            IsMatch
                ? "Match"
                : "No match";

        return
            $"{status}; {CompatibilityScore}% compatibility; " +
            $"{InspectedElementCount} element(s) inspected; " +
            $"{Diagnostics.Count} diagnostic(s)";
    }
}