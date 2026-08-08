using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Import.Common;

/// <summary>
/// Defines the common options applied when importing an
/// external lot-sizing problem instance.
/// </summary>
/// <remarks>
/// These options are format-independent.
///
/// A specialized importer may derive from this class to add
/// format-specific behavior while preserving the common
/// import workflow.
/// </remarks>
[Serializable]
[XmlType(TypeName = "instanceImportOptions")]
public class InstanceImportOptions
{
    private bool _analyzeProductStructure =
        true;

    private bool _classifyProblem =
        true;

    private bool _generateMethodRecommendations;

    private bool _validateSourceData =
        true;

    private bool _validateImportedInstance =
        true;

    private bool _throwOnError;

    private bool _preserveSourceIdentifiers =
        true;

    private bool _includeInformationDiagnostics =
        true;

    private bool _includeTechnicalDetails;

    private bool _stopOnFirstError;

    private bool _allowPartialImport;

    private bool _normalizeTextValues =
        true;

    private bool _trimIdentifiers =
        true;

    private bool _rejectDuplicateIdentifiers =
        true;

    private bool _rejectMissingReferences =
        true;

    private bool _rejectNegativeValues =
        true;

    private bool _rejectInvalidTimeSeriesLength =
        true;

    private bool _detectFormatFromContent =
        true;

    private string _sourceName =
        string.Empty;

    private string _instanceIdOverride =
        string.Empty;

    private string _instanceNameOverride =
        string.Empty;

    private string _createdByOverride =
        string.Empty;

    private string _sourceInformationOverride =
        string.Empty;

    /// <summary>
    /// Initializes a new instance of the import options class
    /// with the recommended default values.
    /// </summary>
    public InstanceImportOptions()
    {
    }

