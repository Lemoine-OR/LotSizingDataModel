using System;
using System.Xml.Serialization;
using LotSizingDataModel.Import.Common;

namespace LotSizingDataModel.Import.DellaertJeunet;

/// <summary>
/// Defines the options specific to the import of Dellaert and
/// Jeunet XML benchmark instances.
/// </summary>
/// <remarks>
/// The Dellaert–Jeunet XML format represents:
/// <list type="bullet">
/// <item>
/// <description>
/// the planning horizon through <c>NBPeriods</c>;
/// </description>
/// </item>
/// <item>
/// <description>
/// external demand through a sequence of <c>int</c> elements;
/// </description>
/// </item>
/// <item>
/// <description>
/// setup, holding and production costs at item level;
/// </description>
/// </item>
/// <item>
/// <description>
/// product-structure relationships through
/// <c>ListOfComponents</c>;
/// </description>
/// </item>
/// <item>
/// <description>
/// the declared bill-of-material depth through
/// <c>DepthInBOM</c>.
/// </description>
/// </item>
/// </list>
///
/// This class extends the common import options with behavior
/// specific to that source format.
/// </remarks>
[Serializable]
[XmlRoot("dellaertJeunetImportOptions")]
[XmlType(TypeName = "dellaertJeunetImportOptions")]
public sealed class DellaertJeunetImportOptions :
    InstanceImportOptions
{
    private bool _convertEmptyDemandToZeroSeries =
        true;

    private bool _verifyDeclaredDepth =
        true;

    private bool _verifyDeclaredBomType =
        true;

    private bool _preserveBibliographicMetadata =
        true;

    private bool _requireBibliographicMetadata;

    private bool _requireContiguousItemIdentifiers;

    private bool _verifyItemIdentifierOrder =
        true;

    private bool _rejectSelfReferences =
        true;

    private bool _rejectDuplicateComponentRelationships =
        true;

    private bool _verifyAcyclicProductStructure =
        true;

    private bool _requirePositiveComponentQuantities =
        true;

    private bool _requireNonEmptyItemNames;

    private bool _treatEmptyComponentListAsLeaf =
        true;

    private bool _allowDemandOnNonRootItems;

    private bool _normalizeDeclaredBomType =
        true;

    private string _itemIdentifierPrefix =
        string.Empty;

    /// <summary>
    /// Initializes a new instance of the Dellaert–Jeunet
    /// import options class with the recommended default
    /// values.
    /// </summary>
    public DellaertJeunetImportOptions()
    {
        SourceName =
            "Dellaert–Jeunet benchmark collection";

        PreserveSourceIdentifiers =
            true;

        AnalyzeProductStructure =
            true;

        ClassifyProblem =
            true;

        ValidateSourceData =
            true;

        ValidateImportedInstance =
            true;
    }

    /// <summary>
    /// Gets or sets a value indicating whether an empty
    /// external-demand element must be converted into a
    /// zero-filled time series whose length equals the
    /// planning horizon.
    /// </summary>
    /// <remarks>
    /// In Dellaert–Jeunet files, an empty <c>Demand</c>
    /// element normally indicates that the item has no
    /// external demand. Its requirements are induced by its
    /// parent items.
    ///
    /// When this option is enabled, an empty demand is
    /// converted into one zero value for each planning period.
    /// </remarks>
    [XmlAttribute("convertEmptyDemandToZeroSeries")]
    public bool ConvertEmptyDemandToZeroSeries
    {
        get =>
            _convertEmptyDemandToZeroSeries;

        set =>
            _convertEmptyDemandToZeroSeries =
                value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the
    /// <c>DepthInBOM</c> value declared for each item must be
    /// compared with the depth computed from the imported
    /// product structure.
    /// </summary>
    [XmlAttribute("verifyDeclaredDepth")]
    public bool VerifyDeclaredDepth
    {
        get =>
            _verifyDeclaredDepth;

        set =>
            _verifyDeclaredDepth =
                value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the declared
    /// <c>BOMType</c> must be compared with the product
    /// structure type detected after import.
    /// </summary>
    [XmlAttribute("verifyDeclaredBomType")]
    public bool VerifyDeclaredBomType
    {
        get =>
            _verifyDeclaredBomType;

        set =>
            _verifyDeclaredBomType =
                value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether bibliographic
    /// information stored in the source XML document must be
    /// preserved in the imported instance metadata.
    /// </summary>
    [XmlAttribute("preserveBibliographicMetadata")]
    public bool PreserveBibliographicMetadata
    {
        get =>
            _preserveBibliographicMetadata;

        set =>
            _preserveBibliographicMetadata =
                value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether bibliographic
    /// information is mandatory for a valid source document.
    /// </summary>
    /// <remarks>
    /// This option is disabled by default because benchmark
    /// files may remain valid even when their article metadata
    /// is incomplete.
    /// </remarks>
    [XmlAttribute("requireBibliographicMetadata")]
    public bool RequireBibliographicMetadata
    {
        get =>
            _requireBibliographicMetadata;

        set =>
            _requireBibliographicMetadata =
                value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether item
    /// identifiers must form a contiguous integer sequence.
    /// </summary>
    /// <remarks>
    /// The supplied benchmark instance uses identifiers from
    /// 1 to 500 without gaps. This property is useful for
    /// strict benchmark verification, but contiguous
    /// identifiers are not required by the generic import
    /// architecture.
    /// </remarks>
    [XmlAttribute("requireContiguousItemIdentifiers")]
    public bool RequireContiguousItemIdentifiers
    {
        get =>
            _requireContiguousItemIdentifiers;

        set =>
            _requireContiguousItemIdentifiers =
                value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the physical
    /// order of item elements must be checked against their
    /// numeric identifiers.
    /// </summary>
    /// <remarks>
    /// An ordering mismatch should normally generate a warning
    /// rather than invalidate the imported instance.
    /// </remarks>
    [XmlAttribute("verifyItemIdentifierOrder")]
    public bool VerifyItemIdentifierOrder
    {
        get =>
            _verifyItemIdentifierOrder;

        set =>
            _verifyItemIdentifierOrder =
                value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether a product
    /// structure relationship in which an item references
    /// itself as a component must be rejected.
    /// </summary>
    [XmlAttribute("rejectSelfReferences")]
    public bool RejectSelfReferences
    {
        get =>
            _rejectSelfReferences;

        set =>
            _rejectSelfReferences =
                value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether duplicate
    /// component relationships for the same parent item must
    /// be rejected.
    /// </summary>
    [XmlAttribute("rejectDuplicateComponentRelationships")]
    public bool RejectDuplicateComponentRelationships
    {
        get =>
            _rejectDuplicateComponentRelationships;

        set =>
            _rejectDuplicateComponentRelationships =
                value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the imported
    /// product structure must be checked for cycles.
    /// </summary>
    /// <remarks>
    /// A cyclic product structure is not valid for the
    /// classical multilevel lot-sizing benchmark represented
    /// by this format.
    /// </remarks>
    [XmlAttribute("verifyAcyclicProductStructure")]
    public bool VerifyAcyclicProductStructure
    {
        get =>
            _verifyAcyclicProductStructure;

        set =>
            _verifyAcyclicProductStructure =
                value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether each component
    /// quantity must be strictly positive.
    /// </summary>
    [XmlAttribute("requirePositiveComponentQuantities")]
    public bool RequirePositiveComponentQuantities
    {
        get =>
            _requirePositiveComponentQuantities;

        set =>
            _requirePositiveComponentQuantities =
                value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether every source
    /// item must have a non-empty name.
    /// </summary>
    /// <remarks>
    /// When disabled, the importer may generate an item name
    /// from its source identifier.
    /// </remarks>
    [XmlAttribute("requireNonEmptyItemNames")]
    public bool RequireNonEmptyItemNames
    {
        get =>
            _requireNonEmptyItemNames;

        set =>
            _requireNonEmptyItemNames =
                value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether an empty
    /// <c>ListOfComponents</c> element must be interpreted as
    /// a leaf item in the product structure.
    /// </summary>
    [XmlAttribute("treatEmptyComponentListAsLeaf")]
    public bool TreatEmptyComponentListAsLeaf
    {
        get =>
            _treatEmptyComponentListAsLeaf;

        set =>
            _treatEmptyComponentListAsLeaf =
                value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether external demand
    /// is allowed on items that are not roots of the product
    /// structure.
    /// </summary>
    /// <remarks>
    /// The classical benchmark normally places external demand
    /// on finished products only. Enabling this option permits
    /// more general data variants.
    /// </remarks>
    [XmlAttribute("allowDemandOnNonRootItems")]
    public bool AllowDemandOnNonRootItems
    {
        get =>
            _allowDemandOnNonRootItems;

        set =>
            _allowDemandOnNonRootItems =
                value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the text stored
    /// in <c>BOMType</c> must be normalized before being
    /// interpreted.
    /// </summary>
    /// <remarks>
    /// Normalization may include trimming white-space and
    /// case-insensitive comparison.
    /// </remarks>
    [XmlAttribute("normalizeDeclaredBomType")]
    public bool NormalizeDeclaredBomType
    {
        get =>
            _normalizeDeclaredBomType;

        set =>
            _normalizeDeclaredBomType =
                value;
    }

    /// <summary>
    /// Gets or sets an optional prefix applied to imported
    /// item identifiers.
    /// </summary>
    /// <remarks>
    /// When the prefix is empty and source identifiers are
    /// preserved, item identifier <c>12</c> remains
    /// <c>12</c>.
    ///
    /// With prefix <c>DJ40-I</c>, the same identifier may be
    /// converted to <c>DJ40-I12</c>.
    /// </remarks>
    [XmlElement("itemIdentifierPrefix")]
    public string ItemIdentifierPrefix
    {
        get =>
            _itemIdentifierPrefix;

        set =>
            _itemIdentifierPrefix =
                value?.Trim() ??
                string.Empty;
    }

    /// <summary>
    /// Gets a value indicating whether an item identifier
    /// prefix has been configured.
    /// </summary>
    [XmlIgnore]
    public bool HasItemIdentifierPrefix =>
        !string.IsNullOrWhiteSpace(
            ItemIdentifierPrefix);

    /// <summary>
    /// Validates the option combination.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when the option combination is
    /// coherent; otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Validate()
    {
        if (!base.Validate())
        {
            return false;
        }

        if (HasItemIdentifierPrefix &&
            PreserveSourceIdentifiers)
        {
            return false;
        }

        if (VerifyDeclaredDepth &&
            !AnalyzeProductStructure)
        {
            return false;
        }

        if (VerifyDeclaredBomType &&
            !AnalyzeProductStructure)
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
    public override void EnsureValid()
    {
        base.EnsureValid();

        if (HasItemIdentifierPrefix &&
            PreserveSourceIdentifiers)
        {
            throw new InvalidOperationException(
                "An item identifier prefix cannot be applied " +
                "while source identifiers are preserved.");
        }

        if (VerifyDeclaredDepth &&
            !AnalyzeProductStructure)
        {
            throw new InvalidOperationException(
                "Declared bill-of-material depth verification " +
                "requires product-structure analysis.");
        }

        if (VerifyDeclaredBomType &&
            !AnalyzeProductStructure)
        {
            throw new InvalidOperationException(
                "Declared bill-of-material type verification " +
                "requires product-structure analysis.");
        }
    }

    /// <summary>
    /// Creates a copy of the current Dellaert–Jeunet import
    /// options.
    /// </summary>
    /// <returns>
    /// New option object containing the same values.
    /// </returns>
    public override InstanceImportOptions Clone()
    {
        return new DellaertJeunetImportOptions
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
                SourceInformationOverride,

            ConvertEmptyDemandToZeroSeries =
                ConvertEmptyDemandToZeroSeries,

            VerifyDeclaredDepth =
                VerifyDeclaredDepth,

            VerifyDeclaredBomType =
                VerifyDeclaredBomType,

            PreserveBibliographicMetadata =
                PreserveBibliographicMetadata,

            RequireBibliographicMetadata =
                RequireBibliographicMetadata,

            RequireContiguousItemIdentifiers =
                RequireContiguousItemIdentifiers,

            VerifyItemIdentifierOrder =
                VerifyItemIdentifierOrder,

            RejectSelfReferences =
                RejectSelfReferences,

            RejectDuplicateComponentRelationships =
                RejectDuplicateComponentRelationships,

            VerifyAcyclicProductStructure =
                VerifyAcyclicProductStructure,

            RequirePositiveComponentQuantities =
                RequirePositiveComponentQuantities,

            RequireNonEmptyItemNames =
                RequireNonEmptyItemNames,

            TreatEmptyComponentListAsLeaf =
                TreatEmptyComponentListAsLeaf,

            AllowDemandOnNonRootItems =
                AllowDemandOnNonRootItems,

            NormalizeDeclaredBomType =
                NormalizeDeclaredBomType,

            ItemIdentifierPrefix =
                ItemIdentifierPrefix
        };
    }

    /// <summary>
    /// Builds the target identifier corresponding to a source
    /// item identifier.
    /// </summary>
    /// <param name="sourceIdentifier">
    /// Source item identifier.
    /// </param>
    /// <returns>
    /// Identifier to use in the imported domain model.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="sourceIdentifier"/> is
    /// empty.
    /// </exception>
    public string BuildTargetItemIdentifier(
        string sourceIdentifier)
    {
        if (string.IsNullOrWhiteSpace(
                sourceIdentifier))
        {
            throw new ArgumentException(
                "A source item identifier is required.",
                nameof(sourceIdentifier));
        }

        string normalizedIdentifier =
            TrimIdentifiers
                ? sourceIdentifier.Trim()
                : sourceIdentifier;

        if (PreserveSourceIdentifiers)
        {
            return normalizedIdentifier;
        }

        if (HasItemIdentifierPrefix)
        {
            return
                ItemIdentifierPrefix +
                normalizedIdentifier;
        }

        return
            "ITEM-" +
            normalizedIdentifier;
    }

    /// <summary>
    /// Determines whether the item identifier prefix must be
    /// serialized.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when a prefix exists; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public bool ShouldSerializeItemIdentifierPrefix()
    {
        return HasItemIdentifierPrefix;
    }
}