    /// <summary>
    /// Gets or sets a value indicating whether the imported
    /// product structure must be analyzed automatically.
    /// </summary>
    /// <remarks>
    /// When enabled, the importer may invoke the product
    /// structure analyzer after the domain model has been
    /// created.
    /// </remarks>
    [XmlAttribute("analyzeProductStructure")]
    public bool AnalyzeProductStructure
    {
        get => _analyzeProductStructure;
        set => _analyzeProductStructure = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the imported
    /// problem must be classified automatically.
    /// </summary>
    /// <remarks>
    /// Problem classification normally requires a valid
    /// imported instance and may depend on product-structure
    /// analysis.
    /// </remarks>
    [XmlAttribute("classifyProblem")]
    public bool ClassifyProblem
    {
        get => _classifyProblem;
        set => _classifyProblem = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether solution-method
    /// recommendations must be generated after import.
    /// </summary>
    /// <remarks>
    /// This option should normally remain disabled during
    /// basic imports because recommendation generation may
    /// require a valid classification and a method catalog.
    /// </remarks>
    [XmlAttribute("generateMethodRecommendations")]
    public bool GenerateMethodRecommendations
    {
        get => _generateMethodRecommendations;
        set => _generateMethodRecommendations = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the source data
    /// must be validated before domain-object creation.
    /// </summary>
    [XmlAttribute("validateSourceData")]
    public bool ValidateSourceData
    {
        get => _validateSourceData;
        set => _validateSourceData = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the resulting
    /// lot-sizing instance must be validated after conversion.
    /// </summary>
    [XmlAttribute("validateImportedInstance")]
    public bool ValidateImportedInstance
    {
        get => _validateImportedInstance;
        set => _validateImportedInstance = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether blocking import
    /// diagnostics must be converted into exceptions.
    /// </summary>
    /// <remarks>
    /// When disabled, errors are returned through the import
    /// result and its diagnostic collection.
    /// </remarks>
    [XmlAttribute("throwOnError")]
    public bool ThrowOnError
    {
        get => _throwOnError;
        set => _throwOnError = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether identifiers
    /// provided by the source data must be preserved.
    /// </summary>
    /// <remarks>
    /// Preserving source identifiers is strongly recommended
    /// for benchmark instances because published results are
    /// generally indexed with the original identifiers.
    /// </remarks>
    [XmlAttribute("preserveSourceIdentifiers")]
    public bool PreserveSourceIdentifiers
    {
        get => _preserveSourceIdentifiers;
        set => _preserveSourceIdentifiers = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether informational
    /// diagnostics must be included in the import result.
    /// </summary>
    [XmlAttribute("includeInformationDiagnostics")]
    public bool IncludeInformationDiagnostics
    {
        get => _includeInformationDiagnostics;
        set => _includeInformationDiagnostics = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether technical
    /// exception details may be copied into diagnostics.
    /// </summary>
    /// <remarks>
    /// This option should normally be disabled in user-facing
    /// applications and enabled only for debugging or detailed
    /// logs.
    /// </remarks>
    [XmlAttribute("includeTechnicalDetails")]
    public bool IncludeTechnicalDetails
    {
        get => _includeTechnicalDetails;
        set => _includeTechnicalDetails = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether import
    /// processing must stop immediately after the first
    /// blocking error.
    /// </summary>
    [XmlAttribute("stopOnFirstError")]
    public bool StopOnFirstError
    {
        get => _stopOnFirstError;
        set => _stopOnFirstError = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether a partially
    /// constructed instance may be returned when non-fatal
    /// errors are encountered.
    /// </summary>
    /// <remarks>
    /// Partial imports are useful for diagnostics and data
    /// repair, but they should not normally be used for
    /// optimization.
    /// </remarks>
    [XmlAttribute("allowPartialImport")]
    public bool AllowPartialImport
    {
        get => _allowPartialImport;
        set => _allowPartialImport = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether textual values
    /// must be normalized during import.
    /// </summary>
    /// <remarks>
    /// Normalization may include trimming leading and trailing
    /// white-space and replacing null text values with empty
    /// strings.
    /// </remarks>
    [XmlAttribute("normalizeTextValues")]
    public bool NormalizeTextValues
    {
        get => _normalizeTextValues;
        set => _normalizeTextValues = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether source
    /// identifiers must be trimmed before validation and
    /// conversion.
    /// </summary>
    [XmlAttribute("trimIdentifiers")]
    public bool TrimIdentifiers
    {
        get => _trimIdentifiers;
        set => _trimIdentifiers = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether duplicate
    /// source identifiers must be rejected.
    /// </summary>
    [XmlAttribute("rejectDuplicateIdentifiers")]
    public bool RejectDuplicateIdentifiers
    {
        get => _rejectDuplicateIdentifiers;
        set => _rejectDuplicateIdentifiers = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether references to
    /// missing source entities must be rejected.
    /// </summary>
    [XmlAttribute("rejectMissingReferences")]
    public bool RejectMissingReferences
    {
        get => _rejectMissingReferences;
        set => _rejectMissingReferences = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether negative
    /// quantities, costs and demand values must be rejected.
    /// </summary>
    [XmlAttribute("rejectNegativeValues")]
    public bool RejectNegativeValues
    {
        get => _rejectNegativeValues;
        set => _rejectNegativeValues = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether time series
    /// whose length differs from the planning horizon must be
    /// rejected.
    /// </summary>
    [XmlAttribute("rejectInvalidTimeSeriesLength")]
    public bool RejectInvalidTimeSeriesLength
    {
        get => _rejectInvalidTimeSeriesLength;
        set => _rejectInvalidTimeSeriesLength = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the import
    /// service may inspect document content to detect its
    /// format automatically.
    /// </summary>
    [XmlAttribute("detectFormatFromContent")]
    public bool DetectFormatFromContent
    {
        get => _detectFormatFromContent;
        set => _detectFormatFromContent = value;
    }

    /// <summary>
    /// Gets or sets an optional human-readable source name.
    /// </summary>
    /// <remarks>
    /// Examples include a benchmark collection name, an
    /// industrial system name or a data provider.
    /// </remarks>
    [XmlElement("sourceName")]
    public string SourceName
    {
        get => _sourceName;
        set =>
            _sourceName =
                value?.Trim() ??
                string.Empty;
    }

    /// <summary>
    /// Gets or sets an optional identifier that replaces the
    /// identifier provided by the source data.
    /// </summary>
    [XmlElement("instanceIdOverride")]
    public string InstanceIdOverride
    {
        get => _instanceIdOverride;
        set =>
            _instanceIdOverride =
                value?.Trim() ??
                string.Empty;
    }

    /// <summary>
    /// Gets or sets an optional name that replaces the
    /// instance name provided by the source data.
    /// </summary>
    [XmlElement("instanceNameOverride")]
    public string InstanceNameOverride
    {
        get => _instanceNameOverride;
        set =>
            _instanceNameOverride =
                value?.Trim() ??
                string.Empty;
    }

    /// <summary>
    /// Gets or sets optional creator information that replaces
    /// the value inferred from the source data.
    /// </summary>
    [XmlElement("createdByOverride")]
    public string CreatedByOverride
    {
        get => _createdByOverride;
        set =>
            _createdByOverride =
                value?.Trim() ??
                string.Empty;
    }

    /// <summary>
    /// Gets or sets optional source information that replaces
    /// the value inferred from the imported document.
    /// </summary>
    [XmlElement("sourceInformationOverride")]
    public string SourceInformationOverride
    {
        get => _sourceInformationOverride;
        set =>
            _sourceInformationOverride =
                value?.Trim() ??
                string.Empty;
    }

    /// <summary>
    /// Gets a value indicating whether a source name has been
    /// specified.
    /// </summary>
    [XmlIgnore]
    public bool HasSourceName =>
        !string.IsNullOrWhiteSpace(
            SourceName);

    /// <summary>
    /// Gets a value indicating whether the source instance
    /// identifier must be overridden.
    /// </summary>
    [XmlIgnore]
    public bool HasInstanceIdOverride =>
        !string.IsNullOrWhiteSpace(
            InstanceIdOverride);

    /// <summary>
    /// Gets a value indicating whether the source instance
    /// name must be overridden.
    /// </summary>
    [XmlIgnore]
    public bool HasInstanceNameOverride =>
        !string.IsNullOrWhiteSpace(
            InstanceNameOverride);

    /// <summary>
    /// Gets a value indicating whether creator information
    /// must be overridden.
    /// </summary>
    [XmlIgnore]
    public bool HasCreatedByOverride =>
        !string.IsNullOrWhiteSpace(
            CreatedByOverride);

    /// <summary>
    /// Gets a value indicating whether source information
    /// must be overridden.
    /// </summary>
    [XmlIgnore]
    public bool HasSourceInformationOverride =>
        !string.IsNullOrWhiteSpace(
            SourceInformationOverride);

    /// <summary>
    /// Gets a value indicating whether the configured
    /// post-import workflow is internally coherent.
    /// </summary>
    /// <remarks>
    /// Method recommendations normally require problem
    /// classification. Problem classification normally
    /// benefits from product-structure analysis.
    /// </remarks>
    [XmlIgnore]
    public bool HasCoherentPostImportWorkflow =>
        !GenerateMethodRecommendations ||
        ClassifyProblem;

    /// <summary>
    /// Validates the option combination.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when the option combination is
    /// coherent; otherwise, <see langword="false"/>.
    /// </returns>
    public virtual bool Validate()
    {
        if (!HasCoherentPostImportWorkflow)
        {
            return false;
        }

        if (AllowPartialImport &&
            ThrowOnError)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Validates the option combination and throws an
    /// exception when it is inconsistent.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the option combination is inconsistent.
    /// </exception>
    public virtual void EnsureValid()
    {
        if (GenerateMethodRecommendations &&
            !ClassifyProblem)
        {
            throw new InvalidOperationException(
                "Solution-method recommendation generation " +
                "requires problem classification.");
        }

        if (AllowPartialImport &&
            ThrowOnError)
        {
            throw new InvalidOperationException(
                "Partial imports cannot be enabled when " +
                "blocking diagnostics are configured to " +
                "throw exceptions.");
        }
    }

    /// <summary>
    /// Creates a shallow copy of the current options.
    /// </summary>
    /// <returns>
    /// New option object containing the same values.
    /// </returns>
    public virtual InstanceImportOptions Clone()
    {
        return new InstanceImportOptions
        {
            AnalyzeProductStructure =
                AnalyzeProductStructure,

            ClassifyProblem =
                ClassifyProblem,

            GenerateMethodRecommendations =
                GenerateMethodRecommendations,

            ValidateSourceData =
                ValidateSourceData,

            ValidateImportedInstance =
                ValidateImportedInstance,

            ThrowOnError =
                ThrowOnError,

            PreserveSourceIdentifiers =
                PreserveSourceIdentifiers,

            IncludeInformationDiagnostics =
                IncludeInformationDiagnostics,

            IncludeTechnicalDetails =
                IncludeTechnicalDetails,

            StopOnFirstError =
                StopOnFirstError,

            AllowPartialImport =
                AllowPartialImport,

            NormalizeTextValues =
                NormalizeTextValues,

            TrimIdentifiers =
                TrimIdentifiers,

            RejectDuplicateIdentifiers =
                RejectDuplicateIdentifiers,

            RejectMissingReferences =
                RejectMissingReferences,

            RejectNegativeValues =
                RejectNegativeValues,

            RejectInvalidTimeSeriesLength =
                RejectInvalidTimeSeriesLength,

            DetectFormatFromContent =
                DetectFormatFromContent,

            SourceName =
                SourceName,

            InstanceIdOverride =
                InstanceIdOverride,

            InstanceNameOverride =
                InstanceNameOverride,

            CreatedByOverride =
                CreatedByOverride,

            SourceInformationOverride =
                SourceInformationOverride
        };
    }

    /// <summary>
    /// Determines whether the source name must be serialized.
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
    /// Determines whether the instance identifier override
    /// must be serialized.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when an override exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool ShouldSerializeInstanceIdOverride()
    {
        return HasInstanceIdOverride;
    }

    /// <summary>
    /// Determines whether the instance name override must be
    /// serialized.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when an override exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool ShouldSerializeInstanceNameOverride()
    {
        return HasInstanceNameOverride;
    }

    /// <summary>
    /// Determines whether the creator override must be
    /// serialized.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when an override exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool ShouldSerializeCreatedByOverride()
    {
        return HasCreatedByOverride;
    }

    /// <summary>
    /// Determines whether the source-information override
    /// must be serialized.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when an override exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool ShouldSerializeSourceInformationOverride()
    {
        return HasSourceInformationOverride;
    }
}